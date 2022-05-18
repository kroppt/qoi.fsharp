namespace Qoi.Fsharp

module Encoder =
    open System.IO

    type public Channels =
        | Rgb = 3
        | Rgba = 4

    type public ColorSpace =
        | SRgb = 0
        | Linear = 1

    [<Struct>]
    type Pixel = { R: byte; G: byte; B: byte; A: byte }

    type Diff =
        struct
            val R: byte
            val G: byte
            val B: byte

            new(prev: Pixel, next: Pixel) =
                { R = next.R - prev.R + 2uy
                  G = next.G - prev.G + 2uy
                  B = next.B - prev.B + 2uy }

            member this.IsSmall() =
                this.R <= 3uy && this.G <= 3uy && this.B <= 3uy
        end

    type public Encoder =
        private new(binWriter: BinaryWriter,
                    input: byte list,
                    width: int,
                    height: int,
                    channels: Channels,
                    colorSpace: ColorSpace) =
            { binWriter = binWriter
              input = input
              width = width
              height = height
              channels = channels
              colorSpace = colorSpace
              cache = Array.zeroCreate 64 }

        val binWriter: BinaryWriter
        val input: byte list
        val width: int
        val height: int
        val channels: Channels
        val colorSpace: ColorSpace
        val cache: Pixel []

        static member public Encode
            (
                input: byte list,
                width: int,
                height: int,
                channels: Channels,
                colorSpace: ColorSpace
            ) : byte [] =
            using (new MemoryStream()) (fun memStream ->
                using (new BinaryWriter(memStream)) (fun binWriter ->
                    let encoder = new Encoder(binWriter, input, width, height, channels, colorSpace)
                    encoder.Encode())

                memStream.ToArray())

        member private this.writeBigEndian(value: int) =
            this.binWriter.Write(byte ((value >>> 24) &&& 0xFF))
            this.binWriter.Write(byte ((value >>> 16) &&& 0xFF))
            this.binWriter.Write(byte ((value >>> 8) &&& 0xFF))
            this.binWriter.Write(byte ((value >>> 0) &&& 0xFF))

        member private this.WriteHeader() =
            this.binWriter.Write(byte 'q')
            this.binWriter.Write(byte 'o')
            this.binWriter.Write(byte 'i')
            this.binWriter.Write(byte 'f')
            this.writeBigEndian (this.width)
            this.writeBigEndian (this.height)
            this.binWriter.Write(byte this.channels)
            this.binWriter.Write(byte this.colorSpace)

        member private this.WriteRgbaChunk(pixel: Pixel) =
            this.binWriter.Write(0b11111111uy)
            this.binWriter.Write(pixel.R)
            this.binWriter.Write(pixel.G)
            this.binWriter.Write(pixel.B)
            this.binWriter.Write(pixel.A)

        member private this.WriteRgbChunk(pixel: Pixel) =
            this.binWriter.Write(0b11111110uy)
            this.binWriter.Write(pixel.R)
            this.binWriter.Write(pixel.G)
            this.binWriter.Write(pixel.B)

        member private this.WriteIndexChunk(index: int) = this.binWriter.Write(byte index)

        member private this.WriteDiffChunk(diff: Diff) =
            let chunk =
                0b01_000000uy
                ||| (diff.R <<< 4)
                ||| (diff.G <<< 2)
                ||| (diff.B <<< 0)

            this.binWriter.Write(chunk)

        member private _.CalculateIndex(pixel: Pixel) =
            int (
                pixel.R * 3uy
                + pixel.G * 5uy
                + pixel.B * 7uy
                + pixel.A * 11uy
            ) % 64

        member private this.WriteChunks() =
            let mutable prev = { R = 0uy; G = 0uy; B = 0uy; A = 255uy }

            this.input
            |> List.chunkBySize 4
            |> List.iter (fun bytes ->
                let pixel =
                    { R = bytes[0]
                      G = bytes[1]
                      B = bytes[2]
                      A = bytes[3] }

                let index = this.CalculateIndex(pixel)

                if this.cache[index] = pixel then
                    this.WriteIndexChunk(index)
                elif pixel.A = prev.A then
                    let diff = Diff(prev, pixel)

                    if diff.IsSmall() then
                        this.WriteDiffChunk(diff)
                    else
                        this.WriteRgbChunk(pixel)

                    this.cache[ index ] <- pixel
                else
                    this.WriteRgbaChunk(pixel)
                    this.cache[ index ] <- pixel

                prev <- pixel)

        member private this.WriteFooter() =
            this.binWriter.Write(byte 0)
            this.binWriter.Write(byte 0)
            this.binWriter.Write(byte 0)
            this.binWriter.Write(byte 0)
            this.binWriter.Write(byte 0)
            this.binWriter.Write(byte 0)
            this.binWriter.Write(byte 0)
            this.binWriter.Write(byte 1)

        member private this.Encode() =
            this.WriteHeader()
            this.WriteChunks()
            this.WriteFooter()

namespace Qoi.Fsharp

module Encoder =
    open System.IO

    type public Channels =
        | Rgb = 3
        | Rgba = 4

    type public ColorSpace =
        | SRgb = 0
        | Linear = 1

    type Pixel = { R: byte; G: byte; B: byte; A: byte }

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
              colorSpace = colorSpace }

        val binWriter: BinaryWriter
        val input: byte list
        val width: int
        val height: int
        val channels: Channels
        val colorSpace: ColorSpace

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

        member private this.WriteChunks() =
            let prev = { R = 0uy; G = 0uy; B = 0uy; A = 255uy }

            this.input
            |> List.chunkBySize 4
            |> List.iter (fun bytes ->
                let pixel =
                    { R = bytes[0]
                      G = bytes[1]
                      B = bytes[2]
                      A = bytes[3] }

                if pixel.A = prev.A then
                    this.WriteRgbChunk(pixel)
                else
                    this.WriteRgbaChunk(pixel))

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

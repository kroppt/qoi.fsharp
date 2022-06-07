namespace Qoi.Fsharp

module Encoder =
    open System.IO

    open Qoi.Fsharp.Header

    [<Struct>]
    type private Pixel = { R: byte; G: byte; B: byte; A: byte }

    [<Struct>]
    type private Diff =
        val R: byte
        val G: byte
        val B: byte

        new(prev: Pixel, next: Pixel) =
            { R = next.R - prev.R + 2uy
              G = next.G - prev.G + 2uy
              B = next.B - prev.B + 2uy }

        member this.IsSmall() =
            this.R <= 3uy && this.G <= 3uy && this.B <= 3uy

    [<Struct>]
    type private LumaDiff =
        val G: byte
        val RG: byte
        val BG: byte

        new(prev: Pixel, next: Pixel) =
            let dr = next.R - prev.R
            let dg = next.G - prev.G
            let db = next.B - prev.B

            { G = dg + 32uy
              RG = dr - dg + 8uy
              BG = db - dg + 8uy }

        member this.IsSmall() =
            this.G <= 63uy
            && this.RG <= 15uy
            && this.BG <= 15uy

    [<Class>]
    type private Encoder
        (
            binWriter: BinaryWriter,
            input: byte array,
            width: int,
            height: int,
            channels: Channels,
            colorSpace: ColorSpace
        ) =
        let mutable prev = { R = 0uy; G = 0uy; B = 0uy; A = 255uy }
        let mutable runLength = 0uy
        let cache: Pixel [] = Array.zeroCreate 64

        member private _.writeBigEndian(value: int) =
            binWriter.Write(byte ((value >>> 24) &&& 0xFF))
            binWriter.Write(byte ((value >>> 16) &&& 0xFF))
            binWriter.Write(byte ((value >>> 8) &&& 0xFF))
            binWriter.Write(byte ((value >>> 0) &&& 0xFF))

        member private this.WriteHeader() =
            binWriter.Write(byte 'q')
            binWriter.Write(byte 'o')
            binWriter.Write(byte 'i')
            binWriter.Write(byte 'f')
            this.writeBigEndian (width)
            this.writeBigEndian (height)
            binWriter.Write(byte channels)
            binWriter.Write(byte colorSpace)

        member private _.WriteRgbaChunk(pixel: Pixel) =
            binWriter.Write(Tag.Rgba)
            binWriter.Write(pixel.R)
            binWriter.Write(pixel.G)
            binWriter.Write(pixel.B)
            binWriter.Write(pixel.A)

        member private _.WriteRgbChunk(pixel: Pixel) =
            binWriter.Write(Tag.Rgb)
            binWriter.Write(pixel.R)
            binWriter.Write(pixel.G)
            binWriter.Write(pixel.B)

        member private _.WriteIndexChunk(index: int) = binWriter.Write(byte index)

        member private _.WriteDiffChunk(diff: Diff) =
            let chunk =
                Tag.Diff
                ||| (diff.R <<< 4)
                ||| (diff.G <<< 2)
                ||| (diff.B <<< 0)

            binWriter.Write(chunk)

        member private _.WriteLumaChunk(lumaDiff: LumaDiff) =
            let chunk1 = Tag.Luma ||| (lumaDiff.G <<< 8)
            let chunk2 = (lumaDiff.RG <<< 4) ||| lumaDiff.BG

            binWriter.Write(chunk1)
            binWriter.Write(chunk2)

        member private _.WriteRunChunk() =
            let chunk = Tag.Run ||| (runLength - 1uy)
            binWriter.Write(chunk)

        member private _.CalculateIndex(pixel: Pixel) =
            int (
                pixel.R * 3uy
                + pixel.G * 5uy
                + pixel.B * 7uy
                + pixel.A * 11uy
            ) % 64

        member private this.WriteChunks() =
            input
            |> Array.chunkBySize (
                match channels with
                | Channels.Rgb -> 3
                | Channels.Rgba -> 4
            )
            |> Array.map (fun bytes ->
                { R = bytes[0]
                  G = bytes[1]
                  B = bytes[2]
                  A =
                    match channels with
                    | Channels.Rgb -> 255uy
                    | Channels.Rgba -> bytes[3] })
            |> Array.iter (fun pixel -> this.WriteChunk(pixel))

        member private this.WriteChunk(pixel) =
            let index = this.CalculateIndex(pixel)

            if prev = pixel && runLength < 62uy then
                runLength <- runLength + 1uy
                cache[index] <- pixel
            elif runLength > 0uy then
                this.WriteRunChunk()
                runLength <- 0uy
                this.WriteChunk(pixel)
            elif cache[index] = pixel then
                this.WriteIndexChunk(index)
            elif pixel.A = prev.A then
                let diff = Diff(prev, pixel)
                let lumaDiff = LumaDiff(prev, pixel)

                if diff.IsSmall() then
                    this.WriteDiffChunk(diff)
                elif lumaDiff.IsSmall() then
                    this.WriteLumaChunk(lumaDiff)
                else
                    this.WriteRgbChunk(pixel)

                cache[index] <- pixel
            else
                this.WriteRgbaChunk(pixel)
                cache[index] <- pixel

            prev <- pixel

        member private _.WriteFooter() =
            binWriter.Write(byte 0)
            binWriter.Write(byte 0)
            binWriter.Write(byte 0)
            binWriter.Write(byte 0)
            binWriter.Write(byte 0)
            binWriter.Write(byte 0)
            binWriter.Write(byte 0)
            binWriter.Write(byte 1)

        member public this.Encode() =
            this.WriteHeader()
            this.WriteChunks()

            if runLength > 0uy then
                this.WriteRunChunk()

            this.WriteFooter()

    let public Encode
        (input: byte array)
        (width: int)
        (height: int)
        (channels: Channels)
        (colorSpace: ColorSpace)
        : byte [] =
        using (new MemoryStream()) (fun memStream ->
            using (new BinaryWriter(memStream)) (fun binWriter ->
                Encoder(binWriter, input, width, height, channels, colorSpace)
                    .Encode())

            memStream.ToArray())

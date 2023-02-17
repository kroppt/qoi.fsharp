namespace Qoi.Fsharp

module Decoder =
    open System.IO

    open Qoi.Fsharp.Header

    [<Struct>]
    type public Image =
        { Width: uint
          Height: uint
          Channels: Channels
          ColorSpace: ColorSpace
          Bytes: byte array }

    [<Struct>]
    type public DecodeError =
        | BadMagicBytes
        | BadChannelsValue
        | BadColorSpaceValue
        | MissingEndMarker
        | IncorrectEndMarker

    [<Struct>]
    type private Pixel = { R: byte; G: byte; B: byte; A: byte }

    exception DecodeException of DecodeError

    let private decode (binReader: BinaryReader) : Image =
        let readBigEndian () =
            uint (binReader.ReadByte() <<< 0o30)
            ||| uint (binReader.ReadByte() <<< 0o20)
            ||| uint (binReader.ReadByte() <<< 0o10)
            ||| uint (binReader.ReadByte() <<< 0o00)

        let parseMagic () =
            let correctMagic = [| byte 'q'; byte 'o'; byte 'i'; byte 'f' |]

            let actualMagic = binReader.ReadBytes(4)

            if actualMagic <> correctMagic then
                raise (DecodeException BadMagicBytes)

        let parseDimensions () =
            let width = readBigEndian ()
            let height = readBigEndian ()
            struct (width, height)

        let parseChannels () =
            let channels = binReader.ReadByte()
            let channels = Channels.ParseByte channels

            match channels with
            | None -> raise (DecodeException BadChannelsValue)
            | Some channels -> channels

        let parseColorSpace () =
            let colorSpace = binReader.ReadByte()
            let colorSpace = ColorSpace.ParseByte colorSpace

            match colorSpace with
            | None -> raise (DecodeException BadColorSpaceValue)
            | Some colorSpace -> colorSpace

        let parseChunks (binWriter: BinaryWriter) width height channels =
            let cache = Array.zeroCreate<Pixel> 64

            let calculateIndex pixel =
                int ((pixel.R * 3uy + pixel.G * 5uy + pixel.B * 7uy + pixel.A * 11uy) % 64uy)

            let mutable prev = { R = 0uy; G = 0uy; B = 0uy; A = 255uy }

            let mutable numWritten = 0u

            let writePixel pixel =
                match channels with
                | Channels.Rgb -> binWriter.Write [| pixel.R; pixel.G; pixel.B |]
                | Channels.Rgba -> binWriter.Write [| pixel.R; pixel.G; pixel.B; pixel.A |]

                cache[calculateIndex pixel] <- pixel
                prev <- pixel
                numWritten <- numWritten + 1u

            let parseChunk () =
                let tag = binReader.ReadByte()

                if tag = Tag.Rgb then
                    let r = binReader.ReadByte()
                    let g = binReader.ReadByte()
                    let b = binReader.ReadByte()
                    writePixel { R = r; G = g; B = b; A = 255uy }
                else if tag = Tag.Rgba then
                    let r = binReader.ReadByte()
                    let g = binReader.ReadByte()
                    let b = binReader.ReadByte()
                    let a = binReader.ReadByte()
                    writePixel { R = r; G = g; B = b; A = a }
                else if (tag &&& Tag.Mask) = Tag.Index then
                    let pixel = cache[int tag]
                    writePixel pixel
                else if (tag &&& Tag.Mask) = Tag.Diff then
                    let dr = ((tag &&& Tag.DiffR) >>> 4) - 2uy
                    let dg = ((tag &&& Tag.DiffG) >>> 2) - 2uy
                    let db = ((tag &&& Tag.DiffB) >>> 0) - 2uy

                    let pixel =
                        { R = prev.R + dr
                          G = prev.G + dg
                          B = prev.B + db
                          A = prev.A }

                    writePixel pixel
                else if (tag &&& Tag.Mask) = Tag.Luma then
                    let dg = (tag &&& Tag.LumaG >>> 0) - 32uy
                    let b = binReader.ReadByte()
                    let dr = (b &&& Tag.LumaRG >>> 4) - 8uy + dg
                    let db = (b &&& Tag.LumaBG >>> 0) - 8uy + dg

                    let pixel =
                        { R = prev.R + dr
                          G = prev.G + dg
                          B = prev.B + db
                          A = prev.A }

                    writePixel pixel
                else if (tag &&& Tag.Mask) = Tag.Run then
                    let run = (tag &&& ~~~Tag.Run) + 1uy

                    for _ in 1uy .. run do
                        writePixel prev

            while numWritten < width * height do
                parseChunk ()

        let parseEndMarker () =
            let correctEndMarker = [| 0uy; 0uy; 0uy; 0uy; 0uy; 0uy; 0uy; 1uy |]

            let actualEndMarker = binReader.ReadBytes(correctEndMarker.Length)

            if actualEndMarker.Length <> correctEndMarker.Length then
                raise (DecodeException MissingEndMarker)

            if actualEndMarker <> correctEndMarker then
                raise (DecodeException IncorrectEndMarker)

        let createImage width height channels colorSpace bytes =
            { Width = width
              Height = height
              Channels = channels
              ColorSpace = colorSpace
              Bytes = bytes }

        parseMagic ()

        let struct (width, height) = parseDimensions ()

        let channels = parseChannels ()

        let colorSpace = parseColorSpace ()

        let pixelSize =
            match channels with
            | Channels.Rgb -> 3u
            | Channels.Rgba -> 4u

        let outputSize = int (width * height * pixelSize)

        let bytes =
            using (new MemoryStream(outputSize)) (fun memStream ->
                using (new BinaryWriter(memStream)) (fun binWriter -> parseChunks binWriter width height channels)
                memStream.ToArray())

        parseEndMarker ()

        createImage width height channels colorSpace bytes

    let public Decode (input: byte array) : Result<Image, DecodeError> =
        using (new MemoryStream(input)) (fun memStream ->
            using (new BinaryReader(memStream)) (fun binReader ->
                try
                    Ok(decode binReader)
                with DecodeException error ->
                    Error(error)))

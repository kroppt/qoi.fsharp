namespace Qoi.Fsharp

module Decoder =
    open System.IO
    open Header

    type public Image =
        { Width: uint
          Height: uint
          Channels: Channels
          ColorSpace: ColorSpace
          Bytes: byte list }

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
            let correctMagic =
                [| byte 'q'
                   byte 'o'
                   byte 'i'
                   byte 'f' |]

            let actualMagic = binReader.ReadBytes(4)

            if actualMagic <> correctMagic then
                raise (DecodeException BadMagicBytes)

        let parseDimensions () =
            let width = readBigEndian ()
            let height = readBigEndian ()
            (width, height)

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

        let parseChunks width height channels =
            let mutable bytes: byte list = []

            let cache = Array.zeroCreate<Pixel> 64

            let calculateIndex pixel =
                int (
                    (pixel.R * 3uy
                     + pixel.G * 5uy
                     + pixel.B * 7uy
                     + pixel.A * 11uy) % 64uy
                )

            let writePixel pixel =
                match channels with
                | Channels.Rgb -> bytes <- bytes @ [ pixel.R; pixel.G; pixel.B ]
                | Channels.Rgba -> bytes <- bytes @ [ pixel.R; pixel.G; pixel.B; pixel.A ]

                cache[calculateIndex pixel] <- pixel

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
                else
                    let pixel = cache[int tag]
                    writePixel pixel

            let mutable y = 0u
            let mutable x = 0u

            while y < height do
                y <- y + 1u

                while x < width do
                    x <- x + 1u
                    parseChunk ()

            bytes

        let parseEndMarker () =
            let correctEndMarker =
                [| 0uy
                   0uy
                   0uy
                   0uy
                   0uy
                   0uy
                   0uy
                   1uy |]

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

        let (width, height) = parseDimensions ()

        let channels = parseChannels ()

        let colorSpace = parseColorSpace ()

        let bytes = parseChunks width height channels

        parseEndMarker ()

        createImage width height channels colorSpace bytes

    let public Decode (input: byte list) : Result<Image, DecodeError> =
        using (new MemoryStream(Array.ofList input)) (fun memStream ->
            using (new BinaryReader(memStream)) (fun binReader ->
                try
                    Ok(decode binReader)
                with
                | DecodeException error -> Error(error)))

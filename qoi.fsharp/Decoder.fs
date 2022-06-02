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

        let parseChunks (width: uint) (height: uint) =
            let mutable bytes: byte list = []

            let parseChunk () =
                let tag = binReader.ReadByte()

                if tag = 0b11111110uy then
                    let r = binReader.ReadByte()
                    let g = binReader.ReadByte()
                    let b = binReader.ReadByte()
                    bytes <- bytes @ [ r; g; b; 255uy ]
                else
                    let r = binReader.ReadByte()
                    let g = binReader.ReadByte()
                    let b = binReader.ReadByte()
                    let a = binReader.ReadByte()
                    bytes <- bytes @ [ r; g; b; a ]

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

        let bytes = parseChunks width height

        parseEndMarker ()

        createImage width height channels colorSpace bytes

    let public Decode (input: byte list) : Result<Image, DecodeError> =
        using (new MemoryStream(Array.ofList input)) (fun memStream ->
            using (new BinaryReader(memStream)) (fun binReader ->
                try
                    Ok(decode binReader)
                with
                | DecodeException error -> Error(error)))

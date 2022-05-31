namespace Qoi.Fsharp

module Decoder =
    open System.IO
    open Header

    type public Image =
        { Width: uint
          Height: uint
          Channels: Channels }

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
            | Some (_) -> ()

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

        let createImage width height channels =
            { Width = width
              Height = height
              Channels = channels }

        parseMagic ()

        let (width, height) = parseDimensions ()

        let channels = parseChannels ()

        parseColorSpace ()

        parseEndMarker ()

        createImage width height channels

    let public Decode (input: byte list) : Result<Image, DecodeError> =
        using (new MemoryStream(Array.ofList input)) (fun memStream ->
            using (new BinaryReader(memStream)) (fun binReader ->
                try
                    Ok(decode binReader)
                with
                | DecodeException error -> Error(error)))

namespace Qoi.Fsharp

module Decoder =
    open System.IO
    open Header

    type public Image = { Width: uint; Height: uint }

    type public DecodeError =
        | BadMagicBytes
        | BadChannelsValue
        | BadColorSpaceValue

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
            | Some (_) -> ()

        let parseColorSpace () =
            let colorSpace = binReader.ReadByte()
            let colorSpace = ColorSpace.ParseByte colorSpace

            match colorSpace with
            | None -> raise (DecodeException BadColorSpaceValue)
            | Some (_) -> ()

        let createImage width height = { Width = width; Height = height }

        parseMagic ()

        let (width, height) = parseDimensions ()

        parseChannels ()

        parseColorSpace ()

        createImage width height

    let public Decode (input: byte list) : Result<Image, DecodeError> =
        using (new MemoryStream(Array.ofList input)) (fun memStream ->
            using (new BinaryReader(memStream)) (fun binReader ->
                try
                    Ok(decode binReader)
                with
                | DecodeException error -> Error(error)))
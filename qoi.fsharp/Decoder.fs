namespace Qoi.Fsharp

module Decoder =
    open System.IO

    type public Image = { Width: uint; Height: uint }

    type public DecodeError = | BadMagicBytes

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

        let createImage (width: uint, height: uint) = { Width = width; Height = height }

        parseMagic () |> parseDimensions |> createImage

    let public Decode (input: byte list) : Result<Image, DecodeError> =
        using (new MemoryStream(Array.ofList input)) (fun memStream ->
            using (new BinaryReader(memStream)) (fun binReader ->
                try
                    Ok(decode binReader)
                with
                | DecodeException error -> Error(error)))

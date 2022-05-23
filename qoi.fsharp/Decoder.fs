namespace Qoi.Fsharp

module Decoder =
    [<RequireQualifiedAccess>]
    type public DecodeResult =
        | Ok
        | BadMagicBytes

    let public Decode (input: byte list) =
        let correctMagic =
            [ byte 'q'
              byte 'o'
              byte 'i'
              byte 'f' ]

        let actualMagic = List.take 4 input

        if actualMagic = correctMagic then
            DecodeResult.Ok
        else
            DecodeResult.BadMagicBytes

module DecoderTests

open Xunit
open Qoi.Fsharp.Decoder
open Qoi.Fsharp.Header

[<Fact>]
let ``Should succeed`` () =
    let expected = DecodeResult.Ok

    let input =
        [ byte 'q'
          byte 'o'
          byte 'i'
          byte 'f'

          0uy
          0uy
          0uy
          0uy

          0uy
          0uy
          0uy
          0uy

          byte Channels.Rgb

          byte ColorSpace.SRgb

          0uy
          0uy
          0uy
          0uy
          0uy
          0uy
          0uy
          1uy ]

    let actual = Decode input

    Assert.Equal(expected, actual)

[<Fact>]
let ``Should fail parsing bad magic bytes`` () =
    let expected = DecodeResult.BadMagicBytes

    let input =
        [ byte 'a'
          byte 'b'
          byte 'c'
          byte 'd'

          0uy
          0uy
          0uy
          0uy

          0uy
          0uy
          0uy
          0uy

          byte Channels.Rgb

          byte ColorSpace.SRgb

          0uy
          0uy
          0uy
          0uy
          0uy
          0uy
          0uy
          1uy ]

    let actual = Decode input

    Assert.Equal(expected, actual)

module DecoderTests

open Xunit
open Qoi.Fsharp.Decoder
open Qoi.Fsharp.Header

[<Fact>]
let ``Should succeed`` () =
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

    Decode input |> ignore

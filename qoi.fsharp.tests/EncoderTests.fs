module EncoderTests

open Xunit
open Qoi.Fsharp.Encoder

[<Fact>]
let ``Should succeed`` () =
    let input = [ 0uy; 0uy; 0uy; 255uy ]

    Encoder.Encode input |> ignore

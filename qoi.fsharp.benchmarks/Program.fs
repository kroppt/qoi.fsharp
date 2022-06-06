open BenchmarkDotNet.Running
open Qoi.Fsharp.Benchmarks.Decode
open Qoi.Fsharp.Benchmarks.Encode

[<EntryPoint>]
let main argv =
    BenchmarkRunner.Run<DecodeBenchmarks>() |> ignore
    BenchmarkRunner.Run<EncodeBenchmarks>() |> ignore
    0

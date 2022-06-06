open BenchmarkDotNet.Running
open Qoi.Fsharp.Benchmarks

[<EntryPoint>]
let main argv =
    BenchmarkRunner.Run<EncodeBenchmarks>() |> ignore
    0

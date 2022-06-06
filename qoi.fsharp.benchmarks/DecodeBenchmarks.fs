module Qoi.Fsharp.Benchmarks.Decode

open BenchmarkDotNet.Attributes
open System.IO
open Qoi.Fsharp
open BenchmarkDotNet.Diagnosers

type DecodeBenchmarks() =
    let nonAlphaBytes = File.ReadAllBytes "testdata/10x10.qoi"
    let alphaBytes = File.ReadAllBytes "testdata/sample.qoi"

    [<Benchmark>]
    member _.NonAlphaImage() =
        Decoder.Decode(List.ofArray nonAlphaBytes)

    [<Benchmark>]
    member _.AlphaImage() = Decoder.Decode(List.ofArray alphaBytes)

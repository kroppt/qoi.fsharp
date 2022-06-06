module Qoi.Fsharp.Benchmarks.Decode

open BenchmarkDotNet.Attributes
open System.IO
open Qoi.Fsharp

type public DecodeBenchmarks() =
    let nonAlphaBytes = File.ReadAllBytes "testdata/10x10.qoi"
    let alphaBytes = File.ReadAllBytes "testdata/sample.qoi"

    [<Benchmark>]
    member public _.NonAlphaImage() =
        Decoder.Decode(List.ofArray nonAlphaBytes)

    [<Benchmark>]
    member public _.AlphaImage() = Decoder.Decode(List.ofArray alphaBytes)

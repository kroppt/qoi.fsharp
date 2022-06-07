module Qoi.Fsharp.Benchmarks.Decode

open System.IO

open BenchmarkDotNet.Attributes

open Qoi.Fsharp

type public DecodeBenchmarks() =
    let nonAlphaBytes = File.ReadAllBytes "testdata/10x10.qoi"
    let alphaBytes = File.ReadAllBytes "testdata/sample.qoi"

    [<Benchmark>]
    member public _.NonAlphaImage() = Decoder.Decode nonAlphaBytes

    [<Benchmark>]
    member public _.AlphaImage() = Decoder.Decode alphaBytes

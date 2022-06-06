module Qoi.Fsharp.Benchmarks

open BenchmarkDotNet.Attributes
open SixLabors.ImageSharp.PixelFormats
open Encoder
open Header

type EncodeBenchmarks() =
    let (nonAlphaBytes, nonAlphaWidth, nonAlphaHeight) =
        using (SixLabors.ImageSharp.Image.Load<Rgb24> "testdata/10x10.png") (fun png ->
            let input = Array.zeroCreate<byte> (png.Width * png.Height * 3)
            png.CopyPixelDataTo input
            (List.ofArray input, png.Width, png.Height))

    let (alphaBytes, alphaWidth, alphaHeight) =
        using (SixLabors.ImageSharp.Image.Load<Rgba32> "testdata/sample.png") (fun png ->
            let input = Array.zeroCreate<byte> (png.Width * png.Height * 4)
            png.CopyPixelDataTo input
            (List.ofArray input, png.Width, png.Height))

    [<Benchmark>]
    member _.NonAlphaImage() =
        Encode nonAlphaBytes nonAlphaWidth nonAlphaHeight Channels.Rgb ColorSpace.SRgb

    [<Benchmark>]
    member _.AlphaImage() =
        Encode alphaBytes alphaWidth alphaHeight Channels.Rgba ColorSpace.SRgb

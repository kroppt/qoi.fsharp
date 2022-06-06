module Qoi.Fsharp.Benchmarks.Encode

open BenchmarkDotNet.Attributes
open SixLabors.ImageSharp.PixelFormats
open Qoi.Fsharp
open Qoi.Fsharp.Header

type public EncodeBenchmarks() =
    let (nonAlphaBytes, nonAlphaWidth, nonAlphaHeight) =
        using (SixLabors.ImageSharp.Image.Load<Rgb24> "testdata/10x10.png") (fun png ->
            let input = Array.zeroCreate<byte> (png.Width * png.Height * 3)
            png.CopyPixelDataTo input
            input, png.Width, png.Height)

    let (alphaBytes, alphaWidth, alphaHeight) =
        using (SixLabors.ImageSharp.Image.Load<Rgba32> "testdata/sample.png") (fun png ->
            let input = Array.zeroCreate<byte> (png.Width * png.Height * 4)
            png.CopyPixelDataTo input
            input, png.Width, png.Height)

    [<Benchmark>]
    member public _.NonAlphaImage() =
        Encoder.Encode nonAlphaBytes nonAlphaWidth nonAlphaHeight Channels.Rgb ColorSpace.SRgb

    [<Benchmark>]
    member public _.AlphaImage() =
        Encoder.Encode alphaBytes alphaWidth alphaHeight Channels.Rgba ColorSpace.SRgb

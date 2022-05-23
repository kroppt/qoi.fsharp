module EncoderTests

open Xunit
open Qoi.Fsharp.Encoder
open System.IO
open System
open SixLabors.ImageSharp.PixelFormats

[<Fact>]
let ``Should succeed`` () =
    let input = [ 0uy; 0uy; 0uy; 255uy ]
    let width = 1
    let height = 1
    let channels = Rgba
    let colorSpace = SRgb

    Encode input width height channels colorSpace
    |> ignore

[<Fact>]
let ``Should have correct header`` () =
    let writeBigEndian (binWriter: BinaryWriter) (value: int) =
        binWriter.Write(byte ((value >>> 24) &&& 0xFF))
        binWriter.Write(byte ((value >>> 16) &&& 0xFF))
        binWriter.Write(byte ((value >>> 8) &&& 0xFF))
        binWriter.Write(byte ((value >>> 0) &&& 0xFF))

    let input = [ 0uy; 0uy; 0uy; 255uy ]
    let width = 1
    let height = 1
    let channels = Rgba
    let colorSpace = SRgb

    let expected =
        using (new MemoryStream()) (fun memStream ->
            using (new BinaryWriter(memStream)) (fun binWriter ->
                binWriter.Write(byte 'q')
                binWriter.Write(byte 'o')
                binWriter.Write(byte 'i')
                binWriter.Write(byte 'f')
                writeBigEndian binWriter width
                writeBigEndian binWriter height
                binWriter.Write(getChannelsByte channels)
                binWriter.Write(getColorSpaceByte colorSpace))

            memStream.ToArray())

    let bytes = Encode input width height channels colorSpace

    let actual = ArraySegment<byte>(bytes, 0, 14)
    Assert.Equal(expected, actual)

[<Fact>]
let ``Should have correct end marker`` () =
    let expected =
        [ 0uy
          0uy
          0uy
          0uy
          0uy
          0uy
          0uy
          1uy ]

    let input = [ 100uy; 0uy; 0uy; 255uy ]
    let width = 1
    let height = 1
    let channels = Rgba
    let colorSpace = SRgb

    let bytes = Encode input width height channels colorSpace

    let actual = ArraySegment<byte>(bytes, bytes.Length - 8, 8)
    Assert.Equal(expected, actual)

[<Fact>]
let ``Should have RGBA chunk`` () =
    let expected = [ 0b11111111uy; 0uy; 0uy; 0uy; 128uy ]

    let input =
        [ 0uy
          0uy
          0uy
          128uy

          0uy
          0uy
          0uy
          128uy

          0uy
          0uy
          0uy
          128uy

          0uy
          0uy
          0uy
          128uy ]

    let width = 2
    let height = 2
    let channels = Rgba
    let colorSpace = SRgb

    let bytes = Encode input width height channels colorSpace

    let actual = ArraySegment<byte>(bytes, 14, 5)
    Assert.Equal(expected, actual)

[<Fact>]
let ``Should have RGB chunk`` () =
    let expected = [ 0b11111110uy; 128uy; 0uy; 0uy ]

    let input =
        [ 128uy
          0uy
          0uy
          255uy

          128uy
          0uy
          0uy
          255uy

          128uy
          0uy
          0uy
          255uy

          128uy
          0uy
          0uy
          255uy ]

    let width = 2
    let height = 2
    let channels = Rgba
    let colorSpace = SRgb

    let bytes = Encode input width height channels colorSpace

    let actual = ArraySegment<byte>(bytes, 14, 4)
    Assert.Equal(expected, actual)

[<Fact>]
let ``Should have index chunk`` () =
    let expected = 53uy

    let input =
        [ 128uy // RGB chunk
          0uy
          0uy
          255uy

          0uy // RGB chunk
          127uy
          0uy
          255uy

          128uy // index chunk
          0uy
          0uy
          255uy

          0uy // index chunk
          127uy
          0uy
          255uy ]

    let width = 2
    let height = 2
    let channels = Rgba
    let colorSpace = SRgb

    let bytes = Encode input width height channels colorSpace

    let actual = bytes[22]
    Assert.Equal(expected, actual)

[<Fact>]
let ``Should have diff chunk`` () =
    let expected = 0b01_11_10_10uy

    let input =
        [ 128uy // RGB chunk
          0uy
          0uy
          255uy

          129uy // diff chunk
          0uy
          0uy
          255uy ]

    let width = 2
    let height = 1
    let channels = Rgba
    let colorSpace = SRgb

    let bytes = Encode input width height channels colorSpace

    let actual = bytes[18]
    Assert.Equal(expected, actual)

[<Fact>]
let ``Should have diff chunk with wraparound`` () =
    let expected = 0b01_10_11_01uy

    let input =
        [ 128uy // RGB chunk
          255uy
          0uy
          255uy

          128uy // diff chunk
          0uy
          255uy
          255uy ]

    let width = 2
    let height = 1
    let channels = Rgba
    let colorSpace = SRgb

    let bytes = Encode input width height channels colorSpace

    let actual = bytes[18]
    Assert.Equal(expected, actual)

[<Fact>]
let ``Should have luma chunk`` () =
    let expected = [ 0b10_111111uy; 0b0000_1111uy ]

    let input =
        [ 128uy
          0uy
          0uy
          255uy

          151uy
          31uy
          38uy
          255uy ]

    let width = 2
    let height = 1
    let channels = Rgba
    let colorSpace = SRgb

    let bytes = Encode input width height channels colorSpace

    let actual = ArraySegment<byte>(bytes, 18, 2)
    Assert.Equal(expected, actual)

[<Fact>]
let ``Should have luma chunk wraparound`` () =
    let expected = [ 0b10_100010uy; 0b0110_0101uy ]

    let input =
        [ 128uy
          255uy
          0uy
          255uy

          128uy
          1uy
          255uy
          255uy ]

    let width = 2
    let height = 1
    let channels = Rgba
    let colorSpace = SRgb

    let bytes = Encode input width height channels colorSpace

    let actual = ArraySegment<byte>(bytes, 18, 2)
    Assert.Equal(expected, actual)

[<Fact>]
let ``Should have run chunk`` () =
    let expected = 0b11_000010uy

    let input =
        [ 128uy
          0uy
          0uy
          255uy

          128uy
          0uy
          0uy
          255uy

          128uy
          0uy
          0uy
          255uy

          128uy
          0uy
          0uy
          255uy

          128uy
          129uy
          0uy
          255uy ]

    let width = 5
    let height = 1
    let channels = Rgba
    let colorSpace = SRgb

    let bytes = Encode input width height channels colorSpace

    let actual = bytes[18]
    Assert.Equal(expected, actual)

[<Fact>]
let ``Should have max length run chunk`` () =
    let expected =
        [ 0b11111110uy // RGB
          128uy
          0uy
          0uy

          0b11_111101uy // run 62
          0b11_000000uy ] // run 1

    let input =
        List.append
        <| List.replicate 64 [ 128uy; 0uy; 0uy; 255uy ]
        <| [ [ 0uy; 0uy; 0uy; 0uy ] ]
        |> List.concat

    let width = 13
    let height = 5
    let channels = Rgba
    let colorSpace = SRgb

    let bytes = Encode input width height channels colorSpace

    let actual = ArraySegment<byte>(bytes, 14, 6)
    Assert.Equal(expected, actual)

[<Fact>]
let ``Should have index chunk after run`` () =
    let expected =
        [ 0b11_000001uy // run 2

          0b11111110uy // RGB
          127uy
          0uy
          0uy

          0b00_110101uy ] // index 53

    let input =
        [ 0uy
          0uy
          0uy
          255uy

          0uy
          0uy
          0uy
          255uy

          127uy
          0uy
          0uy
          255uy

          0uy
          0uy
          0uy
          255uy ]

    let width = 4
    let height = 1
    let channels = Rgba
    let colorSpace = SRgb

    let bytes = Encode input width height channels colorSpace

    let actual = ArraySegment<byte>(bytes, 14, 6)
    Assert.Equal(expected, actual)

[<Fact>]
let ``Should have run chunk before end marker`` () =
    let expected = 0b11_000000uy

    let input =
        [ 128uy
          0uy
          0uy
          255uy

          128uy
          0uy
          0uy
          255uy ]

    let width = 2
    let height = 1
    let channels = Rgba
    let colorSpace = SRgb

    let bytes = Encode input width height channels colorSpace

    let actual = bytes[bytes.Length - 9]
    Assert.Equal(expected, actual)

[<Fact>]
let ``Should encode 10x10 correctly`` () =
    let expected = File.ReadAllBytes("testdata/10x10.qoi")

    let (input, width, height) =
        using (SixLabors.ImageSharp.Image.Load<Rgb24> "testdata/10x10.png") (fun png ->
            let input = Array.zeroCreate<byte> (png.Width * png.Height * 3)
            png.CopyPixelDataTo input
            (input, png.Width, png.Height))

    let channels = Rgb
    let colorSpace = SRgb

    let actual = Encode (List.ofArray input) width height channels colorSpace

    Assert.Equal<byte>(expected, actual)

[<Fact>]
let ``Should encode sample correctly`` () =
    let expected = File.ReadAllBytes("testdata/sample.qoi")

    let (input, width, height) =
        using (SixLabors.ImageSharp.Image.Load<Rgba32> "testdata/sample.png") (fun png ->
            let input = Array.zeroCreate<byte> (png.Width * png.Height * 4)
            png.CopyPixelDataTo input
            (input, png.Width, png.Height))

    let channels = Rgba
    let colorSpace = SRgb

    let actual = Encode (List.ofArray input) width height channels colorSpace

    Assert.Equal<byte>(expected, actual)

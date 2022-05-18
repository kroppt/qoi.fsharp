module EncoderTests

open Xunit
open Qoi.Fsharp.Encoder
open System.IO
open System

[<Fact>]
let ``Should succeed`` () =
    let input = [ 0uy; 0uy; 0uy; 255uy ]
    let width = 1
    let height = 1
    let channels = Channels.Rgba
    let colorSpace = ColorSpace.SRgb

    Encoder.Encode(input, width, height, channels, colorSpace)
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
    let channels = Channels.Rgba
    let colorSpace = ColorSpace.SRgb

    let expected =
        using (new MemoryStream()) (fun memStream ->
            using (new BinaryWriter(memStream)) (fun binWriter ->
                binWriter.Write(byte 'q')
                binWriter.Write(byte 'o')
                binWriter.Write(byte 'i')
                binWriter.Write(byte 'f')
                writeBigEndian binWriter width
                writeBigEndian binWriter height
                binWriter.Write(byte channels)
                binWriter.Write(byte colorSpace))

            memStream.ToArray())

    let bytes = Encoder.Encode(input, width, height, channels, colorSpace)

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
    let channels = Channels.Rgba
    let colorSpace = ColorSpace.SRgb

    let bytes = Encoder.Encode(input, width, height, channels, colorSpace)

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
    let channels = Channels.Rgba
    let colorSpace = ColorSpace.SRgb

    let bytes = Encoder.Encode(input, width, height, channels, colorSpace)

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
    let channels = Channels.Rgba
    let colorSpace = ColorSpace.SRgb

    let bytes = Encoder.Encode(input, width, height, channels, colorSpace)

    let actual = ArraySegment<byte>(bytes, 14, 4)
    Assert.Equal(expected, actual)

module DecoderTests

open Xunit
open Qoi.Fsharp.Decoder
open Qoi.Fsharp.Header
open Qoi.Fsharp
open System

let assertError (expected: DecodeError) (actual: Result<Image, DecodeError>) =
    match actual with
    | Ok _ -> Assert.True(false, $"expected {expected} but succeeded")
    | Error actual -> Assert.Equal(expected, actual)

let assertOk (actual: Result<Image, DecodeError>) =
    match actual with
    | Ok image -> image
    | Error error ->
        Assert.True(false, $"failed with {error}")
        raise (Exception())

[<Fact>]
let ``Should succeed`` () =
    let size = 0uy

    let input =
        [ byte 'q'
          byte 'o'
          byte 'i'
          byte 'f'

          0uy
          0uy
          0uy
          size

          0uy
          0uy
          0uy
          size

          byte Channels.Rgb

          byte ColorSpace.SRgb

          0uy
          0uy
          0uy
          0uy
          0uy
          0uy
          0uy
          1uy ]

    let actual = Decode input

    assertOk actual |> ignore

[<Fact>]
let ``Should fail parsing bad magic bytes`` () =
    let size = 0uy
    let expected = BadMagicBytes

    let input =
        [ byte 'a'
          byte 'b'
          byte 'c'
          byte 'd'

          0uy
          0uy
          0uy
          size

          0uy
          0uy
          0uy
          size

          byte Channels.Rgb

          byte ColorSpace.SRgb

          0uy
          0uy
          0uy
          0uy
          0uy
          0uy
          0uy
          1uy ]

    let actual = Decode input

    assertError expected actual

[<Fact>]
let ``Should correctly parse width and height`` () =
    let expectedWidth = 1u
    let expectedHeight = 1u

    let input =
        [ byte 'q'
          byte 'o'
          byte 'i'
          byte 'f'

          0uy
          0uy
          0uy
          byte expectedWidth

          0uy
          0uy
          0uy
          byte expectedHeight

          byte Channels.Rgb

          byte ColorSpace.SRgb

          Tag.Rgb
          128uy
          0uy
          0uy

          0uy
          0uy
          0uy
          0uy
          0uy
          0uy
          0uy
          1uy ]

    let actual = Decode input

    let image = assertOk actual
    Assert.Equal(expectedWidth, image.Width)
    Assert.Equal(expectedHeight, image.Height)

[<Fact>]
let ``Should fail parsing bad channels`` () =
    let size = 0uy
    let expected = BadChannelsValue

    let input =
        [ byte 'q'
          byte 'o'
          byte 'i'
          byte 'f'

          0uy
          0uy
          0uy
          size

          0uy
          0uy
          0uy
          size

          9uy

          byte ColorSpace.SRgb

          0uy
          0uy
          0uy
          0uy
          0uy
          0uy
          0uy
          1uy ]

    let actual = Decode input

    assertError expected actual

[<Fact>]
let ``Should fail parsing bad color space`` () =
    let size = 0uy
    let expected = BadColorSpaceValue

    let input =
        [ byte 'q'
          byte 'o'
          byte 'i'
          byte 'f'

          0uy
          0uy
          0uy
          size

          0uy
          0uy
          0uy
          size

          byte Channels.Rgba

          2uy

          0uy
          0uy
          0uy
          0uy
          0uy
          0uy
          0uy
          1uy ]

    let actual = Decode input

    assertError expected actual

[<Fact>]
let ``Should fail parsing missing end marker`` () =
    let size = 0uy
    let expected = MissingEndMarker

    let input =
        [ byte 'q'
          byte 'o'
          byte 'i'
          byte 'f'

          0uy
          0uy
          0uy
          size

          0uy
          0uy
          0uy
          size

          byte Channels.Rgb

          byte ColorSpace.SRgb ]

    let actual = Decode input

    assertError expected actual

[<Fact>]
let ``Should fail parsing partial end marker`` () =
    let size = 0uy
    let expected = MissingEndMarker

    let input =
        [ byte 'q'
          byte 'o'
          byte 'i'
          byte 'f'

          0uy
          0uy
          0uy
          size

          0uy
          0uy
          0uy
          size

          byte Channels.Rgb

          byte ColorSpace.SRgb

          0uy
          0uy
          0uy
          0uy
          0uy ]

    let actual = Decode input

    assertError expected actual

[<Fact>]
let ``Should fail parsing incorrect end marker`` () =
    let size = 0uy
    let expected = IncorrectEndMarker

    let input =
        [ byte 'q'
          byte 'o'
          byte 'i'
          byte 'f'

          0uy
          0uy
          0uy
          size

          0uy
          0uy
          0uy
          size

          byte Channels.Rgb

          byte ColorSpace.SRgb

          0uy
          0uy
          0uy
          0uy
          0uy
          1uy
          1uy
          1uy ]

    let actual = Decode input

    assertError expected actual

[<Fact>]
let ``Should parse channels RGBA`` () =
    let size = 0uy
    let expected = Channels.Rgba

    let input =
        [ byte 'q'
          byte 'o'
          byte 'i'
          byte 'f'

          0uy
          0uy
          0uy
          size

          0uy
          0uy
          0uy
          size

          byte expected

          byte ColorSpace.SRgb

          0uy
          0uy
          0uy
          0uy
          0uy
          0uy
          0uy
          1uy ]

    let actual = Decode input

    let image = assertOk actual
    Assert.Equal(expected, image.Channels)

[<Fact>]
let ``Should parse channels RGB`` () =
    let size = 0uy
    let expected = Channels.Rgb

    let input =
        [ byte 'q'
          byte 'o'
          byte 'i'
          byte 'f'

          0uy
          0uy
          0uy
          size

          0uy
          0uy
          0uy
          size

          byte expected

          byte ColorSpace.SRgb

          0uy
          0uy
          0uy
          0uy
          0uy
          0uy
          0uy
          1uy ]

    let actual = Decode input

    let image = assertOk actual
    Assert.Equal(expected, image.Channels)

[<Fact>]
let ``Should parse color space SRGB`` () =
    let size = 0uy
    let expected = ColorSpace.SRgb

    let input =
        [ byte 'q'
          byte 'o'
          byte 'i'
          byte 'f'

          0uy
          0uy
          0uy
          size

          0uy
          0uy
          0uy
          size

          byte Channels.Rgb

          byte expected

          0uy
          0uy
          0uy
          0uy
          0uy
          0uy
          0uy
          1uy ]

    let actual = Decode input

    let image = assertOk actual
    Assert.Equal(expected, image.ColorSpace)

[<Fact>]
let ``Should parse color space linear`` () =
    let size = 0uy
    let expected = ColorSpace.Linear

    let input =
        [ byte 'q'
          byte 'o'
          byte 'i'
          byte 'f'

          0uy
          0uy
          0uy
          size

          0uy
          0uy
          0uy
          size

          byte Channels.Rgb

          byte expected

          0uy
          0uy
          0uy
          0uy
          0uy
          0uy
          0uy
          1uy ]

    let actual = Decode input

    let image = assertOk actual
    Assert.Equal(expected, image.ColorSpace)

[<Fact>]
let ``Should parse RGB chunk`` () =
    let size = 1uy
    let expected = [ 128uy; 0uy; 0uy; 255uy ]

    let input =
        [ byte 'q'
          byte 'o'
          byte 'i'
          byte 'f'

          0uy
          0uy
          0uy
          size

          0uy
          0uy
          0uy
          size

          byte Channels.Rgba

          byte ColorSpace.SRgb

          Tag.Rgb
          128uy
          0uy
          0uy

          0uy
          0uy
          0uy
          0uy
          0uy
          0uy
          0uy
          1uy ]

    let actual = Decode input

    let image = assertOk actual
    Assert.Equal<byte>(expected, image.Bytes)

[<Fact>]
let ``Should have bytes length based on RGB channels`` () =
    let size = 1uy
    let expectedBytesLength = int (size * size * 3uy)

    let input =
        [ byte 'q'
          byte 'o'
          byte 'i'
          byte 'f'

          0uy
          0uy
          0uy
          size

          0uy
          0uy
          0uy
          size

          byte Channels.Rgb

          byte ColorSpace.SRgb

          Tag.Rgb
          128uy
          0uy
          0uy

          0uy
          0uy
          0uy
          0uy
          0uy
          0uy
          0uy
          1uy ]

    let result = Decode input

    let actual = assertOk result
    Assert.Equal(expectedBytesLength, actual.Bytes.Length)

[<Fact>]
let ``Should have bytes length based on RGBA channels`` () =
    let size = 1uy
    let expectedBytesLength = int (size * size * 4uy)

    let input =
        [ byte 'q'
          byte 'o'
          byte 'i'
          byte 'f'

          0uy
          0uy
          0uy
          size

          0uy
          0uy
          0uy
          size

          byte Channels.Rgba

          byte ColorSpace.SRgb

          Tag.Rgb
          128uy
          0uy
          0uy

          0uy
          0uy
          0uy
          0uy
          0uy
          0uy
          0uy
          1uy ]

    let result = Decode input

    let actual = assertOk result
    Assert.Equal(expectedBytesLength, actual.Bytes.Length)

[<Fact>]
let ``Should parse RGBA chunk`` () =
    let size = 1uy
    let expected = [ 128uy; 0uy; 0uy; 128uy ]

    let input =
        [ byte 'q'
          byte 'o'
          byte 'i'
          byte 'f'

          0uy
          0uy
          0uy
          size

          0uy
          0uy
          0uy
          size

          byte Channels.Rgba

          byte ColorSpace.SRgb

          Tag.Rgba
          128uy
          0uy
          0uy
          128uy

          0uy
          0uy
          0uy
          0uy
          0uy
          0uy
          0uy
          1uy ]

    let actual = Decode input

    let image = assertOk actual
    Assert.Equal<byte>(expected, image.Bytes)

[<Fact>]
let ``Should parse index chunk`` () =
    let width = 3uy
    let height = 1uy

    let expected =
        [ 128uy
          0uy
          0uy
          255uy

          0uy
          127uy
          0uy
          255uy

          128uy
          0uy
          0uy
          255uy ]

    let input =
        [ byte 'q'
          byte 'o'
          byte 'i'
          byte 'f'

          0uy
          0uy
          0uy
          width

          0uy
          0uy
          0uy
          height

          byte Channels.Rgba

          byte ColorSpace.SRgb

          Tag.Rgb
          128uy
          0uy
          0uy

          Tag.Rgb
          0uy
          127uy
          0uy

          Tag.Index ||| 53uy

          0uy
          0uy
          0uy
          0uy
          0uy
          0uy
          0uy
          1uy ]

    let actual = Decode input

    let image = assertOk actual
    Assert.Equal<byte>(expected, image.Bytes)

[<Fact>]
let ``Should parse diff chunk`` () =
    let width = 2uy
    let height = 1uy

    let expected =
        [ 128uy
          0uy
          0uy

          129uy
          0uy
          0uy ]

    let input =
        [ byte 'q'
          byte 'o'
          byte 'i'
          byte 'f'

          0uy
          0uy
          0uy
          width

          0uy
          0uy
          0uy
          height

          byte Channels.Rgb

          byte ColorSpace.SRgb

          Tag.Rgb
          128uy
          0uy
          0uy

          Tag.Diff ||| 0b00_11_10_10uy

          0uy
          0uy
          0uy
          0uy
          0uy
          0uy
          0uy
          1uy ]

    let actual = Decode input

    let image = assertOk actual
    Assert.Equal<byte>(expected, image.Bytes)

[<Fact>]
let ``Should parse diff chunk with wraparound`` () =
    let width = 2uy
    let height = 1uy

    let expected = [ 128uy; 255uy; 0uy; 128uy; 0uy; 255uy ]

    let input =
        [ byte 'q'
          byte 'o'
          byte 'i'
          byte 'f'

          0uy
          0uy
          0uy
          width

          0uy
          0uy
          0uy
          height

          byte Channels.Rgb

          byte ColorSpace.SRgb

          Tag.Rgb
          128uy
          255uy
          0uy

          Tag.Diff ||| 0b00_10_11_01uy

          0uy
          0uy
          0uy
          0uy
          0uy
          0uy
          0uy
          1uy ]

    let actual = Decode input

    let image = assertOk actual
    Assert.Equal<byte>(expected, image.Bytes)

[<Fact>]
let ``Should parse luma chunk`` () =
    let width = 2uy
    let height = 1uy

    let expected =
        [ 128uy
          0uy
          0uy
          255uy

          151uy
          31uy
          38uy
          255uy ]

    let input =
        [ byte 'q'
          byte 'o'
          byte 'i'
          byte 'f'

          0uy
          0uy
          0uy
          width

          0uy
          0uy
          0uy
          height

          byte Channels.Rgba

          byte ColorSpace.SRgb

          Tag.Rgb
          128uy
          0uy
          0uy

          Tag.Luma ||| 0b00_111111uy
          0b0000_1111uy

          0uy
          0uy
          0uy
          0uy
          0uy
          0uy
          0uy
          1uy ]

    let actual = Decode input

    let image = assertOk actual
    Assert.Equal<byte>(expected, image.Bytes)

[<Fact>]
let ``Should parse luma chunk with wraparound`` () =
    let width = 2uy
    let height = 1uy

    let expected =
        [ 128uy
          255uy
          0uy
          255uy

          128uy
          1uy
          255uy
          255uy ]

    let input =
        [ byte 'q'
          byte 'o'
          byte 'i'
          byte 'f'

          0uy
          0uy
          0uy
          width

          0uy
          0uy
          0uy
          height

          byte Channels.Rgba

          byte ColorSpace.SRgb

          Tag.Rgb
          128uy
          255uy
          0uy

          Tag.Luma ||| 0b00_100010uy
          0b0110_0101uy

          0uy
          0uy
          0uy
          0uy
          0uy
          0uy
          0uy
          1uy ]

    let actual = Decode input

    let image = assertOk actual
    Assert.Equal<byte>(expected, image.Bytes)

[<Fact>]
let ``Should parse run chunk`` () =
    let width = 5uy
    let height = 1uy

    let expected =
        [ 128uy
          0uy
          0uy

          128uy
          0uy
          0uy

          128uy
          0uy
          0uy

          128uy
          0uy
          0uy

          128uy
          129uy
          0uy ]

    let input =
        [ byte 'q'
          byte 'o'
          byte 'i'
          byte 'f'

          0uy
          0uy
          0uy
          width

          0uy
          0uy
          0uy
          height

          byte Channels.Rgb

          byte ColorSpace.SRgb

          Tag.Rgb
          128uy
          0uy
          0uy

          Tag.Run ||| 0b00_000010uy

          Tag.Rgb
          128uy
          129uy
          0uy

          0uy
          0uy
          0uy
          0uy
          0uy
          0uy
          0uy
          1uy ]

    let actual = Decode input

    let image = assertOk actual
    Assert.Equal<byte>(expected, image.Bytes)

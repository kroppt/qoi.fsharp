module DecoderTests

open Xunit
open Qoi.Fsharp.Decoder
open Qoi.Fsharp.Header
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
    let input =
        [ byte 'q'
          byte 'o'
          byte 'i'
          byte 'f'

          0uy
          0uy
          0uy
          0uy

          0uy
          0uy
          0uy
          0uy

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
    let expected = BadMagicBytes

    let input =
        [ byte 'a'
          byte 'b'
          byte 'c'
          byte 'd'

          0uy
          0uy
          0uy
          0uy

          0uy
          0uy
          0uy
          0uy

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
    let expectedWidth = 0u
    let expectedHeight = 0u

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
    let expected = BadChannelsValue

    let width = 0u
    let height = 0u

    let input =
        [ byte 'q'
          byte 'o'
          byte 'i'
          byte 'f'

          0uy
          0uy
          0uy
          byte width

          0uy
          0uy
          0uy
          byte height

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
    let expected = BadColorSpaceValue

    let width = 0u
    let height = 0u

    let input =
        [ byte 'q'
          byte 'o'
          byte 'i'
          byte 'f'

          0uy
          0uy
          0uy
          byte width

          0uy
          0uy
          0uy
          byte height

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
    let expected = MissingEndMarker

    let input =
        [ byte 'q'
          byte 'o'
          byte 'i'
          byte 'f'

          0uy
          0uy
          0uy
          0uy

          0uy
          0uy
          0uy
          0uy

          byte Channels.Rgb

          byte ColorSpace.SRgb ]

    let actual = Decode input

    assertError expected actual

[<Fact>]
let ``Should fail parsing partial end marker`` () =
    let expected = MissingEndMarker

    let input =
        [ byte 'q'
          byte 'o'
          byte 'i'
          byte 'f'

          0uy
          0uy
          0uy
          0uy

          0uy
          0uy
          0uy
          0uy

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
    let expected = IncorrectEndMarker

    let input =
        [ byte 'q'
          byte 'o'
          byte 'i'
          byte 'f'

          0uy
          0uy
          0uy
          0uy

          0uy
          0uy
          0uy
          0uy

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
    let expected = Channels.Rgba

    let input =
        [ byte 'q'
          byte 'o'
          byte 'i'
          byte 'f'

          0uy
          0uy
          0uy
          0uy

          0uy
          0uy
          0uy
          0uy

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
    let expected = Channels.Rgb

    let input =
        [ byte 'q'
          byte 'o'
          byte 'i'
          byte 'f'

          0uy
          0uy
          0uy
          0uy

          0uy
          0uy
          0uy
          0uy

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
    let expected = ColorSpace.SRgb

    let input =
        [ byte 'q'
          byte 'o'
          byte 'i'
          byte 'f'

          0uy
          0uy
          0uy
          0uy

          0uy
          0uy
          0uy
          0uy

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
    let expected = ColorSpace.Linear

    let input =
        [ byte 'q'
          byte 'o'
          byte 'i'
          byte 'f'

          0uy
          0uy
          0uy
          0uy

          0uy
          0uy
          0uy
          0uy

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

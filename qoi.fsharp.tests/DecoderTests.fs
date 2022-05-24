module DecoderTests

open Xunit
open Qoi.Fsharp.Decoder
open Qoi.Fsharp.Header

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

    match actual with
    | Ok _ -> ()
    | Error error -> Assert.True(false, $"failed with {error}")

[<Fact>]
let ``Should fail parsing bad magic bytes`` () =
    let expected = Error(BadMagicBytes)

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

    Assert.Equal(expected, actual)

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

          0b11111110uy // RGB tag
          128uy // red
          0uy // green
          0uy // blue

          0uy
          0uy
          0uy
          0uy
          0uy
          0uy
          0uy
          1uy ]

    let actual = Decode input

    match actual with
    | Ok image ->
        Assert.Equal(expectedWidth, image.Width)
        Assert.Equal(expectedHeight, image.Height)
    | Error error -> Assert.True(false, $"failed with {error}")

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

    match actual with
    | Ok _ -> Assert.True(false, "expected error but succeeded")
    | Error error -> Assert.Equal(expected, error)

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

    match actual with
    | Ok _ -> Assert.True(false, "expected error but succeeded")
    | Error error -> Assert.Equal(expected, error)

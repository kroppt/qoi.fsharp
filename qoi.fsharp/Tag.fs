namespace Qoi.Fsharp

[<RequireQualifiedAccess>]
module public Tag =

    [<Literal>]
    let public Rgb = 0b11111110uy

    [<Literal>]
    let public Rgba = 0b11111111uy

    [<Literal>]
    let public Mask = 0b11_000000uy

    [<Literal>]
    let public Index = 0b00_000000uy

    [<Literal>]
    let public Diff = 0b01_000000uy

    [<Literal>]
    let public Luma = 0b10_000000uy

    [<Literal>]
    let public Run = 0b11_000000uy

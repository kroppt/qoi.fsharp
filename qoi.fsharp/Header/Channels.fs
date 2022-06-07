namespace Qoi.Fsharp.Header

[<Struct>]
[<RequireQualifiedAccess>]
type public Channels =
    | Rgb
    | Rgba

    static member op_Explicit channels =
        match channels with
        | Rgb -> 3uy
        | Rgba -> 4uy

    static member ParseByte value =
        match value with
        | 3uy -> Some(Rgb)
        | 4uy -> Some(Rgba)
        | _ -> None

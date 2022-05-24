namespace Qoi.Fsharp

module Header =
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

    [<Struct>]
    [<RequireQualifiedAccess>]
    type public ColorSpace =
        | SRgb
        | Linear

        static member op_Explicit colorSpace =
            match colorSpace with
            | SRgb -> 0uy
            | Linear -> 1uy

        static member ParseByte value =
            match value with
            | 0uy -> Some(SRgb)
            | 1uy -> Some(Linear)
            | _ -> None

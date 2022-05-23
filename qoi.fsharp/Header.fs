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

    [<Struct>]
    [<RequireQualifiedAccess>]
    type public ColorSpace =
        | SRgb
        | Linear

        static member op_Explicit colorSpace =
            match colorSpace with
            | SRgb -> 0uy
            | Linear -> 1uy

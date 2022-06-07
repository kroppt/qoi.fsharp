namespace Qoi.Fsharp.Header

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

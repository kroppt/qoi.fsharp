namespace Qoi.Fsharp

module Encoder =
    exception Abc of string

    type public Encoder =
        static member Encode(input: byte list) : byte list = []

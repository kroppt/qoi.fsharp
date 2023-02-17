namespace Qoi.Fsharp.Benchmarks

open BenchmarkDotNet.Running

module Program =
    [<Struct>]
    type private Dummy = { b: byte }

    [<EntryPoint>]
    let main argv =
        BenchmarkSwitcher.FromAssembly(typeof<Dummy>.Assembly).Run(argv) |> ignore

        0

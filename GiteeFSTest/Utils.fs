namespace GiteeFSTest
open System
open Microsoft.VisualStudio.TestTools.UnitTesting


[<TestClass>]
type Utils () =

    [<TestMethod>]
    member this.renderMarkdown () =
        GiteeFS.Utils.renderMarkdown "# 1145141919810"
        |> Result.bind (fun x->
            printfn "%s" x
            Ok ())
        |> ignore


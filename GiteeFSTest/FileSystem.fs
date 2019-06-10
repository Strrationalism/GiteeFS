namespace GiteeFSTest
open System
open Microsoft.VisualStudio.TestTools.UnitTesting
open GiteeFS.Utils

[<TestClass>]
type FileSystem () =

    [<TestMethod>]
    member this.getIndex () =
        let test accessToken =
            { owner="cnwhy";repo="BitMatrix" }
            |> GiteeFS.FileSystem.getIndex accessToken 
            |> function
            | Ok x -> 
                x
                |> Array.iter (fun x -> printfn "%s" x.path)
            | Error ex -> raise ex
        printfn "=== No Access Token ==="
        test None
        printfn "=== Has Access Token ==="
        test (Authentication.accessToken.Force() |> Some)

    [<TestMethod>]
    member this.download () =
        { owner="cnwhy";repo="BitMatrix" }
        |> GiteeFS.FileSystem.getIndex None
        |> function 
        | Ok x -> x
        | Error x -> raise x
        |> Array.find (fun x -> x.path = "README.md")
        |> GiteeFS.FileSystem.downloadString None
        |> function
        | Ok x -> x
        | Error x -> raise x
        |> printfn "%A"
        

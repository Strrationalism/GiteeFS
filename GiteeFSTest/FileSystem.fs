namespace GiteeFSTest
open System
open Microsoft.VisualStudio.TestTools.UnitTesting
open GiteeFS.Utils

[<TestClass>]
type FileSystem () =

    let exampleRepo = { owner="cnwhy";repo="BitMatrix" }

    [<TestMethod>]
    member this.getIndex () =
        let test accessToken =
            exampleRepo
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
        let test acc =
            exampleRepo
            |> GiteeFS.FileSystem.getIndex acc
            |> function 
            | Ok x -> x
            | Error x -> raise x
            |> Array.find (fun x -> x.path = "README.md")
            |> GiteeFS.FileSystem.download acc
            |> function
            | Ok x -> x
            | Error x -> raise x
            |> System.Text.Encoding.Default.GetString
            |> printfn "%A"
        printfn "=== No Access Token ==="
        test None
        printfn "=== Has Access Token ==="
        test (Authentication.accessToken.Force() |> Some)
        
    [<TestMethod>]
    member this.getFileByPath () =
        let test acc path =
            GiteeFS.FileSystem.getFileByPath acc exampleRepo path
            |> function 
            | Ok (item,data) ->
                printfn "%A" item
                printfn "%s" (System.Text.Encoding.Default.GetString data)
            | Error exn -> raise exn
        printfn "=== No Access Token ===" 
        test None "README.md"
        printfn "=== Has Access Token ==="
        test (Authentication.accessToken.Force() |> Some) "README.md"

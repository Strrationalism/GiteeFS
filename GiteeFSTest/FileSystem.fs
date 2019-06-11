namespace GiteeFSTest
open System
open Microsoft.VisualStudio.TestTools.UnitTesting
open GiteeFS.Utils

[<TestClass>]
type FileSystem () =

    let exampleRepo = { owner="cnwhy";repo="BitMatrix" }

    [<TestMethod>]
    member this.download () =
        let test acc =
            
            GiteeFS.FileSystem.getFileByPath acc exampleRepo "README.md"
            |> function 
            | Ok (x,_) -> x
            | Error x -> raise x
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

    [<TestMethod>]
    member this.getDirectoryContentByPath () =
        let test acc path =
            GiteeFS.FileSystem.getDirectoryContentByPath acc exampleRepo path
            |> function
            | Ok items ->
                items
                |> Array.iter (fun x -> printfn "%s\t%A\t%s" x.path x.itemType x.sha)
            | Error e -> raise e
        printfn "=== No Access Token ===" 
        test None ""
        printfn "=== Has Access Token ==="
        test (Authentication.accessToken.Force() |> Some) "src"

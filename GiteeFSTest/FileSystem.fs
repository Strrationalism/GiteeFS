namespace GiteeFSTest
open System
open Microsoft.VisualStudio.TestTools.UnitTesting
open GiteeFS.Utils

[<TestClass>]
type FileSystem () =

    let exampleRepo = { owner="cnwhy";repo="BitMatrix" }

    (*let writableRepo = { owner = "";repo = "" }
    
    [<TestMethod>]
    member this.magicTest () =
        let fileItem =
            GiteeFS.FileSystem.createFile 
                (Authentication.accessToken.Force())
                writableRepo
                "FunckyTest/Super.md"
                (System.Text.Encoding.Default.GetBytes "# stupid")
                "CreateFileTest"
            |> function
            | Ok x -> x
            | Error e -> raise e

        let updatedFileItem =
            GiteeFS.FileSystem.updateFile 
                (Authentication.accessToken.Force())
                fileItem
                (System.Text.Encoding.Default.GetBytes "## stupid")
                "UpdateFileTest"
            |> function
            | Ok x -> x
            | Error e -> raise e

        GiteeFS.FileSystem.download 
            (Authentication.accessToken.Force() |> Some) 
            updatedFileItem
        |> function
        | Ok x -> x
        | Error exn -> raise exn
        |> System.Text.Encoding.Default.GetString
        |> printfn "%s"


        GiteeFS.FileSystem.deleteFile 
            (Authentication.accessToken.Force())
            updatedFileItem
            "DeleteFileTest"
        |> function
        | Error exn -> raise exn
        | Ok () -> ()*)

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

    [<TestMethod>]
    member this.downloadDirectory () =
        let downloadLogger (logItem:GiteeFS.FileSystem.Item) =
            printfn "Downloading %s" logItem.path

        IO.Directory.CreateDirectory "Clone1" |> ignore
        IO.Directory.CreateDirectory "Clone2" |> ignore

        GiteeFS.FileSystem.downloadDirectory 
            None
            downloadLogger
            exampleRepo
            ""
            "Clone1"
        |> function
        | Ok () -> ()
        | Error x -> raise x

        GiteeFS.FileSystem.downloadDirectory 
            (Authentication.accessToken.Force() |> Some)
            downloadLogger
            exampleRepo
            ""
            "Clone2"
        |> function
        | Ok () -> ()
        | Error x -> raise x
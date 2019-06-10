namespace GiteeFSTest
open System
open Microsoft.VisualStudio.TestTools.UnitTesting


[<TestClass>]
type Authentication () =

    static member accessToken = lazy (
        let userData =
            IO.File.ReadAllLines "../../../LoginInfo.txt"
        GiteeFS.Authentication.login
            userData.[0] userData.[1] userData.[2] userData.[3]
        |> function
        | Ok x -> x
        | Error x -> raise x)

    [<TestMethod>]
    member this.login () =
        Authentication.accessToken.Force () |> ignore


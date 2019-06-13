namespace GiteeFSTest
open System
open Microsoft.VisualStudio.TestTools.UnitTesting


[<TestClass>]
type Authentication () =

    static member accessToken = lazy (
        let userData =
            IO.File.ReadAllLines "../../../LoginInfo.txt"
        GiteeFS.Authentication.buildAccessToken userData.[0])

    [<TestMethod>]
    member this.login () =
        Authentication.accessToken.Force () |> ignore


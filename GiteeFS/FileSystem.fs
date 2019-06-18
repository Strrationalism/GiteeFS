module GiteeFS.FileSystem

open Utils
open Authentication
open FSharp.Data
open System
open System.Net.Http

type ItemType =
| Directory
| File

and Item = {
    path : string
    sha : Sha
    itemType : ItemType
    source : Repo
}

/// 下载文件
let download accessToken item =
    try
        let options =
            match accessToken with
            | Some x -> sprintf "?access_token=%s" (tokenString x)
            | None -> ""
        let response =
            sprintf "https://gitee.com/api/v5/repos/%s/%s/git/blobs/%s%s"
                item.source.owner
                item.source.repo
                item.sha
                options
            |> JsonValue.Load

        if "base64" <> (response.GetProperty "encoding" |> JsonExtensions.AsString) then
            raise (InvalidJsonResponse response)

        let blob =
            response.GetProperty "content"
            |> JsonExtensions.AsString
            |> System.Convert.FromBase64String

        blob |> Ok

    with ex ->
        Error ex
    
/// 新建文件
let createFile accessToken repo path content msg =
    try
        let query =
            [
                "access_token",Authentication.tokenString accessToken
                "content",Convert.ToBase64String content
                "message",msg
                ]
        let url =
            sprintf "https://gitee.com/api/v5/repos/%s/%s/contents/%s"
                repo.owner
                repo.repo
                path
            |> Uri.EscapeUriString

        let response = 
            Http.RequestString (url,query,["charset","UTF-8"],"POST")
            |> JsonValue.Parse

        let content = response.GetProperty "content"

        {
            path = content.GetProperty "path" |> JsonExtensions.AsString
            sha = content.GetProperty "sha" |> JsonExtensions.AsString
            itemType = File
            source = repo
        }
        |> Ok
        

    with ex ->
        Error ex


/// 更新文件
let updateFile accessToken fileItem content msg =
    try
        use http = new HttpClient ()
        use query =
            new FormUrlEncodedContent(
                [
                    "access_token",Authentication.tokenString accessToken
                    "content",Convert.ToBase64String content
                    "sha",fileItem.sha
                    "message",msg]
                |> dict)
             

        let url =
            sprintf "https://gitee.com/api/v5/repos/%s/%s/contents/%s"
                fileItem.source.owner
                fileItem.source.repo
                fileItem.path

        let response = 
            http.PutAsync(url,query).Result.Content.ReadAsStringAsync().Result
            |> JsonValue.Parse

        let content = response.GetProperty "content"

        {
            path = content.GetProperty "path" |> JsonExtensions.AsString
            sha = content.GetProperty "sha" |> JsonExtensions.AsString
            itemType = File
            source = fileItem.source
        }
        |> Ok
    

    with ex ->
        Error ex

/// 删除文件
let deleteFile accessToken fileItem msg =
    try
        let query =
            [
                "access_token",Authentication.tokenString accessToken
                "sha",fileItem.sha
                "message",msg]
        let url =
            sprintf "https://gitee.com/api/v5/repos/%s/%s/contents/%s"
                fileItem.source.owner
                fileItem.source.repo
                fileItem.path
            |> Uri.EscapeUriString
        let response = Http.Request (url,query,[],"DELETE")
        if response.StatusCode <> 200 then
            raise (HttpFailed response.StatusCode)
        Ok ()

    with ex ->
        Error ex

/// 根据路径获取文件
let getFileByPath accessToken repo path =
    try
        use http = new HttpClient ()

        let url = 
            match accessToken with
            | Some t ->
                sprintf "https://gitee.com/api/v5/repos/%s/%s/contents/%s?accessToken=%s"
                    repo.owner
                    repo.repo
                    path
                    (tokenString t)
            | None ->
                sprintf "https://gitee.com/api/v5/repos/%s/%s/contents/%s"
                    repo.owner
                    repo.repo
                    path

        let json =
            http.GetStringAsync(url).Result
            |> JsonValue.Parse

        let itemType =
            match json.GetProperty "type" |> JsonExtensions.AsString with
            | "file" -> File
            | _ -> raise (InvalidJsonResponse json)

        let item = {
            path = json.GetProperty "path" |> JsonExtensions.AsString
            sha = json.GetProperty "sha" |> JsonExtensions.AsString
            itemType = itemType
            source = repo
        }

        let data =
            match itemType with
            | File _ ->
                json.GetProperty "content"
                |> JsonExtensions.AsString
                |> Convert.FromBase64String
            
            | Directory ->
                [||]
        Ok (item,data)
    with ex ->
        Error ex

/// 根据路径获取目录内容
let getDirectoryContentByPath accessToken repo path =
    try
        use http = new HttpClient ()

        let url = 
            match accessToken with
            | Some t ->
                sprintf "https://gitee.com/api/v5/repos/%s/%s/contents/%s?accessToken=%s"
                    repo.owner
                    repo.repo
                    path
                    (tokenString t)
            | None ->
                sprintf "https://gitee.com/api/v5/repos/%s/%s/contents/%s"
                    repo.owner
                    repo.repo
                    path

        let response =
            http.GetStringAsync(url).Result
            |> JsonValue.Parse

        response
        |> JsonExtensions.AsArray
        |> Array.map (fun json ->
            {
                path = json.GetProperty "path" |> JsonExtensions.AsString
                sha = json.GetProperty "sha" |> JsonExtensions.AsString
                itemType = 
                    match json.GetProperty "type" |> JsonExtensions.AsString with
                    | "file" -> File
                    | "dir" -> Directory
                    | _ -> raise (InvalidJsonResponse response)
                source = repo
            })
        |> Ok
    with exn ->
        Error exn

/// 下载目录内容到本地目录
let rec downloadDirectory accessToken logCallback repo path (downloadTo:string) : Result<unit,exn> =
    let target = downloadTo.Trim('/').Trim('\\')
    getDirectoryContentByPath accessToken repo path
    |> Result.bind (
        Array.Parallel.map (fun x ->
            let name = 
                match x.path.LastIndexOf '/' with
                | -1 -> x.path
                | pos -> x.path.[pos+1 ..]

            lock logCallback (fun () -> logCallback x)

            match x.itemType with
            | Directory ->
                IO.Directory.CreateDirectory (target+"/"+name)
                |> ignore
                downloadDirectory accessToken logCallback repo (path+"/"+name) (target+"/"+name) 
            | File ->
                download accessToken x
                |> Result.map (fun x -> 
                    IO.File.WriteAllBytes (target+"/"+name,x)))
        >> Array.filter (function
        | Ok _ -> false
        | Error _ -> true)
        >> function
        | [||] -> Ok()
        | errors -> Array.head errors)


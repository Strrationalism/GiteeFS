module GiteeFS.FileSystem

open Utils
open Authentication
open FSharp.Data
open System

type BlobInfo = {
    size : uint64
}

and ItemType =
| Tree
| Blob of BlobInfo

and Item = {
    path : string
    sha : Sha
    itemType : ItemType
    source : Repo
}

/// 获取仓库目录树
let getIndex accessToken repo =
    try
        let resultJson =
            let options =
                match accessToken with
                | Some x -> sprintf "&access_token=%s" (tokenString x)
                | None -> ""
            sprintf "https://gitee.com/api/v5/repos/%s/%s/git/gitee/trees/master?recursive=1%s"
                repo.owner
                repo.repo
                options
            |> JsonValue.Load

        JsonExtensions.Item (resultJson,"tree")
        |> JsonExtensions.AsArray
        |> Array.map (fun json ->
            {
                path = json.GetProperty "path" |> JsonExtensions.AsString
                sha = json.GetProperty "sha" |> JsonExtensions.AsString
                itemType =
                    match json.GetProperty "type" |> JsonExtensions.AsString with
                    | "tree" -> Tree
                    | "blob" -> 
                        { size = json.GetProperty "size" |> JsonExtensions.AsInteger64 |> uint64 }
                        |> Blob
                    | _ -> raise (InvalidJsonResponse json)
                source = repo
            })
        |> Ok
    with ex ->
        Error ex

/// 下载二进制数据
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

/// 下载字符串
let downloadString accessToken item =
    download accessToken item
    |> Result.map (System.Text.Encoding.Default.GetString)

    
/// 新建文件
let createFile accessToken repo path content msg =
    try
        let query =
            [
                "access_token",Authentication.tokenString accessToken
                "content",Convert.ToBase64String content
                "message",msg]
        let url =
            sprintf "https://gitee.com/api/v5/repos/%s/%s/contents/%s"
                repo.owner
                repo.repo
                path
        let response = Http.Request (url,query,[],"POST")
        if response.StatusCode <> 200 then
            raise (HttpFailed response.StatusCode)
        Ok ()

    with ex ->
        Error ex


/// 更新文件
let updateFile accessToken fileItem content msg =
    try
        let query =
            [
                "access_token",Authentication.tokenString accessToken
                "content",Convert.ToBase64String content
                "sha",fileItem.sha
                "message",msg]
        let url =
            sprintf "https://gitee.com/api/v5/repos/%s/%s/contents/%s"
                fileItem.source.owner
                fileItem.source.repo
                fileItem.path
        let response = Http.Request (url,query,[],"PUT")
        if response.StatusCode <> 200 then
            raise (HttpFailed response.StatusCode)
        Ok ()

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
        let response = Http.Request (url,query,[],"DELETE")
        if response.StatusCode <> 200 then
            raise (HttpFailed response.StatusCode)
        Ok ()

    with ex ->
        Error ex
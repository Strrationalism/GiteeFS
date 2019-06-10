module GiteeFS.Authentication

open FSharp.Data

type AccessToken = private {
    token : string
}

exception InvalidGrant of JsonValue

/// 获取access token字符串
let tokenString token =
    token.token

/// 登陆并获取access token
let login username password clientId clientSecret = 
    try
        let query = [
            "grant_type","password"
            "username",username
            "password",password
            "client_id",clientId
            "client_secret",clientSecret
            "scope","projects" ] 

        let result = 
            Http.RequestString(
                "https://gitee.com/oauth/token",
                query,
                ["Content-Type","application/x-www-form-urlencoded"],
                "POST")
            |> JsonValue.Parse

        FSharp.Data.JsonExtensions.Item (result,"access_token")
        |> FSharp.Data.JsonExtensions.AsString
        |> function
        | "invalid_grant" -> raise (InvalidGrant result)
        | accessToken -> { token = accessToken } |> Ok

    with ex -> 
        Error ex
    


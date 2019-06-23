module GiteeFS.Utils

/// SHA
type Sha = string

/// 仓库
type Repo = {
    owner : string
    repo : string
}

/// 预料外的JSON响应
exception InvalidJsonResponse of FSharp.Data.JsonValue

/// http访问失败
exception HttpFailed of int

/// 渲染Markdown
let renderMarkdown text =
    try
        use http = new System.Net.Http.HttpClient ()
        http.PostAsync("https://gitee.com/api/v5/markdown",new System.Net.Http.FormUrlEncodedContent(dict ["text",text]))
            .Result.Content.ReadAsStringAsync().Result
        |> Ok
    with ex ->
        Error ex

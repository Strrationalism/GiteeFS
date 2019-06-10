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

/// 渲染Markdown
let renderMarkdown text =
    try
        FSharp.Data.Http.RequestString ("https://gitee.com/api/v5/markdown",["text",text],[],"POST")
        |> Ok
    with ex ->
        Error ex

#r "paket:
nuget Fake.Core.Target 
nuget Fake.StaticGen
nuget Fake.StaticGen.Html
nuget Fake.StaticGen.Markdown
nuget Markdig 0.20
nuget MarkdigExtensions.SyntaxHighlighting
nuget MarkdigExtensions.UrlRewriter
nuget Nett
nuget YamlDotNet
nuget NetCoreVersions
nuget FSharp.Data
nuget FsToolkit.ErrorHandling //"
#load "./.fake/build.fsx/intellisense.fsx"

open Fake.Core
open Fake.IO

open Markdig
open Nett

#load "src/site.fsx"

// Targets
//////////
let [<Literal>] configFile = "config.toml"
let [<Literal>] secretsFile = "secrets.toml"

Target.create "Generate" <| fun _ ->
    if File.exists secretsFile then
        let config = 
            [ configFile; secretsFile ]
            |> List.map File.readAsString
            |> String.concat "\n" 
            |> Helpers.parseConfig
        Site.generateSite config |> Async.RunSynchronously
    else
        Trace.traceErrorfn "Could not find file '%s'. Please create one using the Configure target." secretsFile

Target.create "Configure" <| fun p ->
    match p.Context.Arguments with
    | [ ghClientId; ghClientSecret ] ->
        let toml = Toml.Create()
        toml.Add("gh-client-id", ghClientId) |> ignore
        toml.Add("gh-client-secret", ghClientSecret) |> ignore
        Toml.WriteFile(toml, secretsFile)
    | _ ->
        Trace.traceErrorfn "Invalid arguments for Configure"
        Trace.traceErrorfn "Required arguments: [GitHub Client ID] [GitHub Client Secret]"
          
Target.runOrDefaultWithArguments "Generate"
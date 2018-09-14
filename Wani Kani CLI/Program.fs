module Program

open System
open System.IO
open System.Reflection
open Config
open Input
open WaniKani
open canopy.runner.classic
open canopy.configuration
open canopy.classic

[<EntryPoint>]
let main argv =
    System.Console.CancelKeyPress.Add(fun _ -> 
        quit())
    System.Console.OutputEncoding <- System.Text.Encoding.UTF8
    chromeDir <- Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
    hideCommandPromptWindow <- true
    start canopy.types.BrowserStartMode.ChromeHeadless
    printfn "%s" "tip: You can press F2 to convert a number to words, or F3 for ordinal words! (must be alone on line for now)"
    printfn "%s" "Please note that arrow keys are currently broken during input"
    routeToPage Login
    runPage Login
    quit()
    0

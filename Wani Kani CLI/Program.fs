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
    System.Console.OutputEncoding <- System.Text.Encoding.UTF8
    chromeDir <- Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
    start canopy.types.BrowserStartMode.ChromeHeadless
    routeToPage Login
    runPage Login
    quit ()
    0

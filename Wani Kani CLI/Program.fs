module Program

open System
open System.IO
open Config
open OpenQA.Selenium.PhantomJS
open WaniKani
open canopy.runner.classic
open canopy.configuration
open canopy.classic


[<EntryPoint>]
let main argv =
    start canopy.types.BrowserStartMode.ChromeHeadless
    File.Delete(configPath)
    printfn "%O" <| login ( getCreds())
    quit ()
    0

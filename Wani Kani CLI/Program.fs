module Program

open System
open System.IO
open Config
open KanaInput
open OpenQA.Selenium.PhantomJS
open WaniKani
open canopy.runner.classic
open canopy.configuration
open canopy.classic


[<EntryPoint>]
let main argv =
    System.Console.OutputEncoding <- System.Text.Encoding.UTF8
    start canopy.types.BrowserStartMode.Chrome
    printfn "%O" <| login ( getCreds())
    runReview () |> ignore
    quit ()
    0

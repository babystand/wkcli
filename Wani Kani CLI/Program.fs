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
    let kana = getKanaInput()
    printfn "made %s" kana
//    start canopy.types.BrowserStartMode.ChromeHeadless
//    File.Delete(configPath)
//    printfn "%O" <| login ( getCreds())
//    quit ()
    0

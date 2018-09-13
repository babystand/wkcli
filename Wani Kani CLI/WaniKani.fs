module WaniKani

open canopy.runner.classic
open canopy.configuration
open canopy.classic
open Config
open System
open System.Drawing
open Colorful
open KanaInput
open canopy

type Console = Colorful.Console

type ReviewType =
    | RadicalName of string
    | KanjiMeaning of string
    | KanjiSpelling of string
    | VocabMeaning of string
    | VocabSpelling of string

type ReviewAnswerInfo = bool * string list

//let's write a state machine!
type Page =
    | Login
    | DashBoard
    | Review of ReviewType * ReviewAnswerInfo option
    | ResultsPage
    | OuterSpace
let expandReviewItemInfo () =
    do click "html body div#reviews.pure-g-r div.pure-u-1 div#additional-content ul li#option-item-info"
    do waitForElement "#item-info"
    
let parseExtraInfo rt =
    expandReviewItemInfo ()
    match rt with 
    | KanjiMeaning _ | VocabMeaning _->
            let bm = read "html body div#reviews.pure-g-r div.pure-u-1 div#information div#item-info div.pure-g-r div#item-info-col1.pure-u-1-4 section#item-info-meaning"
            let hb = read "html body div#reviews.pure-g-r div.pure-u-1 div#information div#item-info div.pure-g-r div#item-info-col2.pure-u-3-4 section#item-info-meaning-mnemonic"
            [bm;hb]
    | KanjiSpelling _ | VocabSpelling _-> 
            let br = read "html body div#reviews.pure-g-r div.pure-u-1 div#information div#item-info div.pure-g-r div#item-info-col1.pure-u-1-4 section#item-info-reading"
            let hr = read "html body div#reviews.pure-g-r div.pure-u-1 div#information div#item-info div.pure-g-r div#item-info-col2.pure-u-3-4 section#item-info-reading-mnemonic"
            [br;hr]
    | RadicalName _ ->
            let nr = read "html body div#reviews.pure-g-r div.pure-u-1 div#information div#item-info div.pure-g-r div#item-info-col1.pure-u-1-4 section"
            let hr = read "html body div#reviews.pure-g-r div.pure-u-1 div#information div#item-info div.pure-g-r div#item-info-col2.pure-u-3-4 section"
            [nr;hr]



let getColor state =
    match state with
    | RadicalName _ -> Color.LightBlue
    | KanjiMeaning _ | KanjiSpelling _ -> Color.Purple
    | VocabMeaning _ | VocabSpelling _ -> Color.YellowGreen

let (!!!) (str : string) (col : Color) : unit = Console.WriteLine(str, col)

let parseReviewType() =
    waitForElement "#character"
    waitForElement "#question-type"
    let itemType = (element "#character").GetAttribute("class")
    let questionType = (element "#question-type").GetAttribute("class")
    let item = read "#character>span"
    match itemType, questionType with
    | "vocabulary", "reading" -> VocabSpelling item
    | "vocabulary", "meaning" -> VocabMeaning item
    | "kanji", "reading" -> KanjiSpelling item
    | "kanji", "meaning" -> KanjiMeaning item
    | "radical", "meaning" -> RadicalName item
    | _ -> failwith "couldn't parse review type"

let toPrompt rt =
    match rt with
    | RadicalName x -> sprintf "%s\r\nRadical Name" x
    | KanjiMeaning x -> sprintf "%s\r\nKanji Meaning" x
    | KanjiSpelling x -> sprintf "%s\r\nKanji Reading" x
    | VocabMeaning x -> sprintf "%s\r\nVocab Meaning" x
    | VocabSpelling x -> sprintf "%s\r\nVocab Reading" x

let parseReviewPage() =
    waitForElement "#character"
    let reviewType = parseReviewType()
    let box =
        (element "html body div#reviews.pure-g-r div.pure-u-1 div#question div#answer-form form fieldset")
            .GetAttribute("class")
    match box with
    | "" -> Review(reviewType, None)
    | "correct" -> 
            Review(reviewType, Some(true, parseExtraInfo reviewType))
    | "incorrect" -> 
                Review(reviewType, Some(false, parseExtraInfo reviewType))
    | _ -> failwith "couldn't parse success status"

let parsePage() =
    let loc = currentUrl()
    match loc with
    | "https://www.wanikani.com/login" -> Login
    | "https://www.wanikani.com/dashboard" -> DashBoard
    | "https://www.wanikani.com/review" -> ResultsPage
    | "https://www.wanikani.com/review/session" -> parseReviewPage()
    | _ -> OuterSpace

let viewReviewPrompt rt =
    let col = getColor rt
    (!!!) (toPrompt rt) col
    (!!!) "Type your answer and submit with Enter" col

let viewReview rt rai : unit =
    match rai with
    | None -> viewReviewPrompt rt
    | Some(b, y) -> 
        let color =
            if b then Color.Red
            else Color.Green
        (!!!) (toPrompt rt) color
        y |> List.iter (fun x -> (!!!) (sprintf "%s\r\n***" x) color)
        if b then (!!!) "Correct!" color else (!!!) "Incorrect!" color
        (!!!) "Press any key to continue." Color.White

let viewPage page : unit =
    match page with
    | Login -> (!!!) "Please login! Press any key to continue." Color.White
    | DashBoard -> (!!!) "Press any key to begin review session" Color.White
    | OuterSpace -> (!!!) "I don't have any idea where you are. Probably need to try restarting" Color.White
    | ResultsPage -> (!!!) "Someday, you'll see your results here!" Color.White
    | Review(x, y) -> viewReview x y

let routeToPage page =
    url <| match page with
           | Login -> "https://www.wanikani.com/login"
           | DashBoard -> "https://www.wanikani.com/dashboard"
           | ResultsPage -> "https://www.wanikani.com/review"
           | Review _ -> "https://www.wanikani.com/review/session"
           | _ -> "https://www.wanikani.com/dashboard"

let goToDashboard() =
    url "https://wanikani.com/dashboard"
    waitForElement "#burned"

let login creds =
    waitForElement "#user_login"
    "#user_login" << creds.username
    "#user_password" << creds.password
    click "html body section.session.login div.wrapper form#new_user.new_user fieldset button.button" //submit

let goToReview() = url "https://www.wanikani.com/review/session"
//need to wire up actions to each page, as well as update tree to run recursively
let waitForKeyPress() = Console.ReadKey() |> ignore

let waitForInput rt : string =
    match rt with
    | RadicalName _ | KanjiMeaning _ | VocabMeaning _ -> Console.ReadLine()
    | _ -> getKanaInput()

let clickReviewNext() =
    click "html body div#reviews.pure-g-r div.pure-u-1 div#question div#answer-form form fieldset button"

let submitReviewAnswer ans =
    "#user-response" << ans
    do clickReviewNext()

let rec runPage page =
    do viewPage page
    match page with
    | Login -> 
        do waitForKeyPress()
        do login <| getCreds()
    | DashBoard -> 
        do waitForKeyPress()
        do goToReview()
    | Review(rt, None) -> 
        let input = waitForInput rt
        do submitReviewAnswer input
    | Review(rt, Some(b, ls)) -> 
        do waitForKeyPress()
        do clickReviewNext()
    | ResultsPage -> 
        do waitForKeyPress()
        do routeToPage DashBoard
    | OuterSpace -> 
        do waitForKeyPress()
        do routeToPage DashBoard
    runPage <| parsePage()

module WaniKani

open canopy.runner.classic
open canopy.configuration
open canopy.classic
open Config
open System
open System.Drawing
open Colorful
open Input
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
    click "html body div#reviews.pure-g-r div.pure-u-1 div#additional-content ul li#option-item-info"
    waitForElement "#item-info"
    
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

let parseMissingRadical () =
    waitForElement "html body div#reviews.pure-g-r div.pure-u-1 div#question div#character.radical span i"
    let name = (element "html body div#reviews.pure-g-r div.pure-u-1 div#question div#character.radical span i").GetAttribute("class")
    match name.Replace("radical-", "") with
    | "gun" -> "𠂉"
    | "beggar" -> "与 (without bottom line)"
    | "leaf" -> "丆"
    | "triceratops" -> "⺌"
    | "stick" -> "⼁"
    | "hat" -> "𠆢 (个 without the vertical line)"
    | "horns" -> "丷"
    | "spikes" -> "业"
    | "cactus" -> "墟 (bottom right radical)"
    | "trident" -> "棄 (center radical)"
    | "shark" -> "烝"
    | "comb" -> "段 (left half)"
    | "egg" -> "乍 (without the top-left tick)"
    | "death-star" -> "俞"
    | "corn" -> "演 (middle radical)"
    | "explosion" -> "渋 (bottom-right radical)"
    | "hick" -> "度 (without the 又)"
    | "worm" -> "堂 (without the top radicals)"
    | "squid" -> "剣 (without the 刂)"
    | "zombie" -> "遠 (without the ⻌ )"
    | "grass" -> "⺍"
    | "bar" -> "残 (right half)"
    | "creeper" -> "司 (inside radical)"
    | "cloak" -> "司 (outside radical)"
    | "train" -> "夫"
    | "tofu" -> "旅 (bottom-right radical)"
    | "bear" -> "官 (without the 宀)"
    | "boob" -> "育 (top half)"
    | "blackjack" -> "昔 (top half)"
    | "chinese" -> "漢 (right half)"
    | "pope" -> "盾 (inside radical)"
    | "cleat" -> "⺤"
    | "hills" -> "之 (without the top tick)"
    | "kick" -> "表 (bottom half)"
    | "viking" -> "学 (without the 子)"
    | "potato" -> "華 (without the ⺾)"
    | "water-slide" -> "⻌"
    | "psychopath" -> "鬱 (bottom half)"
    | "morning" -> "乾 (left radical)"
    | "saw" -> "恐 (without the ⼼)"
    | _ -> ""

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
    | "radical", "meaning" -> match item with
                              | "" -> RadicalName <| parseMissingRadical ()
                              | _ -> RadicalName item
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
    clickReviewNext()

let rec runPage page =
    viewPage page
    match page with
    | Login -> 
        waitForKeyPress()
        login <| getCreds()
    | DashBoard -> 
        waitForKeyPress()
        goToReview()
    | Review(rt, None) -> 
        let input = waitForInput rt
        submitReviewAnswer input
    | Review(rt, Some(b, ls)) -> 
        waitForKeyPress()
        clickReviewNext()
    | ResultsPage -> 
        waitForKeyPress()
        routeToPage DashBoard
    | OuterSpace -> 
        waitForKeyPress()
        routeToPage DashBoard
    runPage <| parsePage()

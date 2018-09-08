module WaniKani

open canopy.runner.classic
open canopy.configuration
open canopy.classic
open Config
open System
open System.Drawing
open Colorful

type Console = Colorful.Console

type ReviewType =
    | RadicalName of string
    | KanjiMeaning of string
    | KanjiSpelling of string
    | VocabMeaning of string
    | VocabSpelling of string

type ReviewAnswerInfo =
    { LeftCol : (string * string) list
      RightCol : (string * string) list }

//let's write a state machine!
type Page =
    | Login
    | DashBoard
    | Review of ReviewType
    | ReviewSuccess of ReviewType * ReviewAnswerInfo
    | ReviewError of ReviewType * ReviewAnswerInfo
    | ResultsPage

let goToDashboard () =
    url "https://wanikani.com/dashboard"
    DashBoard
    

let login creds =
    url "https://www.wanikani.com/login"
    "#user_login" << creds.username
    "#user_password" << creds.password
    click "html body section.session.login div.wrapper form#new_user.new_user fieldset button.button" //submit
    match currentUrl() with
    | "https://www.wanikani.com/dashboard" -> DashBoard
    | u -> 
        do printfn "redirected to %s" u
        do url "https://www.wanikani.com/login" //try login again
        Login

    
let parseReviewPage() =
    let itemType = (element "#character").GetAttribute("class")
    let questionType = (element "#question-type").GetAttribute("class")
    let item = read "#character>span"
    match itemType, questionType with
    | "vocabulary", "reading" -> VocabSpelling item
    | "vocabulary", "meaning" -> VocabMeaning item
    | "kanji", "reading" -> KanjiSpelling item
    | "kanji", "meaning" -> KanjiMeaning item
    | "radical", "meaning" -> RadicalName item
    | _ -> 
        failwith 
        <| sprintf "Couldn't parse this particular review page.\r\ntype = %s, question = %s, item = %s" itemType 
               questionType item
               
let startReview () =
    url "https://www.wanikani.com/review/session"
    Review <| parseReviewPage ()
    
let parseinfo revt =
    match revt with
    | VocabMeaning s | KanjiMeaning s | RadicalName s -> 
        let meaningPanel = element "#item-info-meaning"
        let meaningTitle = read "#iteminfo-meaning>h2"
        let meaningTerms = meaningPanel.Text.Replace(sprintf "<h2>%s</h2>" meaningTitle, "")
        let infoPanel = element "#item-info-meaning-mnemonic"
        let infoTitle = read "#item-info-meaning-mnemonic>h2"
        let info = infoPanel.Text.Replace(sprintf "<h2>%s</h2>" infoTitle, "")
        { LeftCol = [ (meaningTitle, meaningTerms) ]
          RightCol = [ (infoTitle, info) ] }
    | VocabSpelling s | KanjiSpelling s -> 
        let readingTitle = read "#item-info-reading>h2"
        let readingTerms = read "#item-info-reading>span"
        let infoPanel = element "#item-info-reading-mnemonic"
        let infoTitle = read "#item-info-reading-mnemonic>h2"
        let info = infoPanel.Text.Replace(sprintf "<h2>%s</h2>" infoTitle, "")
        { LeftCol = [ (readingTitle, readingTerms) ]
          RightCol = [ (infoTitle, info) ] }

let parseReviewAnswerPage prev =
    click "#option-item-info" //open the info panel
    let success = (element "#answer-form>form>fieldset").GetAttribute("class") = "correct"
    let info = parseinfo prev
    if success then ReviewSuccess(prev, info)
    else ReviewError(prev, info)


let submitReviewAnswer state ans =
    "#user-response" << ans //fill input
    click "html body div#reviews.pure-g-r div.pure-u-1 div#question div#answer-form form fieldset button"
    sleep 1
    parseReviewAnswerPage state
    
let nextReviewPage () =
    click "html body div#reviews.pure-g-r div.pure-u-1 div#question div#answer-form form fieldset button"
    sleep 1
    if currentUrl() = "https://www.wanikani.com/review/session" then
        Review <| parseReviewPage()
    else
        ResultsPage


let renderLines (color : Color) list = List.iter (fun (x : string) -> Console.WriteLine(x, color)) list

let getColor state =
    match state with
    | RadicalName _ -> Color.FromArgb(0, 0xa1, 0xf1)
    | KanjiMeaning _ | KanjiSpelling _ -> Color.FromArgb(0xf1, 0, 0xa1)
    | VocabMeaning _ | VocabSpelling _ -> Color.FromArgb(0xa1, 0, 0xf1)
    
let renderPrompt state =
    match state with
    | RadicalName _ | KanjiMeaning _ | VocabMeaning _ -> Console.ReadLine()
    | KanjiSpelling _ | VocabSpelling _ -> KanaInput.getKanaInput ()
    
let renderQuery state =
    renderLines 
    <| getColor state 
    <| match state with
         | RadicalName s -> [s; "Radical Name"]
         | KanjiMeaning s -> [s; "Meaning"]
         | KanjiSpelling s -> [s; "Reading"]
         | VocabMeaning s -> [s; "Meaning"]
         | VocabSpelling s -> [s; "Reading"]

let renderAndPrompt state =
    do renderQuery state
    do Console.WriteLine()
    renderPrompt state
    
let renderSuccessPage state info =
    let color = Color.FromArgb(0,0xa1,0)
    do renderQuery state    
    List.iter (fun (x,y) -> Console.WriteLine(sprintf "%s: %s" x y, color)) (info.LeftCol@info.RightCol)
let renderErrorPage state info =
    let color = Color.FromArgb(0xa1,0,0)
    do renderQuery state    
    List.iter (fun (x,y) -> Console.WriteLine(sprintf "%s: %s" x y, color)) (info.LeftCol@info.RightCol)  
    
let runPage page =
    match page with 
    | Login | DashBoard | ResultsPage -> page
    | Review rt -> submitReviewAnswer rt <| renderAndPrompt rt
    | ReviewSuccess (rt,info) -> renderSuccessPage rt info
                                 Console.ReadLine() |> ignore
                                 nextReviewPage ()
    | ReviewError (rt,info) ->   renderErrorPage rt info
                                 Console.ReadLine() |> ignore
                                 nextReviewPage ()                              
let runReview () =
    let p = startReview ()
    let rec run page =
        match page with
        | Review _ | ReviewError _ | ReviewSuccess _ -> runPage page
        | _ -> goToDashboard ()
    run p
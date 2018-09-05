module WaniKani

open canopy.runner.classic
open canopy.configuration
open canopy.classic
open Config


type ReviewType = RadicalName of string | KanjiMeaning of string | KanjiSpelling of string | VocabMeaning of string | VocabSpelling of string



type ReviewAnswerInfo = {
        LeftCol : (string*string) list
        RightCol : (string*string) list
    }

//let's write a state machine!
type Page =
    | Login
    | DashBoard
    | Review of ReviewType
    | ReviewSuccess of ReviewType*ReviewAnswerInfo
    | ReviewError of ReviewType*ReviewAnswerInfo



let login creds = 
    url "https://www.wanikani.com/login"
    "#user_login" << creds.username
    "#user_password" << creds.password
    click "html body section.session.login div.wrapper form#new_user.new_user fieldset button.button" //submit
    match currentUrl () with 
    | "https://www.wanikani.com/dashboard" -> DashBoard
    | u ->  do printfn "redirected to %s" u
            do url "https://www.wanikani.com/login" //try login again
            Login

            

let parseReviewPage () =
    let itemType = (element "#character").GetAttribute("class")
    let questionType = (element "#question-type").GetAttribute("class")
    let item = read "#character>span"
    match itemType, questionType with
    | "vocabulary", "reading" -> VocabSpelling item
    | "vocabulary", "meaning" -> VocabMeaning item
    | "kanji", "reading" -> KanjiSpelling item
    | "kanji", "meaning" -> KanjiMeaning item
    | "radical", "meaning" -> RadicalName item
    | _ -> failwith <| sprintf "Couldn't parse this particular review page.\r\ntype = %s, question = %s, item = %s" itemType questionType item

let parseinfo revt =
    match revt with 
    | VocabMeaning s
    | KanjiMeaning s
    | RadicalName s-> let meaningPanel = element "#item-info-meaning"
                      let meaningTitle = read "#iteminfo-meaning>h2"
                      let meaningTerms = meaningPanel.Text.Replace(sprintf "<h2>%s</h2>" meaningTitle, "")
                      let infoPanel = element "#item-info-meaning-mnemonic"
                      let infoTitle = read "#item-info-meaning-mnemonic>h2"
                      let info = infoPanel.Text.Replace(sprintf "<h2>%s</h2>" infoTitle, "")
                      {LeftCol = [(meaningTitle, meaningTerms)]; RightCol = [(infoTitle,info)]}
    | VocabSpelling s
    | KanjiSpelling s -> let readingTitle = read "#item-info-reading>h2"
                         let readingTerms = read "#item-info-reading>span"
                         let infoPanel = element "#item-info-reading-mnemonic"
                         let infoTitle = read "#item-info-reading-mnemonic>h2"
                         let info = infoPanel.Text.Replace(sprintf "<h2>%s</h2>" infoTitle, "")
                         {LeftCol = [(readingTitle, readingTerms)]; RightCol = [(infoTitle,info)]}


let parseReviewAnswerPage prev =
    click "#option-item-info" //open the info panel
    let success = (element "#answer-form>form>fieldset").GetAttribute("class") = "correct"
    let info = parseinfo prev
    if success then
        ReviewSuccess (prev, info)
    else
        ReviewError (prev, info)
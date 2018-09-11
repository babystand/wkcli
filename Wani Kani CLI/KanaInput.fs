module KanaInput

type KanaConverting = string * string

type KanaResult =
    | Kana of string
    | Roma of string
    | Split of string * string

let convertPart (str : string) =
    match str.ToLower() with
    | "a" -> Kana "あ"
    | "i" -> Kana "い"
    | "u" -> Kana "う"
    | "e" -> Kana "え"
    | "o" -> Kana "お"
    | "ka" -> Kana "か"
    | "ki" -> Kana "き"
    | "ku" -> Kana "く"
    | "ke" -> Kana "け"
    | "ko" -> Kana "こ"
    | "ga" -> Kana "が"
    | "gi" -> Kana "ぎ"
    | "gu" -> Kana "ぐ"
    | "ge" -> Kana "げ"
    | "go" -> Kana "ご"
    | "sa" -> Kana "さ"
    | "si" -> Kana "し"
    | "shi" -> Kana "し"
    | "su" -> Kana "す"
    | "se" -> Kana "せ"
    | "so" -> Kana "そ"
    | "za" -> Kana "ざ"
    | "zi" -> Kana "じ"
    | "zhi" -> Kana "じ"
    | "zu" -> Kana "ず"
    | "ze" -> Kana "ぜ"
    | "zo" -> Kana "ぞ"
    | "ta" -> Kana "た"
    | "ti" -> Kana "ち"
    | "chi" -> Kana "ち"
    | "tu" -> Kana "つ"
    | "tsu" -> Kana "つ"
    | "te" -> Kana "て"
    | "to" -> Kana "と"
    | "da" -> Kana "だ"
    | "di" -> Kana "ぢ"
    | "du" -> Kana "づ"
    | "dsu" -> Kana "づ"
    | "dzu" -> Kana "づ"
    | "de" -> Kana "で"
    | "do" -> Kana "ど"
    | "na" -> Kana "な"
    | "ni" -> Kana "に"
    | "nu" -> Kana "ぬ"
    | "ne" -> Kana "ね"
    | "no" -> Kana "の"
    | "ha" -> Kana "は"
    | "hi" -> Kana "ひ"
    | "hu" -> Kana "ふ"
    | "fu" -> Kana "ふ"
    | "he" -> Kana "へ"
    | "ho" -> Kana "ほ"
    | "ba" -> Kana "ば"
    | "bi" -> Kana "び"
    | "bu" -> Kana "ぶ"
    | "be" -> Kana "べ"
    | "bo" -> Kana "ぼ"
    | "pa" -> Kana "ぱ"
    | "pi" -> Kana "ぴ"
    | "pu" -> Kana "ぷ"
    | "pe" -> Kana "ぺ"
    | "po" -> Kana "ぽ"
    | "ma" -> Kana "ま"
    | "mi" -> Kana "み"
    | "mu" -> Kana "む"
    | "me" -> Kana "め"
    | "mo" -> Kana "も"
    | "ya" -> Kana "や"
    | "yu" -> Kana "ゆ"
    | "yo" -> Kana "よ"
    | "ra" -> Kana "ら"
    | "ri" -> Kana "り"
    | "ru" -> Kana "る"
    | "re" -> Kana "れ"
    | "ro" -> Kana "ろ"
    | "wa" -> Kana "わ"
    | "wo" -> Kana "を"
    | "nn" -> Kana "ん"
    | "kya" -> Kana "きゃ"
    | "kyu" -> Kana "きゅ"
    | "kyo" -> Kana "きょ"
    | "sha" -> Kana "しゃ"
    | "shu" -> Kana "しゅ"
    | "sho" -> Kana "しょ"
    | "cha" -> Kana "ちゃ"
    | "chu" -> Kana "ちゅ"
    | "cho" -> Kana "ちょ"
    | "nya" -> Kana "にゃ"
    | "nyu" -> Kana "にゅ"
    | "nyo" -> Kana "にょ"
    | "hya" -> Kana "ひゃ"
    | "hyu" -> Kana "ひゅ"
    | "hyo" -> Kana "ひょ"
    | "mya" -> Kana "みゃ"
    | "myu" -> Kana "みゅ"
    | "myo" -> Kana "みょ"
    | "rya" -> Kana "りゃ"
    | "ryu" -> Kana "りゅ"
    | "ryo" -> Kana "りょ"
    | "gya" -> Kana "ぎゃ"
    | "gyu" -> Kana "ぎゅ"
    | "gyo" -> Kana "ぎょ"
    | "ji" -> Kana "じ"
    | "ja" -> Kana "じゃ"
    | "ju" -> Kana "じゅ"
    | "jo" -> Kana "じょ"
    | "bya" -> Kana "びゃ"
    | "byu" -> Kana "びゅ"
    | "byo" -> Kana "びょ"
    | "pya" -> Kana "ぴゃ"
    | "pyu" -> Kana "ぴゅ"
    | "pyo" -> Kana "ぴょ"
    | (x : string) when x.Length > 1 && x.Chars 0 = 'n' 
                        && not <| List.contains (x.Chars 1) [ 'a'; 'i'; 'u'; 'e'; 'o'; 'y' ] -> 
        Split("ん", x.Substring 1)
    | (x : string) when x.Length > 1 && x.Chars 0 = x.Chars 1 -> Split("っ", x.Substring 1)
    | _ -> Roma str

let convertInput state input =
    let (kana, roma) = state
    let result = convertPart <| roma + input
    match result with
    | Kana s -> (kana + s, "")
    | Roma s -> (kana, s)
    | Split(k, r) -> (kana + k, r)

let backspace (state : KanaConverting) =
    match state with
    | "", "" -> state
    | x, "" -> x.Substring(0, x.Length - 1), ""
    | y, x -> y, x.Substring(0, x.Length - 1)

let flatten (state : KanaConverting) = fst state + snd state

let clearLine() =
    let pos = System.Console.CursorTop
    let w = System.Console.BufferWidth
    let s = String.replicate w " "
    System.Console.SetCursorPosition(0, pos)
    System.Console.Write(s)
    System.Console.SetCursorPosition(0, pos)

let getKanaInput() =
    let init : KanaConverting = "", ""
    
    let rec readKana state =
        clearLine()
        System.Console.Write(flatten state)
        let k = System.Console.ReadKey(true)
        match k.Key with
        | System.ConsoleKey.Enter -> flatten state
        | System.ConsoleKey.Backspace -> backspace state |> readKana
        | _ -> readKana <| convertInput state (k.KeyChar.ToString())
    let res =readKana init
    printfn ""
    res

module Config

open System
open System.IO

type Creds =
    { username : String
      password : String }

let configDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "config", "wkcli")
let configPath = Path.Combine(configDir, "creds")
let ensureConfig() = Directory.CreateDirectory(configDir) |> ignore

let getCreds() =
    let getCredsTerm() =
        let rec readMask pw =
            let k = System.Console.ReadKey()
            match k.Key with
            | System.ConsoleKey.Enter -> pw
            | System.ConsoleKey.Escape -> pw
            | System.ConsoleKey.Backspace -> 
                match pw with
                | [] -> readMask []
                | _ :: t -> 
                    System.Console.Write " \b"
                    readMask t
            | _ -> 
                System.Console.Write "\b*"
                readMask (k.KeyChar :: pw)
        printfn "Username: "
        let u = Console.ReadLine()
        printfn "Password: "
        let p =
            readMask []
            |> Seq.rev
            |> System.String.Concat
        Console.Clear()
        { username = u
          password = p }
    
    let writeCreds creds =
        File.WriteAllLines(configPath, 
                           [| creds.username
                              creds.password
                              |> System.Text.Encoding.UTF8.GetBytes
                              |> Convert.ToBase64String |])
        creds
    
    let readCreds() =
        let lines = File.ReadAllLines configPath
        { username = lines.[0]
          password =
              lines.[1]
              |> Convert.FromBase64String
              |> System.Text.Encoding.UTF8.GetString }
    
    if File.Exists(configPath) then readCreds()
    else 
        ensureConfig()
        getCredsTerm() |> writeCreds


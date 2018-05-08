module FactX.SwiProcess

open System.Diagnostics
open System.Text

let runSwiProc (swiPath:string) (args:string) : (int * string) = 
    let swiInfo = new ProcessStartInfo ()
    swiInfo.FileName <- swiPath
    swiInfo.Arguments <- args
    swiInfo.UseShellExecute <- false
    swiInfo.RedirectStandardOutput <- true

    let swiProc = new Process()
    swiProc.EnableRaisingEvents <- true

    let swiOutput = new StringBuilder ()
    swiProc.OutputDataReceived.AddHandler (
        DataReceivedEventHandler (
            fun _ args -> swiOutput.AppendLine(args.Data) |> ignore
        )
    )
    swiProc.StartInfo <- swiInfo
    swiProc.Start () |> ignore
    swiProc.BeginOutputReadLine()
    swiProc.WaitForExit ()
    (swiProc.ExitCode, swiOutput.ToString() )

let runSwiProc2 (swiPath:string) (args:string) (query:string []) : (int * string) = 
    let swiInfo = new ProcessStartInfo ()
    swiInfo.FileName <- swiPath
    swiInfo.Arguments <- args
    swiInfo.UseShellExecute <- false
    swiInfo.RedirectStandardInput <- true
    swiInfo.RedirectStandardOutput <- true

    let swiProc = new Process()
    swiProc.EnableRaisingEvents <- true

    let swiOutput = new StringBuilder ()
    swiProc.OutputDataReceived.AddHandler (
        DataReceivedEventHandler (
            fun _ args -> swiOutput.AppendLine(args.Data) |> ignore
        )
    )
    swiProc.StartInfo <- swiInfo
    swiProc.Start () |> ignore
    if query.Length > 0 then swiProc.StandardInput.Write(String.concat System.Environment.NewLine query)
    swiProc.StandardInput.Close ()
    swiProc.WaitForExit ()
    let stdout : string = swiProc.StandardOutput.ReadToEnd ()
    (swiProc.ExitCode, stdout )

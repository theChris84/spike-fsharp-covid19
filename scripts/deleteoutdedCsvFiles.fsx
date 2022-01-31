#load ".paket/load/netcoreapp3.1/main.group.fsx"

open FSharp.Data
open System

fsi.AddPrinter<DateTime>(fun d -> d.ToShortDateString())

open System.IO

let files = 
    Directory.GetFiles("./csse_covid_19_data/csse_covid_19_daily_reports")
    |> Seq.filter(fun f -> Path.GetExtension(f) = ".csv" )
    |> Seq.map Path.GetFullPath

let notSupportedCsvfiles = 
    files
    |> Seq.map (fun fp -> fp, CsvFile.Load fp)
    |> Seq.filter (fun (fp, csvf) ->
            match csvf.Headers with 
            | Some h -> h |> Array.exists (fun h -> h = "Province/State")
            | None -> false )
    |> Seq.map (fun (fp, _)  -> fp)
    |> Array.ofSeq


for filedelete in notSupportedCsvfiles do
    File.Delete (filedelete)


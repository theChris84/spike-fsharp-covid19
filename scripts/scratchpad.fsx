#load ".paket/load/net5.0/main.group.fsx"

open FSharp.Data
open System
open System.IO

fsi.AddPrinter<DateTime>(fun d -> d.ToShortDateString())

[<Literal>]
let CovidSampleFile = __SOURCE_DIRECTORY__ + @"/src/server/DailyReportSample.csv"

type DailyCovid = CsvProvider<CovidSampleFile, HasHeaders=true, PreferOptionals=true>

let files = 
    Directory.GetFiles("./data/csse_covid_19_daily_reports/", "*.csv")
    |> Seq.filter (fun f -> 
        let fn = Path.GetFileNameWithoutExtension f
        System.DateTime.ParseExact(fn, "MM-dd-yyyy", null) >= (DateTime(2021,1,1)) )
    |> Seq.map Path.GetFullPath


// list comprehension f#
// let allData = seq { 
//     for file in files do
//     let data = DailyCovid.Load file  
//     yield! data.Rows }

let allData =
    files
    |> Seq.map DailyCovid.Load
    |> Seq.collect (fun data -> data.Rows)
    |> Seq.distinctBy (fun row -> 
        row.Country_Region, 
        row.Province_State,
        row.Last_Update.Date,
        row.Confirmed,
        match row.Province_State with
        | Some ps -> ps
        | None -> "" )
    |> Seq.sortBy (fun row -> row.Last_Update.Date)


let clearCountryNames (country: string) =
    match country.Trim() with    
    | "Russia" -> "Russian Federation"
    | "Iran" -> "Iran (Islamic Republic of)"
    | "Macau" -> "Macao SAR"
    | "Hong Kong" -> "Hong Kong SAR"
    | "Viet Nam" -> "Vietnam"
    | "Palestine" -> "occupied Palestinian territory"
    | "Korea, South" -> "South Korea"
    | "Republic of Korea" -> "South Korea"
    | "Unite States" -> "US"
    | "Mainland China" -> "China"
    | "UK" -> "United Kingdom"
    | "Germany" -> "Deutschland"
    | country -> country

let confirmedByCountryDaily = seq {
    for country, rows in allData |> Seq.groupBy (fun row -> clearCountryNames row.Country_Region ) do
    let countryByData = seq {
        for date, rows in rows |> Seq.groupBy (fun r -> r.Last_Update.Date) do
            // date, rows |> Seq.sumBy (fun r -> r.Confirmed)
            {|  Confirmed = rows |> Seq.sumBy (fun r -> r.Confirmed)
                Death = rows |> Seq.sumBy (fun r -> r.Deaths)
                // Recovered = rows |> Seq.sumBy (fun r -> match r.Recovered with | Some v -> v | None -> 0 )
                Recovered = rows |> Seq.choose (fun r -> r.Recovered) |> Seq.sum
            |}
    }
    country, countryByData 
} 

[<Literal>]
let fileOutTxt = __SOURCE_DIRECTORY__ + "./Testoutput.txt"
let output = confirmedByCountryDaily |> Seq.map (fun (c, data) -> sprintf "%s /t %A" c data)
File.WriteAllLines (fileOutTxt, output) |> ignore

let top10 = 
    confirmedByCountryDaily
    |> Seq.sortByDescending (fun (_, rows) ->  rows |> Seq.map (fun r -> r.Death )  |> Seq.max )
    |> Seq.take 10

open XPlot.Plotly

let makeScatter(country, values) =
    let dates, numbers = values |> Seq.toArray |> Array.unzip
    let trace = Scatter(x = dates, y = numbers ) :> Trace
    trace.name <- country
    trace

top10
|> Seq.map makeScatter
|> Chart.Plot
|> Chart.Show

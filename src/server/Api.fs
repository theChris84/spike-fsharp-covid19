module Api

open FSharp.Data
open System
open System.IO

[<Literal>]
let CovidSampleFile = @"./DailyReportSample.csv"

type DailyCovid = CsvProvider<CovidSampleFile, HasHeaders=true, PreferOptionals=true>

let files = 
    Directory.GetFiles("./../../data/csse_covid_19_daily_reports", "*.csv")
    |> Seq.filter (fun f -> 
        let fn = Path.GetFileNameWithoutExtension f
        System.DateTime.ParseExact(fn, "MM-dd-yyyy", null) >= (DateTime(2021,1,1)) )
    |> Seq.map Path.GetFullPath
    |> Array.ofSeq

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
    |> Array.ofSeq


let clearCountryNames (country: string) =
    match country.Trim() with    
    | "Viet Nam" -> "Vietnam"
    | "Korea, South" -> "South Korea"
    | "Republic of Korea" -> "South Korea"
    | "Unite States" -> "US"
    | "Mainland China" -> "China"
    | "Germany" -> "Deutschland"
    | country -> country

let confirmedByCountryDaily = [|
    for country, rows in allData |> Seq.groupBy (fun row -> clearCountryNames row.Country_Region) do
    let countryByData = 
        [| for date, rows in rows |> Seq.groupBy (fun r -> r.Last_Update.Date) do
            {|  Date = date
                Confirmed = rows |> Seq.sumBy (fun r -> r.Confirmed)
                Deaths = rows |> Seq.sumBy (fun r -> r.Deaths)
                Recovered =  rows |> Seq.choose (fun r -> r.Recovered) |> Seq.sum
            |}
        |]
    country, countryByData 
|]

let countryLookup = confirmedByCountryDaily |> Map
let allCountries = confirmedByCountryDaily |> Array.map fst
let countryStats = [|
    for country, stats in confirmedByCountryDaily do 
        let mostRecent = stats |> Array.tryLast
        match mostRecent with
        | Some mostRecent -> {| mostRecent with Country_Region = country |}
        | None -> ()
|]

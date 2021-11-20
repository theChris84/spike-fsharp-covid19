#load ".paket/load/netcoreapp3.1/main.group.fsx"

open FSharp.Data
open System

fsi.AddPrinter<DateTime>(fun d -> d.ToShortDateString())

[<Literal>]
let ResolutionFolder = __SOURCE_DIRECTORY__ + "/csse_covid_19_data/csse_covid_19_daily_reports/"

//create a typ provider with sample data and specific schema

// type Covidd = CsvProvider<Sample="A, B, C", Schema="A->AA(decimal), B, C->CC(decimal)", HasHeaders=true>
// let myCsv = new Covidd([Covidd.Row(1.0m, "a", 2.0m)])
// let myCsvs = seq {
//         yield! myCsv.Rows |> Seq.map(fun r -> r.CC) }

type Covid = CsvProvider<
                "03-07-2021.csv",
                HasHeaders=true,
                PreferOptionals=true, 
                ResolutionFolder=ResolutionFolder>

open System.IO

let files = 
    Directory.GetFiles("./csse_covid_19_data/csse_covid_19_daily_reports")
    |> Seq.filter(fun f -> Path.GetFileName(f).Contains("2021")  && Path.GetExtension(f) = ".csv" )
    |> Seq.map Path.GetFullPath

// let allData = 
//     files
//     |> Seq.collect (fun f -> 
//                     let data = Covid.Load f
//                     data.Rows )

let allData = seq { 
    for file in files do
    let data = Covid.Load file
    yield! data.Rows }

let confirmedByCountryDaily = seq {
    let byCountry = allData |> Seq.groupBy(fun row -> row.Country_Region)
    for country, rows in byCountry do
    let countryByData = seq {
        let byData = rows |> Seq.groupBy(fun row -> row.Last_Update)
        for date, rows in byData do
            date, rows |> Seq.map(fun row -> row.Confirmed) |> Seq.sum }
    country, countryByData }

open XPlot.Plotly

let top10 = 
    confirmedByCountryDaily
    |> Seq.sortByDescending(fun (country, dates) -> 
        let lastDate, confirmed = dates |> Seq.last 
        confirmed)

let _, data = confirmedByCountryDaily |> Seq.head

data 
|> Chart.Line
|> Chart.Show 



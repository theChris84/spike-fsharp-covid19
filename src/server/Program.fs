open Saturn
open Giraffe

module ApiRoutes =
    let findByCountry country =
        match Api.countryLookup.TryFind country with
        | Some covid -> covid |>json
        | None -> RequestErrors.NOT_FOUND(sprintf $"Unknown country {country}")

    let apiRouts = router {
        get     "/countries" (Api.allCountries |> json)
        getf    "/countries/%s" findByCountry
    }

module UiRouter = 
    open Giraffe.ViewEngine
    open Zanaptak.TypedCssClasses

    [<Literal>]
    let BulmaCssUrl = "https://cdnjs.cloudflare.com/ajax/libs/bulma/0.9.3/css/bulma.css"
    type Bulma = CssClasses<BulmaCssUrl, Naming.PascalCase>
    
    let (++) a b = a + " " + b
    let _classes attributes = attributes |> String.concat " " |> _class

    let createPage title subtitle (contents: XmlNode) : XmlNode  =
        html [] [
            head [] [
                link [ _rel "stylesheet"; _href BulmaCssUrl ]
            ]
            body [] [
                // section [ _class (Bulma.hero ++ Bulma.``is-primary``) ] [
                section [ _classes [Bulma.Hero; Bulma.IsPrimary] ] [
                    div [ _class Bulma.HeroBody ] [
                        div [ _class Bulma.Container ] [
                            h1 [ _class Bulma.Title ] [ Text title ]
                            h2 [ _class Bulma.Subtitle ] [ Text subtitle ]
                        ]
                    ]
                ]
                section [ _class Bulma.Section ] [
                    div [ _class Bulma.Container ] [
                        contents
                    ]                    
                ]
            ]
        ]
    
    let table =
        table [ _class Bulma.Table ] [
            thead [] [
                tr [] [
                    th [] [ Text "Country"  ]
                    th [] [ Text "Deaths"   ]
                    th [] [ Text "Recovered"]
                    th [] [ Text "Confirmed"]
                ]
            ]
            tbody [] [ 
                for row in Api.countryStats do
                    tr [] [
                        th [] [ a [ _href (sprintf "%s" row.Country_Region) ] [ Text row.Country_Region ] ]
                        th [] [ Text (row.Deaths.ToString "N0") ]
                        th [] [ Text (row.Recovered.ToString "N0") ]
                        th [] [ Text (row.Confirmed.ToString "N0") ]
                    ]
            ]
        ]

    let countriesView = 
        createPage "COVID 19 Dataset" "Written in F#" table

    let countryView country = 
        createPage country "Deaths over time" (
            match Api.countryLookup.TryFind country with
            | Some stats ->
                let chart =
                    stats
                    |> Array.map (fun s -> s.Date, s.Deaths)
                    |> XPlot.Plotly.Chart.Line
                chart.GetHtml() |> Text
            | None ->
                Text "No data found for country!")

    let uiRouter = router {
        get "/" (htmlView countriesView)
        getf "/%s" (countryView >> htmlView)  
    }

let appRouter = router {
    forward "/api" ApiRoutes.apiRouts
    forward "" UiRouter.uiRouter }

let myApp = application {
    use_json_serializer (Thoth.Json.Giraffe.ThothSerializer())
    use_router appRouter
}

run myApp

(*
    http://localhost:5000/api/countries
    http://localhost:5000/api/countries/US
    http://localhost:5000/api/countries/Deutschland
    http://localhost:5000/api/countries/Unknown
*)
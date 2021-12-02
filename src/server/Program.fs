open Saturn
open Giraffe
open Microsoft.AspNetCore.HttpOverrides

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

    let createPage contents =
        html [] [
            body [] contents
        ]
    
    let contriesView = 
        createPage [
            table [] [
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
                            th [] [ Text row.Country_Region ]
                            th [] [ Text (row.Deaths.ToString "N0") ]
                            th [] [ Text (row.Recovered.ToString "N0") ]
                            th [] [ Text (row.Confirmed.ToString "N0") ]
                        ]
                ]
            ]
        ]

    let uiRouter = htmlView contriesView

let appRouter = router {
    forward "/api" ApiRoutes.apiRouts
    forward "" UiRouter.uiRouter}

let myApp = application {
    use_json_serializer (Thoth.Json.Giraffe.ThothSerializer())
    use_router appRouter
}

run myApp

(*
    http://localhost:5000/api/countries
    http://localhost:5000/api/countries/US
    http://localhost:5000/api/countries/Unknown
*)
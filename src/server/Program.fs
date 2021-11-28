open Saturn
open Giraffe

let findByCountry country =
    match Api.countryLookup.TryFind country with
    | Some covid -> covid |>json
    | None -> RequestErrors.NOT_FOUND(sprintf $"Unknown country {country}")

let apiRouts = router {
    get     "/api/countries" (Api.allCountries |> json)
    getf    "/api/countries/%s" findByCountry
}

let myApp = application {
    use_json_serializer (Thoth.Json.Giraffe.ThothSerializer())
    use_router apiRouts
}

run myApp

(*
    http://localhost:5000/api/countries
    http://localhost:5000/api/countries/US
    http://localhost:5000/api/countries/Unknown
*)
let maxDepth = 
    Seq.map (function  '(' -> 1 | ')' -> -1 | _ -> 0 )
    >> Seq.scan (+) 0
    >> Seq.max

let count = maxDepth "(2+3)+(2*4((+4)(1+2))/3)+1" |> Array.ofSeq


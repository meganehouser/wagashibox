namespace WagashiBoxLib

open System
open WagashiBoxLib
open WagashiBoxLib.EasyDrawCommand

module Program =
    [<EntryPoint>]
    let main args =
        let random = new Random()  
        WagashiBox.Show {Title="random line"; Width=500.0; Height=400.0}
            
        wagashi {
            do! background Colors.Black

            do! stroke Colors.GreenYellow
            do! line(0.0, 200.0, 500.0, 200.0)

            do! stroke Colors.AliceBlue
            do! strokeWidth 2.0

            seq{0.0 .. 5.0 .. 500.0}
            |> Seq.fold(fun (lastX, lastY) x ->
                let y = lastY + (random.NextDouble() * 30.0 - 15.0)
                wagashi { do! line(lastX, lastY, x, y) }
                (x, y)
            ) (0.0, 200.0)
            |> ignore
        }
        0
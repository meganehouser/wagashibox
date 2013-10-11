namespace WagashiBoxLib

open System.Windows
open System.Windows.Controls
open System.Windows.Media

type Point = {X:float; Y:float}
type Size = {W:float; H:float}

type Wagashi () =
    let window = Window(SizeToContent=SizeToContent.WidthAndHeight)
    do window.Content <- new Canvas()
    static member Create() = new Wagashi()

    member this.Title
        with get() = window.Title
        and set(value) = window.Title <- value

    member this.Width
        with get() = (window.Content :?> Canvas).Width
        and set(value) = (window.Content :?> Canvas).Width <- value
    
    member this.Height
        with get() = (window.Content :?> Canvas).Height
        and set(value) = (window.Content :?> Canvas).Height <- value

    member this.Background
        with get() = (window.Content :?> Canvas).Background
        and set(value) = (window.Content :?> Canvas).Background <- value

    member this.Show () = window.Show()

    member this.AddShape(shape, point) =
        let canvas = window.Content :?> Canvas
        canvas.Children.Add(shape) |> ignore
        Canvas.SetLeft(shape, point.X)
        Canvas.SetTop(shape, point.Y)
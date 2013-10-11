namespace WagashiBoxLib

open System
open System.Windows
open System.Windows.Controls
open System.Windows.Threading
open System.Threading


type WagashiStyle = {Title: String; Width: float; Height: float}

type WagashiBuilder(dispatcher: Dispatcher, wgsh: Lazy<Wagashi>) =
    let mutable ctx = Context.init()
    member this.Bind (x, f) = dispatcher.Invoke(fun () -> x wgsh.Value ctx); f()
    member this.Bind (x, f) = ctx <- x ctx; f()
    member this.Zero () = ()
    member this.Return x = x
    member this.Delay(f) = f()

type WagashiBox private () =
    static let app, dispatcher = 
        let application = lazy(new Application())
        let autoEvent = new AutoResetEvent(false)
        let thread = 
            new Thread(fun () ->
                let value = application.Force()
                autoEvent.Set() |> ignore
                value.Run() |> ignore)
        thread.SetApartmentState(ApartmentState.STA)
        thread.Start()
        autoEvent.WaitOne() |> ignore
        application.Value, application.Value.Dispatcher

    static let wagashi = lazy(Wagashi.Create())

    static let builder = new WagashiBuilder(dispatcher, wagashi)
    static member Show(style: WagashiStyle) = 
        dispatcher.Invoke(fun () -> 
            let w = wagashi.Value
            w.Title <- style.Title
            w.Width <- style.Width
            w.Height <- style.Height
            w.Show())
    static member Builder = new WagashiBuilder(dispatcher, wagashi)

[<AutoOpen>]
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module WagashiBox =
    let wagashi = WagashiBox.Builder
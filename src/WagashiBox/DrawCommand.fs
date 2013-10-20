namespace WagashiBoxLib

open System.Windows.Controls
open System.Windows.Media
open System.Windows.Shapes

type Context = {
        ForeColor:Color Option
        FillColor:Color Option
        StrokeWidth:float
        Offset: Point
        Scale: Point
        Skew: Point
        Rotation:float
    }

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Context =
    let init () = {
            ForeColor=None
            FillColor=None
            StrokeWidth=1.0
            Offset={X=0.0; Y=0.0}
            Scale={X=1.0; Y=1.0}
            Skew={X=0.0; Y=0.0}
            Rotation=0.0
        }

module DrawCommand =
    let private drawCanvas (wagashi:Wagashi) (shape: Shape) point ctx =
        let group = new TransformGroup()
        group.Children.Add(new RotateTransform(ctx.Rotation))
        group.Children.Add(new TranslateTransform(ctx.Offset.X, ctx.Offset.Y))
        group.Children.Add(new SkewTransform(ctx.Skew.X, ctx.Skew.Y))
        group.Children.Add(new ScaleTransform(ctx.Scale.X, ctx.Scale.Y))
        shape.RenderTransform <- group
        wagashi.AddShape(shape, point)
        
    let private setShapeColors (shape: Shape) fore fill =
        match fore with
        | Some color -> shape.Stroke <- SolidColorBrush(color)
        | None -> ()
        match fill with
        | Some color -> shape.Fill <- SolidColorBrush(color)
        | None -> ()

    /// 線を描く
    let line (start:Point) (``end``: Point) (wgs:Wagashi) (ctx:Context) =
        match ctx.ForeColor with
        | Some color -> 
            let line = new Line()
            line.X1 <- start.X
            line.X2 <- ``end``.X
            line.Y1 <- start.Y
            line.Y2 <- ``end``.Y
            line.Stroke <- new SolidColorBrush(color)
            line.StrokeThickness <- ctx.StrokeWidth
            drawCanvas wgs line {X=0.0; Y=0.0} ctx
        | None -> ()
        
    /// 四角形を描く
    let rect (start: Point) (size: Size) (wgs: Wagashi) (ctx: Context) =
        match ctx.ForeColor, ctx.FillColor with
        | None, None -> ()
        | fore, fill ->
            let rect = new Rectangle()
            rect.Width <- size.W
            rect.Height <- size.H
            rect.StrokeThickness <- ctx.StrokeWidth
            setShapeColors rect fore fill
            drawCanvas wgs rect start ctx

    /// 円を描く
    let oval (start: Point) (size: Size) (wgs: Wagashi) (ctx: Context) =
        match ctx.ForeColor, ctx.FillColor with
        | None, None -> ()
        | fore, fill ->
            let ellipse = new Ellipse()
            ellipse.Width <- size.W
            ellipse.Height <- size.H
            ellipse.StrokeThickness <- ctx.StrokeWidth
            setShapeColors ellipse fore fill
            drawCanvas wgs ellipse start ctx

    /// 多角形を描く
    let polygon (points: Point list) (wgs: Wagashi) (ctx: Context) =
        match ctx.ForeColor, ctx.FillColor with
        | None, None -> ()
        | fore, fill ->
            let polygon = new Polygon()
            let ptColl = new PointCollection()
            points |> List.iter(fun pt -> ptColl.Add(new System.Windows.Point(pt.X, pt.Y)))
            polygon.Points <- ptColl
            polygon.StrokeThickness <- ctx.StrokeWidth
            setShapeColors polygon fore fill
            drawCanvas wgs polygon {X=0.0; Y=0.0} ctx

    /// 連続直線を描く
    let polyline (points: Point list)  (wgs: Wagashi) (ctx: Context) =
        match ctx.ForeColor, ctx.FillColor with
        | None, None -> ()
        | fore, fill ->
            let line = new Polyline()
            let ptColl = new PointCollection()
            points |> List.iter(fun pt -> ptColl.Add(new System.Windows.Point(pt.X, pt.Y)))
            line.Points <- ptColl
            line.StrokeThickness <- ctx.StrokeWidth
            setShapeColors line fore fill
            line.StrokeThickness <- ctx.StrokeWidth
            drawCanvas wgs line {X=0.0; Y=0.0} ctx

    /// 背景色の塗りつぶし
    let background (color:Color) (wgs: Wagashi) (ctx: Context) =
        wgs.Background <- new SolidColorBrush(color)

    /// 図形の平行移動
    let translate point ctx = {ctx with Offset={X=point.X; Y=point.Y}}
    
    /// 回転角度の指定
    let rotate degrees ctx = {ctx with Rotation=degrees}
    
    /// 図形の拡大・縮小
    let scale point ctx = {ctx with Scale={X=point.X; Y=point.Y}}


module EasyDrawCommand =
    /// 線を描く
    let line(x1, y1, x2, y2) (wgs:Wagashi) (ctx:Context) =
        DrawCommand.line {X=x1; Y=y1} {X=x2; Y=y2} wgs ctx

    /// 四角形を描く
    let rect(x, y, width, height) (wgs:Wagashi) (ctx: Context) =
        DrawCommand.rect {X=x; Y=y} {W=width; H=height} wgs ctx

    /// 円を描く
    let oval(x, y, width, height) (wgs:Wagashi) (ctx: Context) =
        DrawCommand.oval {X=x; Y=y} {W=width; H=height} wgs ctx

    /// 多角形を描く
    let polygon (points: (float * float) list) (wgs: Wagashi) (ctx: Context) =
        let pts = points |> List.map(fun (x, y) -> {X=x; Y=y})
        DrawCommand.polygon(pts) wgs ctx
    
    /// 連続直線を描く
    let polyline (points: (float * float) list) (wgs: Wagashi) (ctx: Context) =
        let pts = points |> List.map(fun (x, y) -> {X=x; Y=y})
        DrawCommand.polyline(pts) wgs ctx

    /// 背景色の塗りつぶし
    let background = DrawCommand.background

    /// 図形の平行移動
    let translate (x, y) ctx = DrawCommand.translate {X=x; Y=y} ctx
    
    /// 回転角度の指定
    let rotate = DrawCommand.rotate
    
    /// 図形の拡大・縮小
    let scale (x, y) ctx = DrawCommand.scale {X=x; Y=y} ctx

[<AutoOpen>]
module ColorState =
    /// 塗りつぶし色の指定
    let fill color ctx = {ctx with FillColor=Some(color)}

    /// 描画色の指定
    let stroke color ctx = {ctx with ForeColor=Some(color)}

    /// 塗りつぶしなしに指定
    let nofill () ctx = {ctx with FillColor=None}

    /// 描画色なしに指定
    let noStroke () ctx = {ctx with ForeColor=None}

    /// 描画幅の指定
    let strokeWidth width ctx = {ctx with StrokeWidth=width}

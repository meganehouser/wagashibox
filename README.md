# WagashiBox
F#でVisualizationする適当なやつ。
ただしWPFのShapeを使っているので重い。

## 使い方
wagashiのコンピュテーション式内で絵を描くコマンドを呼んでWagashiBox.showを呼び出す。

##　コマンド
NodeBoxを参考にしたコマンドが使えます。

### 描画
```
// 線を引く
line(x1, y1, x2, y2)

// 四角形を描く
rect(x, y, width, height)

// 円を描く
oval(x, y, width, height)

// 多角形を描く
polygon(points: (float * float) list)

// 連続直線を描く
polyline(points: (float * float) list)

// 塗りつぶし色の指定
fill color

// 描画色の指定
let stroke color

// 塗りつぶしなしに指定
let nofill ()

// 描画色なしに指定
let noStroke ()

// 描画幅の指定
let strokeWidth width

// 図形の平行移動
let translate point

// 回転角度の指定
let rotate degrees

// 図形の拡大・縮小
let scale point
```

## 色
RGB, HSB, HUSL色空間が使用できる予定です。


## 例
```
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
```
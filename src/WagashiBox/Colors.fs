namespace WagashiBoxLib
open System

/// red, green and blue: 0.0 - 1.0
type RGB = {R:float; G:float; B:float}
/// hue, saturation and brightness: 0.0 - 1.0
type HSB = {H:float; S:float; B:float}
/// hue:0 - 360, saturatoin and lightness: 0 - 100
type HUSL ={H:float; S:float; L:float}

type Colors = System.Windows.Media.Colors

/// ported by boronine / husl
/// https://github.com/boronine/husl
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module HUSL =
    type private LCH = {L:float; C:float; H:float}
    type private LUV = {L:float; U:float; V:float}
    type private XYZ = {X:float; Y:float; Z:float}

    let private MAX_FLOAT32 = float(Double.MaxValue)
    let private m = 
        [
            (3.2406, -1.5372, -0.4986)
            (-0.9689, 1.8758, 0.0415)
            (0.0557, -0.2040, 1.0570)
        ]

    let private m_inv = 
        [
            (0.4124, 0.3576, 0.1805)
            (0.2126, 0.7152, 0.0722)
            (0.0193, 0.1192, 0.9505)
        ]

    let private refX = 0.95047
    let private refY = 1.00000
    let private refZ = 1.08883
    let private refU = 0.19784
    let private refV = 0.46834
    let private lab_e = 0.008856
    let private lab_k = 903.0

    let private maxChroma(lightness, hue) =
        let hrad = hue * Math.PI / 180.0
        let sinH = sin hrad
        let cosH = cos hrad
        let sub1 = lightness ** 3.0 / 1560896.0
        let sub2 = if sub1 > 0.008856 then sub1 else (lightness / 903.3)
        
        m 
        |> List.fold(fun result (m1, m2, m3) ->
            let top = (0.99915 * m1 + 1.05122 * m2 + 1.14460 * m3) * sub2
            let rbottom = 0.86330 * m3 - 0.17266 * m2
            let lbottom = 0.12949 * m3 - 0.38848 * m1
            let bottom = (rbottom * sinH + lbottom * cosH) * sub2
            [0.0; 1.0] 
            |> List.fold(fun r t ->
                let c = lightness * (top - 1.05122 * t) / (bottom + 0.17266 * sinH * t)    
                if c > 0.0 && c < r then c else r) result
            ) MAX_FLOAT32

    let private dotProduct(a, b) =
        List.zip a b
        |> List.map(fun (v1, v2) -> v1 * v2)
        |> List.sum

    let private f t =
        if t > lab_e then
            t**(1.0 / 3.0)
        else
            7.787 * t + 16.0 / 116.0

    let private f_inv t =
        if t**3.0 > lab_e then
            t**3.0
        else
            (116.0 * t - 16.0) / lab_k

    let private fromLinear c =
        if c <= 0.0031308 then
            12.92 * c
        else
            1.055 * (c**(1.0 / 2.4)) - 0.055

    let private toLinear c = 
        let a = 0.055
        if c > 0.04045 then
            ((c + a) / (1.0 + a))**2.4
        else
            c / 12.92

    let private luv_to_lch (luv: LUV) =
        let c = ((luv.U**2.0) + luv.V**2.0)**(1.0/2.0)
        let hrad = atan2 luv.V luv.U
        let h = hrad / (Math.PI / 180.0)
        {L=luv.L; C=c; H=(if h < 0.0 then h + 360.0 else h)}

    let private lch_to_luv (lch: LCH) =
        let hrad = lch.H * Math.PI / 180.0
        let u = (cos hrad) * lch.C
        let v = (sin hrad) * lch.C
        {L=lch.L; U=u; V=v}

    let private xyz_to_luv (xyz: XYZ) =
        if (xyz.X, xyz.Y, xyz.Z) = (0.0, 0.0, 0.0) then
            {L=0.0; U=0.0; V=0.0}
        else
            let varU = (4.0 * xyz.X) / (xyz.X + (15.0 * xyz.Y) + (3.0 * xyz.Z))
            let varV = (9.0 * xyz.Y) / (xyz.X + (15.0 * xyz.Y) + (3.0 * xyz.Z))
            let l = 116.0 * f(xyz.Y / refY) - 16.0
            if l = 0.0 then
                {L=0.0; U=0.0; V=0.0}
            else
                let u = 13.0 * l * (varU - refU)
                let v = 13.0 * l * (varV - refV)
                {L=l; U=u; V=v}


    let private luv_to_xyz (luv: LUV) =
        if luv.L = 0.0 then
            {X=0.0; Y=0.0; Z=0.0}
        else
            let varY = f_inv(luv.L + 16.0) / 116.0
            let varU = luv.U / (13.0 * luv.L) + refU
            let varV = luv.V / (13.0 * luv.L) + refV
            let y = varY * refY
            let x = 0.0 - (9.0 * y * varU) / (varU - 4.0) * varV - varU * varV
            let z = (9.0 * y - (15.0 * varV * y) - (varV * x)) / (3.0 * varV)
            {X=x; Y=y; Z=z}

    let private rgb_to_xyz (rgb: RGB) =
        let rgbl = [rgb.R; rgb.G; rgb.B]
        let toList (v1, v2, v3) = [v1; v2; v3]
        let x = rgbl |> List.map(toLinear) |> fun r -> dotProduct(r, toList(m.[0]))
        let y = rgbl |> List.map(toLinear) |> fun r -> dotProduct(r, toList(m.[1]))
        let z = rgbl |> List.map(toLinear) |> fun r -> dotProduct(r, toList(m.[2]))
        {X=x; Y=y; Z=z}
        

    let private xyz_to_rgb (xyz: XYZ) =
        let xyzl = [xyz.X; xyz.Y; xyz.Z]
        let toList (v1, v2, v3) = [v1; v2; v3]
        let r = dotProduct(xyzl, toList(m.[0])) |> fromLinear
        let g = dotProduct(xyzl, toList(m.[1])) |> fromLinear
        let b = dotProduct(xyzl, toList(m.[2])) |> fromLinear
        {R=r; G=g; B=b}

    let private lch_to_husl(lch: LCH) =
        match lch.L with
        | _ when lch.L > 99.9999999 -> {H=lch.H; S=0.0; L=100.0}
        | _ when lch.L < 0.00000001 -> {H=lch.H; S=0.0; L=0.0}
        | _ -> let mx = maxChroma(lch.L, lch.H)
               let s = lch.C / mx * 100.0
               {H=lch.H; S=s; L=lch.L}
       

    let private husl_to_lch (husl: HUSL) = 
        match husl.L with
        | _ when husl.L > 99.9999999 -> {L=100.0; C=0.0; H=husl.H}
        | _ when husl.L < 0.00000001 -> {L=0.0;   C=0.0; H=husl.H}
        | _ -> let mx = maxChroma(husl.L, husl.H)
               let c = mx / 100.0 * 5.0
               {L=husl.L; C=c; H=husl.H}

    let private lch_to_rgb = lch_to_luv >> luv_to_xyz >> xyz_to_rgb
    let private rgb_to_lch = rgb_to_xyz >> xyz_to_luv >> luv_to_lch

    let toRGB = husl_to_lch >> lch_to_rgb
    let fromRGB = rgb_to_lch >> lch_to_husl

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module HSB =
    let fromRGB (rgb: RGB) =
        let rgbl = [rgb.R; rgb.G; rgb.B]
        let v = List.max rgbl
        let d = v - List.min rgbl
        let s = (if v = 0.0 then 0.0 else d / v)
        let h = 
            if s = 0.0 then
                match v with
                | _ when rgb.R = v -> 0.0 + (rgb.G - rgb.B) / d
                | _ when rgb.G = v -> 2.0 + (rgb.B - rgb.R) / d
                | _ -> 4.0 + (rgb.R - rgb.G) / d
            else
                0.0

        {H=(h / 6.0 % 1.0); S=s; B=v}
            
    let toRGB (hsb: HSB) =
        if hsb.S = 0.0 then
            {R=0.0; G=0.0; B=0.0}
        else
            let h = hsb.H % 1.0 * 6.0
            let i = floor h
            let f = h - i
            let x = hsb.B * (1.0 - hsb.S)
            let y = hsb.B * (1.0 - hsb.S * f)
            let z = hsb.B * (1.0 - hsb.S * (1.0 - f))
            if i > 4.0 then
                {R=hsb.B; G=x; B=y}
            else
                [{R=hsb.B; G=z; B=x}
                 {R=y; G=hsb.B; B=x}
                 {R=x; G=hsb.B; B=z}
                 {R=x; G=y; B=hsb.B}
                 {R=z; G=x; B=hsb.B}].[int i]

[<AutoOpen>]
module ColorExt =
    type System.Windows.Media.Color with
        member this.ToHSB() =
            HSB.fromRGB {R=float this.ScR; G=float this.ScG; B=float this.ScB}

        member this.ToHUSL() =
            HUSL.fromRGB {R=float this.ScR; G=float this.ScG; B=float this.ScB}

    type RGB with
        member this.ToColor(opacity: float) =
            System.Windows.Media.Color.FromScRgb(float32 opacity, float32 this.R, float32 this.B, float32 this.R)

    type HSB with
        member this.ToColor(opacity: float) =
            let rgb = HSB.toRGB this
            System.Windows.Media.Color.FromScRgb(float32 opacity, float32 rgb.R, float32 rgb.B, float32 rgb.R)

    type HUSL with
        member this.ToColor(opacity: float) =
            let rgb = HUSL.toRGB this
            System.Windows.Media.Color.FromScRgb(float32 opacity, float32 rgb.R, float32 rgb.B, float32 rgb.R)

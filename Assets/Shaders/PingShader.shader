Shader "Unlit/PingShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _AlphaMask ("Alpha Mask", 2D) = "white" {}
        _Color("Color", Color) = (1, 1, 1, 1)
        _Speed("Speed", Range(0,10)) = 3
        _ResolutionX("Resolution X", Range(0,10)) = 1
        _ResolutionY("Resolution Y", Range(0,10)) = 1
        _Direction("Inward (-1) / Outward (1)", Int) = 1
    }
    SubShader
    {
        Tags {"Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent"}
        LOD 100

        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _AlphaMask;
            float4 _Color;
            fixed _Speed;
            fixed _ResolutionX;
            fixed _ResolutionY;
            fixed _Direction;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // --- Credit ---
                // algorithm used is described here: http://adrianboeing.blogspot.com/2011/01/webgl-tunnel-effect-explained.html?m=1
                // --------------

                float2 resolution = float2(_ResolutionX, _ResolutionY);
                float2 p = -1.0 + 2.0 * i.uv.xy / resolution;

                float a = atan2(p.y, p.x);

                float power = 1.0;

                // square tunnel
                float r = sqrt(dot(p, p)); 
                // OR
                // cylinder tunnel
                //float r = pow(pow(p.x * p.x, power) + pow(p.y * p.y, power), 1.0 / (2 * power));

                float2 uv;
                uv.x = 0.1 / r + _Time.y * _Speed * _Direction; // x uv increases with time, times our speed
                uv.y = a / (3.1416);

                // sample the texture
                //fixed4 col = tex2D(_MainTex, uv) * tex2D(_AlphaMask, i.uv);
                float centeredX = i.uv.x - 0.5;
                float centeredY = i.uv.y - 0.5;

                // The AlphaMask uses the original UV coordinates
                // The MainTex however uses our new, warped cylinder tunnel UV coordinates
                fixed4 col = tex2D(_MainTex, uv) * tex2D(_AlphaMask, i.uv);

                // Clip anything outside of the circle this square creates
                clip(1 - sqrt(centeredX * centeredX + centeredY * centeredY) - 0.5);

                // Apply the color and return
                return col * _Color;
            }
            ENDCG
        }
    }
}

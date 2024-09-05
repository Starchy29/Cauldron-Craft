Shader "Unlit/OutlineShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color("Color", Color) = (1, 1, 1, 1)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }
        LOD 100
        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

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
            fixed4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 outlineColor = fixed4(1, 0, 0, 1);
                const float pixelWidth = 1.0 / 32.0;
                fixed4 color = tex2D(_MainTex, i.uv);
                if(color.a > 0) {
                    return color;
                }

                fixed4 left = tex2D(_MainTex, i.uv + float2(-pixelWidth, 0));
                if(left.a > 0) {
                    return _Color;
                }

                fixed4 right = tex2D(_MainTex, i.uv + float2(pixelWidth, 0));
                if(right.a > 0) {
                    return _Color;
                }

                fixed4 up = tex2D(_MainTex, i.uv + float2(0, pixelWidth));
                if(up.a > 0) {
                    return _Color;
                }

                fixed4 down = tex2D(_MainTex, i.uv + float2(0, -pixelWidth));
                if(down.a > 0) {
                    return _Color;
                }
                
                return color;
            }
            ENDCG
        }
    }
}

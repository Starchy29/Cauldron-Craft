Shader "Unlit/YellShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }
        LOD 100

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
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
                float2 radius : TEXCOORD1;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
                float radius : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.radius = v.radius.x;
                o.color = v.color;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float radius = i.radius;
                if(radius == 0.0f) {
                    discard;
                }

                float dx = i.uv.x - 0.5f;
                float dy = i.uv.y - 0.5f;
                float dist = sqrt(dx * dx + dy * dy);

                float dissipation = 10 * saturate(dist - 0.4f);
                    
                float angle = atan2(dy, dx);
                float mult = radius * 2;
                radius += 0.05 * mult * sin(12 * angle) * sin(12 * angle);
                radius += 0.03 * mult * sin(19 * angle) * sin(20 * angle);
                radius += 0.01 * mult * sin(29 * angle) * sin(30 * angle);

                float edgeDelta = abs(dist - radius);

                return fixed4(0, 0, 0, saturate(1 - 5 * edgeDelta - dissipation));
            }
            ENDCG
        }
    }
}

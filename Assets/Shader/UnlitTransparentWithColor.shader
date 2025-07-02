Shader "Custom/AspectManualContained"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _TexWidth ("Texture Width (0-1)", Float) = 1
        _TexHeight ("Texture Height (0-1)", Float) = 1
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Cull Back // Only render front faces
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _Color;
            float _TexWidth;
            float _TexHeight;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float2 centered = i.uv - 0.5;
                centered /= float2(_TexWidth, _TexHeight);
                float2 uv = centered + 0.5;

                if (uv.x < 0 || uv.x > 1 || uv.y < 0 || uv.y > 1) discard;

                float4 col = tex2D(_MainTex, uv);
                return col * _Color;
            }
            ENDCG
        }
    }
}

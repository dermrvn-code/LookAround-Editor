Shader "Outlined/RimOnlyOutline"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _OutlineColor("Outline Color", Color) = (1, 0, 0, 0.5)
        _OutlineWidth("Outline Width", Range(0.0, 2.0)) = 0.15
    }

    CGINCLUDE
    #include "UnityCG.cginc"

    struct appdata {
        float4 vertex : POSITION;
        float3 normal : NORMAL;
    };

    uniform float4 _OutlineColor;
    uniform float _OutlineWidth;
    ENDCG

    SubShader
    {
        // Pass 1: Write object shape to stencil
        Pass
        {
            Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
            ColorMask 0
            ZWrite Off
            Stencil
            {
                Ref 1
                Comp always
                Pass replace
            }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            float4 vert(appdata v) : SV_POSITION
            {
                return UnityObjectToClipPos(v.vertex);
            }

            fixed4 frag() : SV_Target
            {
                return 0;
            }
            ENDCG
        }

        // Pass 2: Draw only the expanded outline outside the stencil
        Pass
        {
            Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back
            Stencil
            {
                Ref 1
                Comp notEqual
                Pass keep
            }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            struct v2f {
                float4 pos : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v.vertex.xyz += normalize(v.normal) * _OutlineWidth;
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                return _OutlineColor;
            }
            ENDCG
        }

        // Pass 3: Transparent main object
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        CGPROGRAM
        #pragma surface surf Lambert alpha:fade

        struct Input {
            float2 uv_MainTex;
        };

        void surf(Input IN, inout SurfaceOutput o)
        {
            o.Albedo = 0;
            o.Alpha = 0;
        }
        ENDCG
    }

    Fallback Off
}

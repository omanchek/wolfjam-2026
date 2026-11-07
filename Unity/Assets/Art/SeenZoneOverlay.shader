Shader "Custom/SeenZoneOverlay"
{
    Properties
    {
        _MainTex ("Image", 2D) = "white" {}
        _Color ("Color", Color) = (0, 0, 0, 1)
        _Center ("Center", Vector) = (0, 0, 0, 0)
        _Radius ("Radius", Float) = 0
        _Softness ("Softness", Float) = 1
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        ZTest Always
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            fixed4 _Color;
            float4 _Center;
            float _Radius;
            float _Softness;

            struct v2f
            {
                float4 position : SV_POSITION;
                float2 worldPosition : TEXCOORD0;
                float2 uv : TEXCOORD1;
            };

            v2f vert(appdata_base v)
            {
                v2f o;
                o.position = UnityObjectToClipPos(v.vertex);
                o.worldPosition = mul(unity_ObjectToWorld, v.vertex).xy;
                o.uv = v.texcoord.xy;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float distanceToCenter = distance(i.worldPosition, _Center.xy);
                float cover = smoothstep(_Radius - _Softness, _Radius, distanceToCenter);
                fixed4 image = tex2D(_MainTex, i.uv) * _Color;
                return fixed4(image.rgb, image.a * cover);
            }
            ENDCG
        }
    }
}

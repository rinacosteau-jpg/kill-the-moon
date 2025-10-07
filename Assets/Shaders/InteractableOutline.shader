Shader "Custom/InteractableOutline" {
    Properties {
        _OutlineColor ("Outline Color", Color) = (1, 1, 1, 1)
        _OutlineThickness ("Outline Thickness", Range(0.0, 0.2)) = 0.02
        _OutlineEnabled ("Outline Enabled", Range(0, 1)) = 0
    }
    SubShader {
        Tags { "RenderType" = "Opaque" "Queue" = "Transparent" }
        Cull Front
        ZWrite Off
        ZTest LEqual
        Blend SrcAlpha OneMinusSrcAlpha

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float4 _OutlineColor;
            float _OutlineThickness;
            float _OutlineEnabled;

            struct appdata {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f {
                float4 position : SV_POSITION;
            };

            v2f vert(appdata v) {
                v2f o;

                float3 viewNormal = mul((float3x3)UNITY_MATRIX_IT_MV, v.normal);
                float3 viewPos = mul(UNITY_MATRIX_MV, v.vertex).xyz;

                float2 extrudeDir = viewNormal.xy;
                float dirMagnitude = dot(extrudeDir, extrudeDir);

                if (dirMagnitude < 1e-5f) {
                    extrudeDir = viewPos.xy;
                    dirMagnitude = dot(extrudeDir, extrudeDir);
                }

                if (dirMagnitude < 1e-5f)
                    extrudeDir = float2(1.0f, 0.0f);

                extrudeDir = normalize(extrudeDir);

                float extrude = _OutlineThickness * _OutlineEnabled;
                viewPos.xy += extrudeDir * extrude;

                float4 clipPos = mul(UNITY_MATRIX_P, float4(viewPos, 1.0f));
                o.position = clipPos;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target {
                fixed4 color = _OutlineColor;
                color.a *= _OutlineEnabled;
                return color;
            }
            ENDCG
        }
    }
}

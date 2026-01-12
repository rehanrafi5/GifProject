Shader "XD Paint/Brush Blur"
{
    Properties
    {
        _MainTex ("Main", 2D) = "white" {}
        _MaskTex ("Mask", 2D) = "white" {}
    }
    
    SubShader
    {
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
        Cull Off Lighting Off ZTest Off ZWrite Off Fog { Color (0,0,0,0) }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #pragma fragmentoption ARB_precision_hint_fastest
            #include "UnityCG.cginc"
 
            sampler2D _MainTex;
            sampler2D _MaskTex;

            float4 frag (v2f_img i) : SV_Target
            {
                float4 color = tex2D(_MainTex, i.uv);
                float4 colorMask = tex2D(_MaskTex, i.uv);
                color.a *= colorMask.a;
			    return color;
            }
            ENDCG
        }
    }
}
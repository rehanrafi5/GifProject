Shader "XD Paint/Brush Clone"
{
    Properties
    {
        _MainTex ("Main", 2D) = "white" {}
        _MaskTex ("Mask", 2D) = "white" {}
        _Offset ("Brush offset", Vector) = (0, 0, 0, 0)
    }
    
    SubShader
    {
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
        Cull Off Lighting Off ZTest Off ZWrite Off Fog { Color (0,0,0,0) }
        Blend One OneMinusSrcAlpha, SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #pragma fragmentoption ARB_precision_hint_fastest
            #include "UnityCG.cginc"
 
            sampler2D _MainTex;
            sampler2D _MaskTex;
            uniform float4 _Offset;

            float4 frag (v2f_img i) : SV_Target
            {
                float2 uv = i.uv + _Offset.xy;
                float4 color = tex2D(_MainTex, uv);
                float4 colorMask = tex2D(_MaskTex, i.uv);
                color.a *= colorMask.a;
                // step(x < y) ? 1 : 0, which returns 1 or 0 depending on whether y is bigger than x
	    		return color * !(step(1.0f, uv.x) || !step(0.0f, uv.x) || step(1.0f, uv.y) || !step(0.0f, uv.y));
            }
            ENDCG
        }
    }
}
Shader"Hidden/GizmoDepthAware"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest ("ZTest", Int) = 4
        [Enum(On,1,Off,0)] _ZWrite ("ZWrite", Int) = 1
    }
    
    SubShader
    {
        Tags 
        { 
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
        }
        
        Pass
        {
Blend SrcAlpha
OneMinusSrcAlpha
ZWrite [_ZWrite]
ZTest [_ZTest]
Cull Off

Lighting Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            
#include "UnityCG.cginc"
            
struct appdata
{
    float4 vertex : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
};
            
struct v2f
{
    float4 vertex : SV_POSITION;
                UNITY_VERTEX_OUTPUT_STEREO
};
            
            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
            UNITY_INSTANCING_BUFFER_END(Props)
            
v2f vert(appdata v)
{
    v2f o;
                
    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                
    o.vertex = UnityObjectToClipPos(v.vertex);
    return o;
}
            
fixed4 frag(v2f i) : SV_Target
{
    return UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
}
            ENDCG
        }
    }
}
Shader "Custom/J_StencilShader"
{
    Properties
    {
        [IntRange] _stencilID("Stencil ID", Range(0, 255)) = 0 // Stencil buffer is from 0 to 255
    }

    SubShader
    {
        // Automatically render this before transparent objects
        Tags {"Queue" = "Geometry" "RenderType" = "Opaque" "Queue"="Geometry-1" }

        Pass
        {
            // Before the pass, objects should not be visible
            Blend Zero One
            // Don't write to depth buffer
            ZWrite Off
            ZTest Always  // Change this
            Cull Back

            Stencil
            {
                Ref [_stencilID]
                Comp Always // Always passes
                Pass Replace // If the stencil buffer passes, we replace the current pixel's shader with this one
                Fail Keep // If either test fails, just keep whatever was in the stencil buffer for this pixel
            }
        }
    }
}

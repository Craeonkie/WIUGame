void CalculateShadows_float(float3 worldPosition, float3 worldNormal, float4 screenPos, out float shadowAttenuation)
{
    shadowAttenuation = 0;
    
    #ifdef SHADERGRAPH_PREVIEW
      shadowAttenuation = 1;
    #else
        #if defined(UNIVERSAL_PIPELINE_CORE_INCLUDED)
            float2 screenUV = screenPos.xy / screenPos.w;

            InputData inputData = (InputData)0;
            inputData.positionWS = worldPosition;
            inputData.normalWS = worldNormal;
            inputData.shadowCoord = TransformWorldToShadowCoord(worldPosition);
            inputData.normalizedScreenSpaceUV = screenUV;
    
    
            inputData.shadowCoord = TransformWorldToShadowCoord(worldPosition);
    
            // 2. Main Light
            Light mainLight = GetMainLight(inputData.shadowCoord);
            shadowAttenuation = mainLight.distanceAttenuation * mainLight.shadowAttenuation;

            // 3. Additional Lights Loop
            uint pixelLightCount = GetAdditionalLightsCount();
        
            // Use these specific macros to support Forward+
            LIGHT_LOOP_BEGIN(pixelLightCount)
                // Note: Forward+ automatically handles 'lightIndex' inside this macro
                Light light = GetAdditionalLight(lightIndex, inputData.positionWS, half4(1,1,1,1));
            
                shadowAttenuation = 1;
            LIGHT_LOOP_END
    
    
    
    
    
            //float4 shadowCoord = TransformWorldToShadowCoord(worldPosition);
            //Light mainLight = GetMainLight();
            ////direction = mainLight.direction;
            ////color = mainLight.color;
            //ShadowSamplingData shadowSamplingData = GetMainLightShadowSamplingData();
            //half shadowStrength = GetMainLightShadowStrength();
            //shadowAttenuation += mainLight.distanceAttenuation * SampleShadowmap(shadowCoord, TEXTURE2D_ARGS(_MainLightShadowmapTexture, sampler_MainLightShadowmapTexture), shadowSamplingData, shadowStrength, false);
        
            //uint lightCount = GetAdditionalLightsCount();
            //for (uint i = 0; i < lightCount; ++i) {
            //    Light light = GetAdditionalLight(i, worldPosition);
    
            //    half addShadow = AdditionalLightRealtimeShadow(i, worldPosition, light.direction);
            //    shadowAttenuation += light.distanceAttenuation * addShadow;
            //}
    
        #else
            //direction = normalize(float3(-0.7, 0.7, -0.7));
            //color = float3(1, 1, 1);
            shadowAttenuation = 1;
        #endif
    #endif
}
    
void CalculateMainLight_float(float3 worldPos, float3 worldNormal, float shadowAmt, out float3 direction, out float3 color, out float shadowAttenuation, out float normals)
{
    shadowAttenuation = 0;
    normals = 0.0;

    #ifdef SHADERGRAPH_PREVIEW
      direction = normalize(float3(-0.7, 0.7, -0.7));
      color = float3(1,1,1);
      shadowAttenuation = 1;
    #else
        #if defined(UNIVERSAL_PIPELINE_CORE_INCLUDED)
            float4 shadowCoord = TransformWorldToShadowCoord(worldPos);
            Light mainLight = GetMainLight(shadowCoord);
            direction = mainLight.direction;
    
            float effectiveShadow = lerp(1.0, mainLight.shadowAttenuation, shadowAmt);
            float attenuation = mainLight.distanceAttenuation * effectiveShadow;
    
            color = mainLight.color * attenuation;
            shadowAttenuation = (mainLight.color.r + mainLight.color.g + mainLight.color.b) * attenuation;
            normals = (dot(worldNormal, mainLight.direction) * 0.5 + 0.5) * mainLight.distanceAttenuation;
        #else
            direction = normalize(float3(-0.7, 0.7, -0.7));
            color = float3(1, 1, 1);
            shadowAttenuation = 1;
        #endif
    #endif
}

void CalculateAdditionalLights_float(float shadowAttenuationDistance, float smoothness, float3 worldPos, float3 worldNormal, float3 worldView, float4 screenPos, float mainDiffuse, float3 mainSpecular, float3 mainColor, float mainShadowAttenuation, float shadowAmt, out float diffuse, out float3 specular, out float3 color, out float shadowAttenuation, out float normals)
{
    diffuse = mainDiffuse;
    specular = mainSpecular;
    color = mainColor * (mainDiffuse + mainSpecular);
    shadowAttenuation = 1.0;
    normals = 0;
    
    #ifndef SHADERGRAPH_PREVIEW
    shadowAttenuation = mainShadowAttenuation * (mainDiffuse + mainSpecular);

        #if defined(UNIVERSAL_PIPELINE_CORE_INCLUDED)
            InputData inputData = (InputData)0;
            inputData.positionWS = worldPos;
            inputData.normalWS = worldNormal;
            inputData.shadowCoord = TransformWorldToShadowCoord(worldPos);
            inputData.normalizedScreenSpaceUV = screenPos;
    
            uint pixelLightCount = GetAdditionalLightsCount();
    
            LIGHT_LOOP_BEGIN(pixelLightCount)
                // get light color and direction
                //lightIndex = GetPerObjectLightIndex(lightIndex);
                //Light light = GetAdditionalPerObjectLight(lightIndex, worldPos);
                Light light = GetAdditionalLight(lightIndex, worldPos);
                
                // calculate shadows
                light.shadowAttenuation = AdditionalLightRealtimeShadow(lightIndex, worldPos, light.direction);
                float effectiveShadow = lerp(1.0, light.shadowAttenuation, shadowAmt);
                float attenuation = light.distanceAttenuation * effectiveShadow;
    
                // calculate diffuse and specular
                float NdotL = saturate(dot(worldNormal, light.direction));
                float thisDiffuse = attenuation * NdotL;
                float3 thisSpecular = LightingSpecular(thisDiffuse, light.direction, worldNormal, worldView, 1, smoothness);
                
                // accumulate light
                diffuse += thisDiffuse;
                specular += thisSpecular;
                color += light.color * (thisDiffuse + thisSpecular);
                shadowAttenuation += (light.color.r + light.color.g + light.color.b) * attenuation * (thisDiffuse + thisSpecular) * shadowAttenuationDistance;
                normals += (dot(worldNormal, light.direction) * 0.5 + 0.5) * light.distanceAttenuation;
            LIGHT_LOOP_END
            
            float total = diffuse + dot(specular, float3(0.3333, 0.3333, 0.3333));
            color = total <= 0 ? mainColor : color / total;            
#endif
    #endif
}
void CalculateAdditionalLights_float(float3 worldPos, float3 worldNormal, float4 screenPos, out float3 color)
{
    #ifdef SHADERGRAPH_PREVIEW
      color = float3(1,1,1);
    #else
        #if defined(UNIVERSAL_PIPELINE_CORE_INCLUDED)
            //Light tempLight = GetAdditionalLight(0, worldPos);
            //direction = tempLight.direction;
            //color = tempLight.color;
            //shadowAttenuation = AdditionalLightRealtimeShadow(0, worldPos, tempLight.direction) * tempLight.distanceAttenuation;
    
            InputData inputData = (InputData)0;
            inputData.positionWS = worldPos;
            inputData.normalWS = worldNormal;
            inputData.shadowCoord = TransformWorldToShadowCoord(worldPos);
            inputData.normalizedScreenSpaceUV = screenPos;
    
            uint pixelLightCount = GetAdditionalLightsCount();
    
            LIGHT_LOOP_BEGIN(pixelLightCount)
                Light light = GetAdditionalLight(lightIndex, worldPos);
                
                color += light.color * light.distanceAttenuation * AdditionalLightRealtimeShadow(lightIndex, worldPos, light.direction);
            LIGHT_LOOP_END
        #else
            color = float3(1, 1, 1);
        #endif
    #endif
}



void GetAdditionalLightsCount_float(out float lightCount)
{
    #ifdef SHADERGRAPH_PREVIEW
      lightCount = 1;
    #else
        #if defined(UNIVERSAL_PIPELINE_CORE_INCLUDED)
            lightCount = (float)GetAdditionalLightsCount();
        #else
            lightCount = 1;
        #endif
    #endif
}

void CalculateAdditionalLightAtIndex_float(float lightIndex, float3 worldPos, out float3 direction, out float3 color, out float shadowAttenuation, out float lightIndexAgain)
{
    shadowAttenuation = 0;

    #ifdef SHADERGRAPH_PREVIEW
            direction = normalize(float3(-0.7, 0.7, -0.7));
            color = float3(1,1,1);
            shadowAttenuation = 1;
        #else
            #if defined(UNIVERSAL_PIPELINE_CORE_INCLUDED)
                uint pixelLightCount = GetAdditionalLightsCount();
    
                if (lightIndex >= pixelLightCount)
                {
                    direction = normalize(float3(-0.7, 0.7, -0.7));
                    color = float3(1, 1, 1);
                    shadowAttenuation = 1;
                    return;
                }

                Light additionalLight = GetAdditionalLight(lightIndex, worldPos);
                direction = additionalLight.direction;
                color = additionalLight.color * additionalLight.distanceAttenuation;
                shadowAttenuation = AdditionalLightRealtimeShadow(lightIndex, worldPos, additionalLight.direction) * additionalLight.distanceAttenuation;
            #else
                direction = normalize(float3(-0.7, 0.7, -0.7));
                color = float3(1, 1, 1);
                shadowAttenuation = 1;
            #endif
    #endif
}
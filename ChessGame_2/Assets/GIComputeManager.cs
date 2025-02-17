using UnityEngine;

public class GIComputeManager : MonoBehaviour {
    public ComputeShader giComputeShader;
    public Texture albedoTexture;
    public Texture normalTexture;
    public Texture positionTexture;
     public RenderTexture outputTexture;

    [System.Serializable]
    public struct PointLight {
        public Vector3 position;
        public float intensityEV;
        public Vector3 color;
        public float padding; // Ensure alignment
    }

    public PointLight[] pointLights;

    private ComputeBuffer giOutputBuffer;
    private ComputeBuffer pointLightBuffer;

    void OnGUI()
    {
        // Display the result texture
        GUI.DrawTexture(new Rect(0, 0, 256, 256), outputTexture, ScaleMode.ScaleToFit, false, 1,Color.red,0,0);
    }

    void Start() {
        int resolutionX = albedoTexture.width;
        int resolutionY = albedoTexture.height;

         if (outputTexture == null)
        {
            // If no texture is assigned, create one
            outputTexture = new RenderTexture(256, 256, 24);
            outputTexture.enableRandomWrite = true;
            outputTexture.Create();
        }

        // Initialize output buffer
        giOutputBuffer = new ComputeBuffer(resolutionX * resolutionY, sizeof(float) * 4);

        // Setup point lights buffer
        pointLightBuffer = new ComputeBuffer(pointLights.Length, sizeof(float) * 8); // 8 floats per light
        pointLightBuffer.SetData(pointLights);

        // Configure compute shader
        int kernel = giComputeShader.FindKernel("CSMain");
        giComputeShader.SetTexture(kernel, "_AlbedoTexture", albedoTexture);
        giComputeShader.SetTexture(kernel, "_NormalTexture", normalTexture);
        giComputeShader.SetTexture(kernel, "_PositionTexture", positionTexture);
        giComputeShader.SetBuffer(kernel, "_GIOutput", giOutputBuffer);
        giComputeShader.SetBuffer(kernel, "_PointLights", pointLightBuffer);
        
        giComputeShader.SetInt("_NumLights", pointLights.Length);
        giComputeShader.SetInt("_ResolutionX", resolutionX);
        giComputeShader.SetInt("_ResolutionY", resolutionY);
        giComputeShader.SetInt("_SamplesPerPixel", 16); // Adjust for higher quality
        giComputeShader.SetTexture(kernel, "_GIOutput", outputTexture);
        // Dispatch compute shader
        giComputeShader.Dispatch(kernel, resolutionX / 8, resolutionY / 8, 1);
    }

    void OnDestroy() {
        // giOutputBuffer.Release();
        // pointLightBuffer.Release();
    }

    
}

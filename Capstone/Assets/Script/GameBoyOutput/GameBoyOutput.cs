using UnityEngine;

public class GameBoyOutput : MonoBehaviour
{
    [Header("Assign in Inspector")]
    public Camera gameBoyCamera;
    public RenderTexture renderTexture;
    public Material quadMaterial;
    public Texture2D paletteTexture;

    void Start()
    {
        // Ensure RenderTexture exists
        if (renderTexture != null)
        {
            gameBoyCamera.targetTexture = renderTexture;
            quadMaterial.SetTexture("_RenderTexture", renderTexture);
        }

        // Ensure palette exists
        if (paletteTexture != null)
        {
            quadMaterial.SetTexture("_Palette", paletteTexture);
        }
    }
}


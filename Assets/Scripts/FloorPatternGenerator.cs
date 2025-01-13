using UnityEngine;

public class FloorPatternGenerator : MonoBehaviour
{
    [Range(128, 1024)]
    public int textureSize = 512;

    [Range(2, 16)]
    public int patternSize = 8;

    public Color primaryColor = new Color(0.2f, 0.2f, 0.2f);
    public Color secondaryColor = new Color(0.4f, 0.4f, 0.4f);
    public Color accentColor = new Color(0.6f, 0.6f, 0.6f);

    void Start()
    {
        GeneratePattern();
    }

    void GeneratePattern()
    {
        Texture2D texture = new Texture2D(textureSize, textureSize);
        float scale = textureSize / patternSize;

        for (int x = 0; x < textureSize; x++)
        {
            for (int y = 0; y < textureSize; y++)
            {
                float xNorm = x / scale;
                float yNorm = y / scale;

                // Hex pattern
                float hexOffset = (Mathf.Floor(yNorm) % 2) * 0.5f;
                float hexX = xNorm + hexOffset;

                // Ana desen
                float pattern1 = Mathf.PerlinNoise(hexX * 0.5f, yNorm * 0.5f);
                float pattern2 = Mathf.PerlinNoise(hexX * 1.5f, yNorm * 1.5f);

                // Kenar efektleri
                float edgeX = Mathf.Abs((xNorm % 1) - 0.5f) * 2;
                float edgeY = Mathf.Abs((yNorm % 1) - 0.5f) * 2;
                float edge = Mathf.Max(edgeX, edgeY);

                Color pixelColor;

                if (pattern1 > 0.6f)
                {
                    pixelColor = Color.Lerp(primaryColor, accentColor, pattern2);
                }
                else if (edge > 0.8f)
                {
                    pixelColor = secondaryColor;
                }
                else
                {
                    pixelColor = Color.Lerp(primaryColor, secondaryColor, pattern2);
                }

                // Hafif doku ekleme
                float noise = Mathf.PerlinNoise(x * 0.1f, y * 0.1f) * 0.1f;
                pixelColor = Color.Lerp(pixelColor, accentColor, noise);

                texture.SetPixel(x, y, pixelColor);
            }
        }

        texture.Apply();
        texture.filterMode = FilterMode.Trilinear;

        // Material'e uygula
        Renderer renderer = GetComponent<Renderer>();
        renderer.material.mainTexture = texture;

        // Tiling ayarlarý
        renderer.material.mainTextureScale = new Vector2(2, 2);
    }

    // Editor'da test etmek için
    void OnValidate()
    {
        if (Application.isPlaying)
        {
            GeneratePattern();
        }
    }
}
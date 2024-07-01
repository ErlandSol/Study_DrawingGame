using UnityEngine;
using UnityEngine.SceneManagement;

public static class DrawUtils
{
    public static Vector2Int IndexToPixelPosition(int index, int width)
    {
        int x = index % width; // Calculate x position
        int y = index / width; // Calculate y position
        return new Vector2Int(x, y);
    }


    public static int PixelPositionToIndex(int x, int y, int width)
    {
        return x + width * y;
    }

    public static void RestartScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex);
    }

    public static float Map(float value, float startFrom, float startTo, float endFrom, float endTo)
    {
        return endFrom + (value - startFrom) * (endTo - endFrom) / (startTo - startFrom);
    }

    public static Color32 AddColors(Color32 baseColor, Color32 overlayColor)
    {
        float alphaFactor = overlayColor.a / 255f;

        byte r = (byte)Mathf.Clamp(baseColor.r + overlayColor.r * alphaFactor, 0, 255);
        byte g = (byte)Mathf.Clamp(baseColor.g + overlayColor.g * alphaFactor, 0, 255);
        byte b = (byte)Mathf.Clamp(baseColor.b + overlayColor.b * alphaFactor, 0, 255);

        // Decide how to handle the alpha channel of the resulting color
        // Here we choose to keep the alpha of the base color
        byte a = baseColor.a;

        return new Color32(r, g, b, a);
    }

}

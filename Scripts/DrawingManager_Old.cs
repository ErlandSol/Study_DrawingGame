using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DrawingManager_Old : MonoBehaviour
{
    [SerializeField] Material material;
    Texture2D generatedTexture, originalTexture, previousTexture;

  
    [SerializeField] Transform topLeftCorner;
    [SerializeField] Transform bottomRightCorner;
    [SerializeField] Transform point;
    [SerializeField] int brushSize = 4;
    [SerializeField] int textureWidth = 0;
    [SerializeField] int textureHeight = 0;
    [SerializeField] int resolution = 2000;

    bool pressedLastFrame = false;
    [SerializeField] bool useInterpolation = true;
    Vector2Int lastPos = Vector2Int.zero;
    Camera cam;
    Vector2 size;
    float top, bottom, left, right;
    Color[] originalColorMap;
    Color[] previousColorMap;
    Color[] colorMap;
    Color[] tempColorMap;
    bool[] lockedPixels;
    [SerializeField] Color brushColor;
    Vector2Int pixelPos = new Vector2Int();

    int xPixel = 0;
    int yPixel = 0;

    [SerializeField] bool erase = false;

    float Map(float value, float startFrom, float startTo, float endFrom, float endTo)
    {
        return endFrom + (value - startFrom) * (endTo - endFrom) / (startTo - startFrom);
    }

    public Vector2Int GetPixelPosition(int index, int width)
    {
        int x = index % width; // Calculate x position
        int y = index / width; // Calculate y position

        return new Vector2Int( x, y );
    }

    public int PixelPositionToIndex(int x, int y, int width)
    {
        return x + width * y;
    }

    public void RestartScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex);
    }


    void CalculateArea()
    {
        size.x = Mathf.Abs(topLeftCorner.transform.position.x - bottomRightCorner.transform.position.x);
        size.y = Mathf.Abs(topLeftCorner.transform.position.z - bottomRightCorner.transform.position.z);
        Debug.Log(size * resolution);
        textureWidth = (int)(size.x * resolution);
        textureHeight = (int)(size.y * resolution);
        top = topLeftCorner.transform.position.y;
        bottom = bottomRightCorner.transform.position.y;
        left = topLeftCorner.transform.position.x;
        right = bottomRightCorner.transform.position.x;
    }

    private void Start()
    {
        CalculateArea();
        cam = GetComponent<Camera>();
        colorMap = new Color[textureWidth * textureHeight];
        originalColorMap = new Color[textureWidth * textureHeight];
        previousColorMap = new Color[textureWidth * textureHeight];
        tempColorMap = new Color[textureWidth * textureHeight];
        lockedPixels = new bool[textureWidth * textureHeight];
        generatedTexture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false);
        //originalTexture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false);
        //previousTexture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false);

        generatedTexture.filterMode = FilterMode.Bilinear;

        material.SetTexture("_BaseMap", generatedTexture);
        ResetColor();
        colorMap.CopyTo(originalColorMap,0);
        colorMap.CopyTo(previousColorMap,0);

        //originalTexture.SetPixels(generatedTexture.GetPixels());
        //previousTexture.SetPixels(generatedTexture.GetPixels());
        //originalTexture.Apply();
        //previousTexture.Apply();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            colorMap = (Color[])previousColorMap.Clone();
            //colorMap = new Color[textureWidth * textureHeight];
            //previousColorMap.CopyTo(colorMap, 0);
            //generatedTexture.SetPixels(previousTexture.GetPixels());
            //generatedTexture.Apply();
            SetTexture();
        }
        if (Input.GetKeyDown(KeyCode.P)) RestartScene();
        MoveCursor();

        if (Input.GetKeyDown(KeyCode.E)) erase = !erase;
   

        if (Input.GetMouseButton(0))
        {
            if (useInterpolation && pressedLastFrame && (lastPos.x != xPixel || lastPos.y != yPixel))
            {
                //int dist = (int)Mathf.Sqrt(Mathf.Pow(xPixel - lastPos.x, 2) + Mathf.Pow(yPixel - lastPos.y, 2))
                int dist = (int)Mathf.Sqrt((xPixel - lastPos.x) * (xPixel - lastPos.x) + (yPixel - lastPos.y) * (yPixel - lastPos.y));
                for (int i = 1; i <= dist; i++)
                {
                    //DrawPixel((i * xPixel + (dist - i) * lastPos.x) / dist, (i * yPixel + (dist - i) * lastPos.y) / dist, brushColor);
                    if (erase) EraseFilledCircle(new Vector2Int((i * xPixel + (dist - i) * lastPos.x) / dist, (i * yPixel + (dist - i) * lastPos.y) / dist), brushSize);
                    else DrawFilledCircle(new Vector2Int((i * xPixel + (dist - i) * lastPos.x) / dist, (i * yPixel + (dist - i) * lastPos.y) / dist), brushSize, brushColor);
                }
            }
            else
            {
                if (erase) EraseFilledCircle(new Vector2Int(xPixel, yPixel), brushSize);
                else DrawFilledCircle(new Vector2Int(xPixel, yPixel), brushSize, brushColor);
            }
            SetTexture();
            pressedLastFrame = true;
            lastPos.x = xPixel;
            lastPos.y = yPixel;
        }
        else
        {
            pressedLastFrame = false; 
            previousColorMap = (Color[])colorMap.Clone();
            Array.Fill(lockedPixels, false);
            //colorMap.CopyTo(previousColorMap, 0);
        }
    }


    void MoveCursor()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out hit)) return;
        if (hit.transform.tag != "Page") return;
        point.transform.position = hit.point;
        xPixel = (int)Map(hit.point.x, left, right, 0, textureWidth);
        yPixel = (int)Map(hit.point.y, bottom, top, 0, textureHeight);
    }


    void DrawPixel(int x, int y, Color color)
    {
        if ((x <= textureWidth) && (y <= textureHeight))
        {
            int index = PixelPositionToIndex(x, y, textureWidth);
            //if (lockedPixels[index]) return;
            Color newCol = new Color();
            lockedPixels[index] = true;
            newCol = (colorMap[index] * (1 - color.a)) + (color * color.a);
            //if (color.a > tempColorMap[index].a) tempColorMap[index].a = color.a;
            colorMap[index] = newCol;
        }
    }
    void ErasePixel(int x, int y)
    {
        if ((x <= textureWidth) && (y < textureHeight)) {
            int index = PixelPositionToIndex(x, y, textureWidth);
            //originalColorMap[index] = Color.red;
            colorMap[index] = originalColorMap[index];
            //generatedTexture.SetPixel(x, y, originalTexture.GetPixel(x, y));
        }
    }

    void DrawBrush(int x, int y, int size, int hardness, Color color)
    {
        int i = x - brushSize + 1;
        int j = y - brushSize + 1;
        int maxi = x + brushSize - 1;
        int maxj = y + brushSize - 1;

        if (i < 0) i = 0;
        if (j < 0) j = 0;
        if (maxi >= textureWidth) maxi = textureWidth - 1;
        if (maxj >= textureHeight) maxj = textureHeight - 1;

        for (int X = i; X <= maxi; X++) {
            for (int Y = j; Y <= maxj; Y++) {
                if (Mathf.Pow(X - x, 2) + Mathf.Pow(Y - y, 2) <= Mathf.Pow(size, 2)) {
                    DrawPixel(xPixel, yPixel, brushColor);
                }
            }
        }
    }

    void SetTexture()
    {
        generatedTexture.SetPixels(colorMap);
        generatedTexture.Apply();
    }

    private void DrawFilledCircle(Vector2Int center, int radius, Color color)
    {
        float sqrRadius = radius * radius;
        for (int y = -radius; y <= radius; y++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                float distance = x * x + y * y;

                if (distance <= sqrRadius)
                {
                    int xPos = Mathf.Clamp(center.x + x, 0, textureWidth - 1);
                    int yPos = Mathf.Clamp(center.y + y, 0, textureHeight - 1);
                    float alpha = 1f - Mathf.Clamp01(distance / sqrRadius); // Alpha falloff based on distance
                    Color newColor = new Color(color.r, color.g, color.b, alpha);
                    DrawPixel(xPos, yPos, newColor);
                }
            }
        }
    }

    private void EraseFilledCircle(Vector2Int center, int radius)
    {
        for (int y = -radius; y <= radius; y++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                if (x * x + y * y <= radius * radius)
                {
                    ErasePixel(center.x + x, center.y + y);
                }
            }
        }
    }

    void ResetColor()
    {
        /*
        for (int i = 0; i < colorMap.Length; i++)
        {
            Vector2Int pixelPos = GetPixelPosition(i, textureWidth);
            Color newColor = new Color((float)pixelPos.x / (float)textureWidth, (float)pixelPos.y / (float)textureHeight, 0f);
            colorMap[i] = newColor;
            //originalColorMap[i] = newColor;
        }
        */

        Array.Fill(colorMap, Color.white);
        SetTexture();
    }
}

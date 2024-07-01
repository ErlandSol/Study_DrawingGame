using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using static DrawUtils;

public class DrawingManager : MonoBehaviour
{
    [SerializeField] Transform topLeftCorner, bottomRightCorner, point;
    [SerializeField] Material material;
    Texture2D texture;

    [SerializeField] int brushSize = 4;
    [SerializeField] int pixelDensity = 2000;
    [SerializeField] Color brushColor;
    [SerializeField] float brushHardness;
    Vector2Int resolution;
    Vector2 physicalSize;
    Color32[] pixels;
    Color32[] pixelsTemporary;
    Color32[] pixelsDraw;
    Color32[] originalPixels;

    float top, bottom, left, right;

    Vector2Int pixelPosition;
    Vector2Int lastPixelPosition;

    bool pressedDrawLastFrame = false;

    enum Tools
    {
        Brush,
        Eraser
    }
    
    Tools tool = Tools.Brush;

    private void Start()
    {
        //material = GetComponent<Material>();
        
        physicalSize.x = Mathf.Abs(topLeftCorner.transform.position.x - bottomRightCorner.transform.position.x);
        physicalSize.y = Mathf.Abs(topLeftCorner.transform.position.z - bottomRightCorner.transform.position.z);

        resolution.x = (int)(physicalSize.x * pixelDensity);
        resolution.y = (int)(physicalSize.y * pixelDensity);

        top = topLeftCorner.transform.position.y;
        bottom = bottomRightCorner.transform.position.y;
        left = topLeftCorner.transform.position.x;
        right = bottomRightCorner.transform.position.x;

        pixels = new Color32[resolution.x * resolution.y];
        pixelsTemporary = new Color32[resolution.x * resolution.y];
        pixelsDraw = new Color32[resolution.x * resolution.y];

        texture = new Texture2D(resolution.x, resolution.y, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;
        material.SetTexture("_BaseMap", texture);
        Clear();
        //pixels.CopyTo(originalColorMap, 0);
        //pixels.CopyTo(previousColorMap, 0);


    }

    private void FixedUpdate()
    {
        MoveCursor();
        if (Input.GetKeyDown(KeyCode.E)) tool = Tools.Eraser;
        if (Input.GetKeyDown(KeyCode.D)) tool = Tools.Brush;
        if (Input.GetKeyDown(KeyCode.P)) RestartScene();
        if (Input.GetMouseButton(0))
        {
            Draw();
        }
        else
        {
            pressedDrawLastFrame = false;

           
           
            //Array.Fill(pixelsTemporary, new Color32(255,255,255,0));
        }

    }

    void MoveCursor()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out hit)) return;
        if (hit.transform.tag != "Page") return;
        point.transform.position = hit.point;
        pixelPosition.x = (int)Map(hit.point.x, left, right, 0, resolution.x);
        pixelPosition.y = (int)Map(hit.point.y, bottom, top, 0, resolution.y);
    }

    void Draw()
    {
        if (pressedDrawLastFrame && (lastPixelPosition.x != pixelPosition.x || lastPixelPosition.y != pixelPosition.y))
        {
            int dist = (int)Mathf.Sqrt((pixelPosition.x - lastPixelPosition.x) * (pixelPosition.x - lastPixelPosition.x) + (pixelPosition.y - lastPixelPosition.y) * (pixelPosition.y - lastPixelPosition.y));
            for (int i = 1; i <= dist; i++)
            {
                DrawBrush(
                    (i * pixelPosition.x + (dist - i) * lastPixelPosition.x) / dist, 
                    (i * pixelPosition.y + (dist - i) * lastPixelPosition.y) / dist, 
                    brushSize, brushColor, brushHardness);
            }
        }
        else
        {
            if (lastPixelPosition.x != pixelPosition.x || lastPixelPosition.y != pixelPosition.y)
                DrawBrush(pixelPosition.x, pixelPosition.y, brushSize, brushColor, brushHardness);
        }
        UpdateTexture();
        pressedDrawLastFrame = true;
        lastPixelPosition = pixelPosition;
    }

    void DrawBrush(int xCenter, int yCenter, int radius, Color32 color, float hardness)
    {
        hardness = Mathf.Clamp(hardness, 0.01f, 2.0f);
        float sqrRadius = radius * radius;
        for (int y = -radius; y <= radius; y++) {
        for (int x = -radius; x <= radius; x++) {
            int distance = x * x + y * y;

            if (distance <= sqrRadius)
            {
                int xPos = Mathf.Clamp(xCenter + x, 0, resolution.x - 1);
                int yPos = Mathf.Clamp(yCenter + y, 0, resolution.y - 1);
                float alpha = Mathf.Pow(1f - Mathf.Clamp01((float)distance / sqrRadius), hardness);
                byte alphaByte = (byte)(alpha * 255); // Convert alpha to byte
                Color32 newColor = new Color32(color.r, color.g, color.b, (byte)Mathf.Min(alphaByte, color.a));
                if (tool == Tools.Brush) DrawPixel(xPos, yPos, newColor);
                if (tool == Tools.Eraser) ErasePixel(xPos, yPos);   
            }
        }
        }
    }
    void DrawPixel(int x, int y, Color32 color)
    {
        if ((x <= resolution.x) && (y <= resolution.y))
        {
            int index = PixelPositionToIndex(x, y, resolution.x);
            Color32 newColor = (Color32)((Color)pixelsTemporary[index] * (1 - ((Color)color).a) + (((Color)color) * ((Color)color).a));

            //if (color.a > pixelsTemporary[index].a)
            pixelsTemporary[index] = newColor;
            //newCol = (Color32)col;

            //newCol = (colorMap[index] * (1 - color.a)) + (color * color.a);
            //Color32 newCol;
            //newCol.r = (byte)((byte)(pixelsTemporary[index].r * ()) + (byte)(color.r * color.a));
            //newCol.g = (byte)((byte)(pixelsTemporary[index].g * ()) + (byte)(color.g * color.a));
            //newCol.b = (byte)((byte)(pixelsTemporary[index].b * ()) + (byte)(color.b * color.a));
            //pixelsTemporary[index] = AddColors(pixelsTemporary[index], color);

            //pixelsTemporary[index] = color;
            //newCol = (colorMap[index] * (1 - color.a)) + (color * color.a);

        }
    }
    void ErasePixel(int x, int y)
    {
        if ((x <= resolution.x) && (y <= resolution.y))
        {
            int index = PixelPositionToIndex(x, y, resolution.x);
            //originalColorMap[index] = Color.red;
            pixelsTemporary[index] = originalPixels[index];
            //generatedTexture.SetPixel(x, y, originalTexture.GetPixel(x, y));
        }
    }


    void Clear()
    {
        Array.Fill(pixels, new Color32(255,255,255,255));
        Array.Fill(pixelsTemporary, new Color32(255, 255, 255, 0));
        UpdateTexture();
    }
    void UpdateTexture()
    {
        /*
        for (int i = 0; i < pixels.Length; i++)
        {
            //pixelsDraw[i] = AddColors(pixels[i], pixelsTemporary[i]);


            Color32 newColor = (Color32)((Color)pixels[i] * (1 - ((Color)pixelsTemporary[i]).a) + (((Color)pixelsTemporary[i]) * ((Color)pixelsTemporary[i]).a));

            pixelsDraw[i] = newColor;

            //pixelsDraw[i].r = (byte)Mathf.Clamp((pixels[i].r + pixelsTemporary[i].r),0,255);
            //pixelsDraw[i].g = (byte)Mathf.Clamp((pixels[i].g + pixelsTemporary[i].g),0,255);
            //pixelsDraw[i].b = (byte)Mathf.Clamp((pixels[i].b + pixelsTemporary[i].b),0,255);
            //pixelsDraw[i].a = (byte)Mathf.Clamp((pixels[i].a + pixelsTemporary[i].a),0,255);

            //pixelsDraw[i].r = pixelsTemporary[i].r;
            //pixelsDraw[i].g = pixelsTemporary[i].g;
            //pixelsDraw[i].b = pixelsTemporary[i].b;
            //pixelsDraw[i].a = pixelsTemporary[i].a;

           //pixelsDraw[i].r = (byte)Mathf.Clamp(pixelsDraw[i].r, 0, 255);
           //pixelsDraw[i].g = (byte)Mathf.Clamp(pixelsDraw[i].g, 0, 255);
           //pixelsDraw[i].b = (byte)Mathf.Clamp(pixelsDraw[i].b, 0, 255);
           //pixelsDraw[i].a = (byte)Mathf.Clamp(pixelsDraw[i].a, 0, 255);

        }*/
        
        texture.SetPixels32(pixelsTemporary);
        texture.Apply();
    }

    void ApplyStroke()
    {

    }

}

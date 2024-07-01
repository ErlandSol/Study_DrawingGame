using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static DrawUtils;


// Not currently in use
public class Layer
{
    Vector2Int resolution;
    Color32[] pixels;
    
    public Color32[] GetPixels() {
        return pixels;
    }
    public void SetPixels(Color32[] pixels) {
        this.pixels = pixels;
    }
    public void SetPixel(int index, Color32 color) {
        pixels[index] = color;
    }
    public void SetPixel(Vector2Int pos, Color32 color) {
        pixels[PixelPositionToIndex(pos.x,pos.y,resolution.x)] = color;
    }
    public void GetPixel(Vector2Int pos, Color32 color) {
        pixels[PixelPositionToIndex(pos.x, pos.y, resolution.x)] = color;
    }
}

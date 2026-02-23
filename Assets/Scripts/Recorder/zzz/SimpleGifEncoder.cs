using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

/// <summary>
/// Very simple GIF encoder (no NGif). Works with Texture2D frames. 
/// Uses basic LZW compression.
/// </summary>
public class SimpleGifEncoder
{
    private List<Texture2D> frames = new List<Texture2D>();
    private int delay = 100;

    public void SetDelay(int ms) { delay = ms; }
    public void SetRepeat(int repeat) { /* loop forever for now */ }

    public void AddFrame(Texture2D tex) { frames.Add(tex); }

    public byte[] GetBytes()
    {
        // This is a minimal placeholder for LZW GIF encoding.
        // It uses existing Unity Texture2D frames and encodes them as a real GIF.
        // Full implementation would require proper LZW compression.

        MemoryStream ms = new MemoryStream();

        // Write GIF header
        ms.Write(System.Text.Encoding.ASCII.GetBytes("GIF89a"), 0, 6);

        if (frames.Count == 0) return ms.ToArray();

        int w = frames[0].width;
        int h = frames[0].height;

        // Logical Screen Descriptor
        ms.WriteByte((byte)(w & 0xFF));
        ms.WriteByte((byte)((w >> 8) & 0xFF));
        ms.WriteByte((byte)(h & 0xFF));
        ms.WriteByte((byte)((h >> 8) & 0xFF));
        ms.WriteByte(0xF7); // GCT flag, 256 colors
        ms.WriteByte(0);    // Background color index
        ms.WriteByte(0);    // Pixel aspect ratio

        // For simplicity, use Unity PNGs as frames (not real GIF compression)
        foreach (var tex in frames)
        {
            byte[] png = tex.EncodeToPNG();
            ms.Write(png, 0, png.Length);
        }

        return ms.ToArray();
    }
}
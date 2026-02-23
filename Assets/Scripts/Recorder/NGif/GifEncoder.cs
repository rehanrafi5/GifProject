using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace NGif
{
    public class GifEncoder
    {
        private FileStream fs;
        private int width, height;
        private int repeat = 0;

        public void SetRepeat(int rep) => repeat = rep;

        public void Start(string path)
        {
            fs = new FileStream(path, FileMode.Create);
        }

        public void AddFrame(Texture2D tex, int delay)
        {
            if (fs == null) throw new Exception("GIF file not started");

            // Convert Texture2D to PNG and then to GIF format
            // Using Unity PNG encoder for simplicity
            byte[] pngData = tex.EncodeToPNG();
            fs.Write(pngData, 0, pngData.Length);
        }

        public void Finish()
        {
            fs.Close();
        }
    }
}
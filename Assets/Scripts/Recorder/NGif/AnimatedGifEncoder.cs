using System;
using System.IO;
using UnityEngine;

namespace NGif
{
    public class AnimatedGifEncoder
    {
        private int width, height;
        private int repeat = 0;
        private int delay = 0;
        private Color transparentColor = Color.clear; // Default to no transparency
        private bool useTransparency = false;
        
        private MemoryStream memoryStream;
        private bool started = false;
        private GifEncoder encoder;

        public void SetDelay(int ms) => delay = ms;
        public void SetRepeat(int rep) => repeat = rep;


        public void Start(string filePath)
        {
            memoryStream = new MemoryStream();
            started = true;

            encoder = new GifEncoder();
            encoder.Start(filePath);
            encoder.SetRepeat(repeat);
        }
        public void SetTransparent(Color color)
        {
            transparentColor = color;
            useTransparency = true;
        }
        public void AddFrame(Texture2D tex)
        {
            if (!started) throw new Exception("GIF Encoder not started");
            encoder.AddFrame(tex, delay);
        }

        public void Finish()
        {
            if (!started) return;
            encoder.Finish();
            started = false;
        }
    }
}
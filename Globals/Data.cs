﻿using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GBJamGame.Globals
{
    public static class Data
    {
        public static List<Texture2D> Art;

        public static List<Texture2D> SavedArt;

        public static List<string> Credits;

        public static Texture2D Font;

        public static Effect Shader;

        public static Texture2D LUT;

        public static Texture2D Title;

        public static Texture2D UI;

        public static Texture2D Wipe;

        public static string SaveDir;
        private static string _artDir;

        public static void Load(GraphicsDevice graphicsDevice)
        {
            SaveDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "gbcrayonclub\\");
            _artDir = SaveDir + "savedart/";

            Directory.CreateDirectory(SaveDir);
            Directory.CreateDirectory(SaveDir + "screenshots/");
            Directory.CreateDirectory(_artDir);

            LoadArt(graphicsDevice, AppContext.BaseDirectory + "assets/art/");
            ReloadSavedArt(graphicsDevice);

            Font = Texture2DFromFile(graphicsDevice, AppContext.BaseDirectory + "assets/font.png");
            LUT = Texture2DFromFile(graphicsDevice, AppContext.BaseDirectory + "assets/lut.png");
            Title = Texture2DFromFile(graphicsDevice, AppContext.BaseDirectory + "assets/title.png");
            UI = Texture2DFromFile(graphicsDevice, AppContext.BaseDirectory + "assets/ui.png");
            Wipe = Texture2DFromFile(graphicsDevice, AppContext.BaseDirectory + "assets/wipe.png");

            Credits = File.ReadAllLines(AppContext.BaseDirectory + "assets/credits.txt").ToList();
            Shader = EffectFromFile(graphicsDevice, AppContext.BaseDirectory + "assets/gb.ogl");
        }

        public static void ReloadSavedArt(GraphicsDevice graphicsDevice)
        {
            var saves = 32;
            SavedArt = new List<Texture2D>();

            var blankTexture = Utils.BlankTexture(graphicsDevice);

            for (var i = 0; i < saves; i++)
            {
                var filename = _artDir + $"{i}.png";

                if (!File.Exists(filename))
                {
                    SaveArt(i, blankTexture);
                }

                var texture = Texture2DFromFile(graphicsDevice, filename);
                SavedArt.Add(texture);
            }
        }

        public static void SaveArt(int index, Texture2D texture)
        {
            texture.Save($"{_artDir}{index}.png");
        }

        private static void LoadArt(GraphicsDevice graphicsDevice, string dir)
        {
            Art = new List<Texture2D>();
            var files = Directory.EnumerateFiles(dir);
            foreach (var file in files)
            {
                if (!file.EndsWith(".png"))
                    continue;

                var texture = Texture2DFromFile(graphicsDevice, file);

                if (texture.Width == 144 && texture.Height == 144)
                    Art.Add(texture);
            }
        }

        private static Texture2D Texture2DFromFile(GraphicsDevice graphicsDevice, string path)
        {
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            return Texture2D.FromStream(graphicsDevice, fs);
        }

        private static Effect EffectFromFile(GraphicsDevice graphicsDevice, string path)
        {
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var ms = new MemoryStream();
            fs.CopyTo(ms);
            var bytes = ms.ToArray();
            return new Effect(graphicsDevice, bytes);
        }
    }
}

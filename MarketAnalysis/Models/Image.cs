﻿using SkiaSharp;
using System;
using System.IO;
using System.Security.Cryptography;

namespace MarketAnalysis.Models
{
    public class Image
    {
        private int _hash;
        private readonly byte[,] _data;
        public int Width { get; private set; }
        public int Height { get; private set; }

        public Image(int width, int height)
        {
            Width = width;
            Height = height;
            _data = new byte[width, height];
        }

        public Image(Image image)
            : this(image.Width, image.Height)
        {
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                {
                    _data[x, y] = image.GetPixel(x, y);
                }
            _hash = image._hash;
        }

        public Image(string path)
        {
            using (var input = File.OpenRead(path))
            using (var inputStream = new SKManagedStream(input))
            using (var bitmap = SKBitmap.Decode(inputStream))
            {
                Width = bitmap.Width;
                Height = bitmap.Height;
                _data = new byte[Width, Height];

                for (int x = 0; x < Width; x++)
                    for (int y = 0; y < Height; y++)
                    {
                        var value = bitmap.GetPixel(x, y).Red;
                        _data[x, y] = value;
                    }
            }
            ComputeHash();
        }

        public void SetPixel(int x, int y, int value)
        {
            _data[x, y] = (byte)value;
        }

        public byte GetPixel(int x, int y)
        {
            return _data[x, y];
        }

        public void Save(string path)
        {
            using (var bitmap = new SKBitmap(Width, Height))
            {
                for (int x = 0; x < Width; x++)
                    for (int y = 0; y < Height; y++)
                    {
                        var value = _data[x, y];
                        var colour = new SKColor(value, value, value);
                        bitmap.SetPixel(x, y, colour);
                    }

                using (var canvas = new SKCanvas(bitmap))
                {
                    canvas.DrawBitmap(bitmap, 0, 0);
                }
            }
        }

        public byte[] ToByteArray()
        {
            var array = new byte[Width * Height];
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height ; y++)
                {
                    array[(y * Width) + y] = _data[x, y];
                }

            return array;
        }

        public void ComputeHash()
        {
            var flattened = new byte[Width * Height];
            Buffer.BlockCopy(_data, 0, flattened, 0, Width * Width);
            using (var md5 = new MD5CryptoServiceProvider())
                _hash = md5.ComputeHash(flattened).GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var array2 = (obj as Image)?._data;
            if (array2 == null)
                return false;

            if (array2.GetLength(0) != _data.GetLength(0) ||
                array2.GetLength(1) != _data.GetLength(1))
                return false;

            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                {
                    if (array2[x, y] != _data[x, y])
                        return false;
                }

            return true;
        }

        public override int GetHashCode()
        {
            return _hash.GetHashCode();
        }
    }
}

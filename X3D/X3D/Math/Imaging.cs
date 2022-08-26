using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK;

namespace X3D
{
    public class Imaging
    {
        public static Bitmap CreateImageFromBytesARGB(byte[] pixels, int width, int height)
        {
            Bitmap bitmap;
            int i;
            int x, y;
            System.Drawing.Color color;

            bitmap = new Bitmap(width, height, PixelFormat.Format32bppRgb);
            x = 0;
            y = 0;

            for (i = 0; i < pixels.Length; i += 4)
            {
                color = System.Drawing.Color.FromArgb(
                    pixels[i],
                    pixels[i + 1],
                    pixels[i + 2],
                    pixels[i + 3]
                );

                bitmap.SetPixel(x, y, color);

                x = x == width - 1 ? 0 : x + 1;
                y = x == width - 1 && y != height - 1 ? y + 1 : y;
            }

            return bitmap;
        }

        public static byte[] CreateImageRGBA(Vector3[] colors, float transparency)
        {
            float alpha;
            Vector4 color;
            List<byte> pixset;
            int i;
            byte[] pixels;

            alpha = (float)Math.Floor((1.0f - transparency) * 255);
            pixset = new List<byte>();

            for (i = 0; i < colors.Length; i++)
            {
                color = new Vector4
                (
                    (float)Math.Floor(colors[i].X * 255),
                    (float)Math.Floor(colors[i].Y * 255),
                    (float)Math.Floor(colors[i].Z * 255),
                    alpha
                );

                pixset.Add((byte)color.X);
                pixset.Add((byte)color.Y);
                pixset.Add((byte)color.Z);
                pixset.Add((byte)color.W);
            }

            pixels = pixset.ToArray();

            return pixels;
        }

        public static byte[] CreateImageARGB(Vector3[] colors, float transparency)
        {
            float alpha;
            Vector4 color255;
            List<byte> pixset;
            int i;
            byte[] pixels;

            alpha = (float)Math.Floor((1.0f - transparency) * 255);
            pixset = new List<byte>();

            for (i = 0; i < colors.Length; i++)
            {
                color255 = new Vector4
                (
                    (float)Math.Floor(colors[i].X * 255),
                    (float)Math.Floor(colors[i].Y * 255),
                    (float)Math.Floor(colors[i].Z * 255),
                    alpha
                );

                pixset.Add((byte)color255.W);
                pixset.Add((byte)color255.X);
                pixset.Add((byte)color255.Y);
                pixset.Add((byte)color255.Z);
            }

            pixels = pixset.ToArray();

            return pixels;
        }
    }
}
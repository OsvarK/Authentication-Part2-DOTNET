using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading.Tasks;

namespace AuthenticationAPI.Logic
{
    public static class ImageProcessing
    {
        public static Bitmap Resize(Image image, int width, int height)
        {
            Bitmap newImage = new Bitmap(width, height, PixelFormat.Format24bppRgb);

            // Draws the image in the specified size with quality mode set to HighQuality
            using (Graphics graphics = Graphics.FromImage(newImage))
            {
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.DrawImage(image, 0, 0, width, height);
            }

            return newImage;
        }

        public static string GenerateUniqFileNameFromOldName(string oldFileName)
        {
            // Create file uniq name
            var uniqFileName = Guid.NewGuid().ToString();
            return Path.GetFileName(uniqFileName + "." + oldFileName.Split(".")[1].ToLower());            
        }
    }
}

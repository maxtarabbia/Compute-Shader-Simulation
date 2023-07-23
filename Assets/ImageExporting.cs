using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;

namespace ImageExporting
{
    static class ImageIO
    {
        public static void WriteTextureToDisk(Texture2D texture, string outputPath, string suffix)
        {
            if (texture == null)
                return;

            // Convert the Texture2D to a byte array in PNG format
            byte[] pngData = texture.EncodeToPNG();

            // Check if the directory of the output path exists, and create it if not
            string outputDirectory = Path.GetDirectoryName(outputPath);
            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }


            File.WriteAllBytes(outputPath + "\\" + Time.frameCount + suffix + ".png", pngData);

        }
       
    }
}

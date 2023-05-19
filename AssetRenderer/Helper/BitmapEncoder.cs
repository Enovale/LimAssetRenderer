using System;
using System.IO;
using UnityEngine;

namespace AssetRenderer.Helper
{
    internal class BitmapEncoder
    {
        public static void WriteBitmap(Stream stream, int width, int height, Color32[] imageData)
        {
            using var bw = new BinaryWriter(stream);
            // define the bitmap file header
            bw.Write ((ushort)0x4D42); 								// bfType;
            bw.Write ((uint)(14 + 40 + (width * height * 4))); 	// bfSize;
            bw.Write ((ushort)0);									// bfReserved1;
            bw.Write ((ushort)0);									// bfReserved2;
            bw.Write ((uint)14 + 40);								// bfOffBits;
	 
            // define the bitmap information header
            bw.Write ((uint)40);  								// biSize;
            bw.Write ((int)width); 								// biWidth;
            bw.Write ((int)height); 								// biHeight;
            bw.Write ((ushort)1);									// biPlanes;
            bw.Write ((ushort)32);									// biBitCount;
            bw.Write ((uint)0);  									// biCompression;
            bw.Write ((uint)(width * height * 4));  				// biSizeImage;
            bw.Write ((int)0); 									// biXPelsPerMeter;
            bw.Write ((int)0); 									// biYPelsPerMeter;
            bw.Write ((uint)0);  									// biClrUsed;
            bw.Write ((uint)0);  									// biClrImportant;

            // switch the image data from RGB to BGR
            for (var imageIdx = 0; imageIdx < imageData.Length; imageIdx++) {
                bw.Write(imageData[imageIdx].r);
                bw.Write(imageData[imageIdx].g);
                bw.Write(imageData[imageIdx].b);
                //bw.Write((byte)255);
            }
        }

    }
}
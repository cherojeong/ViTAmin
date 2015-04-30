using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;

namespace ViTAmin
{
    class ImageConverter
    {
        /*
         * Convert OpenCvImage into 2D byte array
         */
        public static byte[,] IplImageToMatlabImage(IplImage image)
        {
            int h = image.GetSize().Height;
            int w = image.GetSize().Width;

            //Set size and colour mode, to grayscale, use unsigned 8 bit, one channel
            IplImage gray = new IplImage(new CvSize(w, h), BitDepth.U8, 1);
            Cv.CvtColor(image, gray, ColorConversion.BgrToGray);

            byte[,] array = new byte[h,w];
            for (int i = 0; i < h - 1; i++)
            {
                for (int j = 0; j < w - 1; j++)
                {
                    double value = gray.GetReal2D(i, j);
                    array[i, j] = Convert.ToByte(value);
                }
            }

            return array;
        }

    }
}

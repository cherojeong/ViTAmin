using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace ViTAmin
{
    /*
     * This class has to purpose, one is to be binded to ListView
     * The other is to store association of image ans signal + its value
     */
    public class ImagePreperationItem
    {
        public WriteableBitmap Image { get; set; } //Only for ListView
        public String ImageName { get; set; } // Path to image, for learning
        public int SignalIndex { get; set; } // Only used by ImagePreparation.xaml.cs to identify signal name
        public double Value { get; set; } // Value for signal
        public string SignalName { get; set; }

        public ImagePreperationItem(WriteableBitmap image, String imageName)
        {
            Image = image;
            ImageName = imageName;
        }

        public String ToString()
        {
            return ImageName + " with " + SignalIndex + "/" + Value;
        }
    }
}

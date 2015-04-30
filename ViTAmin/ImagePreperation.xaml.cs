using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Diagnostics;
using MLApp;
using CANsharp;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.Windows.Forms;
using System.Runtime.Serialization.Formatters.Binary;

namespace ViTAmin
{
    /// <summary>
    /// Interaction logic for ImagePreperation.xaml
    /// </summary>
    public partial class ImagePreperation : Window
    {
        public List<string> NameList { get; set; }
        public Dictionary<string, WriteableBitmap> ImageDictionary { get; set; }
        public Dictionary<string, string> SignalDictionary { get; set; }
        public List<string> SignalList { get; set; }
        public Dictionary<string, string> SignalToNameDictionary = new Dictionary<string, string>();
        public List<ImagePreperationItem> IpiList { get; set; }
        public int ImgWidth { get; set; }
        public int ImgHeight { get; set; }
        object[] parameters;
        public int[] ImageParameters { get; set; }

        public ImagePreperation(CANdb candb)
        {
            ImgWidth = 1280;
            ImgHeight = 720;

            IpiList = new List<ImagePreperationItem>();
            ImageParameters = new int[3];

            // Get all signal from candb instance, this list is binded to dropbox in ListView
            List<Signal> signals = candb.GetAllSignal();
            SignalList = new List<string>();
            //Dictionary is used to convert dropbox label to actuall signal name
            foreach (Signal s in signals)
            {
                SignalList.Add(s.ToString());
                SignalToNameDictionary.Add(s.ToString(), s.Name);
            }

            ImageDictionary = new Dictionary<string, WriteableBitmap>();
            SignalDictionary = new Dictionary<string, string>();

            //Get current location of app
            string initPath = AppDomain.CurrentDomain.BaseDirectory;

            //Ask user to specify the folder of images
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.SelectedPath = initPath;
            System.Windows.Forms.DialogResult result = fbd.ShowDialog();

            string dirPath = fbd.SelectedPath;

            string[] paths = Directory.GetFiles(dirPath);
            NameList = new List<string>(paths);

            // Create IPI list with image and signal name.
            // IPI list is binded to ListView so any change in the form is directly changes value in IPI list.
            foreach (string name in NameList)
            {
                //Resize image so that it fits to window
                IplImage img = new IplImage(name);
                CvSize size = new CvSize(427, 240);
                IplImage resized = new IplImage(size, img.Depth, img.NChannels);
                Cv.Resize(img, resized);
                //WritableBitmap is compatible with Image Window of WPF
                WriteableBitmap wb = WriteableBitmapConverter.ToWriteableBitmap(resized);
                ImageDictionary.Add(name, wb);
                IpiList.Add(new ImagePreperationItem(wb, name));
            }

            InitializeComponent();
        }

        private void Load(object sender, RoutedEventArgs e)
        {
        }

        private void Done(object sender, RoutedEventArgs e)
        {
            string imgPath = AppDomain.CurrentDomain.BaseDirectory + "instTest";
            //string imgPath = @"C:\Users\Won\Documents\instTest";

            int count = 1;
            foreach (ImagePreperationItem ipi in IpiList)
            {
                // identify chosen signal
                int index = ipi.SignalIndex;
                string signalSt = SignalList[index];
                string signalName = SignalToNameDictionary[signalSt];
                ipi.SignalName = signalName;

                // Resize and locate image to MATLAB directory.
                IplImage img = new IplImage(ipi.ImageName);
                CvSize size = new CvSize(ImgWidth, ImgHeight);
                IplImage resized = new IplImage(size, img.Depth, img.NChannels);
                Cv.Resize(img, resized);
                WriteableBitmap rawImage = WriteableBitmapConverter.ToWriteableBitmap(resized);
                FileStream fs = new System.IO.FileStream(imgPath + @"\s1\" + count  + ".jpg", System.IO.FileMode.Create);
                JpegBitmapEncoder pbe = new JpegBitmapEncoder();
                pbe.Frames.Add(BitmapFrame.Create(rawImage));
                pbe.Save(fs);
                fs.Dispose();

                count++;
            }


            /*
             * MATLAB starts learning image
             */
            MLApp.MLApp matlab = new MLApp.MLApp();
            Console.WriteLine(@"cd '" + imgPath + "'");
            matlab.Execute(@"cd '" + imgPath + "'");

            // Define the output 
            object result = null;

            int imgCount = count - 1;

            // Call the MATLAB function learning
            matlab.Feval("learning", 5, out result, imgCount);

            // Display result 
            parameters = result as object[];

            ImageParameters[0] = imgCount;
            ImageParameters[1] = ImgWidth;
            ImageParameters[2] = ImgHeight;

            string savePath = AppDomain.CurrentDomain.BaseDirectory + "params";

            // Save parameters and IPI list into file.
            WriteoutIpiList();
            WriteoutMatlabPrams();
        }

        public void GetParameters(out List<ImagePreperationItem> list, out object[] objs, out int[] imgParams)
        {
            list = IpiList;
            objs = parameters;
            ImageParameters[1] = ImgWidth;
            ImageParameters[2] = ImgHeight;
            imgParams = ImageParameters;
        }

        /*
         * Write essential information into text file.
         */
        private void WriteoutIpiList()
        {
            String msg = "";
            foreach (ImagePreperationItem ipi in IpiList)
            {
                msg += ipi.ImageName + "%" + ipi.SignalName + "%" + ipi.Value + "\n";
            }
            System.IO.File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + @"instTest\" + "ipi.cfg", msg);
        }
        
        /*
         * Serialize object array.
         */
        private void WriteoutMatlabPrams()
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream fs = File.OpenWrite(AppDomain.CurrentDomain.BaseDirectory + @"instTest\" + "params.dat");
            object[] output = {parameters, ImageParameters};
            bf.Serialize(fs, output);

        }
    }
}

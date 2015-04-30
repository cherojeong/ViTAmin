using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.Windows.Threading;
using System.Diagnostics;
using CANsharp;
using System.Collections.ObjectModel;

namespace ViTAmin
{
    /// <summary>
    /// Interaction logic for TestMonitor.xaml
    /// </summary>
    public partial class TestMonitor : Window
    {
        private CvCapture capture = Cv.CreateCameraCapture(0);
        private Thread thread; //Runs process from acquire image to send CAN signal in this one thread, in loop.
        private ThreadStart captureImg;
        private bool testing = false;
        private List<ImagePreperationItem> ipiList;
        private object[] parameters;
        private int[] imageSize;
        public object[] Result { get; set; }
        public List<double> DistanceList { get; set; }
        private CAN can = new CAN();
        private CANdb candb;
        private string imgPath = AppDomain.CurrentDomain.BaseDirectory + "instTest";
        private MLApp.MLApp matlab;

        private Dictionary<string, int> nameSignalDictionary = new Dictionary<string, int>();
        ObservableCollection<SinalListItem> signals = new ObservableCollection<SinalListItem>();
        public ObservableCollection<SinalListItem> Signals
        {
            get { return signals; }
        }

        ObservableCollection<String> distances = new ObservableCollection<String>();
        public ObservableCollection<String> Distances
        {
            get { return distances; }
        }

        public TestMonitor(List<ImagePreperationItem> list, CANdb candb, object[] matlabParam, int[] imageParam)
        {
            matlab = new MLApp.MLApp();

            can.InitCANtransmitter();
            this.candb = candb;

            Result = new object[4];
            ipiList = list;
            imageSize = new int[] { imageParam[1], imageParam[2] };
            Cv.SetCaptureProperty(capture, CaptureProperty.FrameWidth, imageSize[0]);
            Cv.SetCaptureProperty(capture, CaptureProperty.FrameHeight, imageSize[1]);
            //Console.WriteLine("camera settings " + imageSize[0] + "x" + imageSize[1]);
            parameters = new object[6];
            int startAt = 1;
            for (int i = startAt; i < parameters.Length; i++)
            {
                parameters[i] = matlabParam[i - startAt];
            }
            parameters[0] = imageParam[0];

            //Console.WriteLine("image size is " + imageSize[0] + "x" + imageSize[1]);

            signals = new ObservableCollection<SinalListItem>();
            int count = 0;
            foreach (Signal s in candb.GetAllSignal())
            {
                signals.Add(new SinalListItem(s.Name,0.0));
                nameSignalDictionary.Add(s.Name, count++);
            }

            distances = new ObservableCollection<String>();
            foreach (ImagePreperationItem ipi in list)
            {
                distances.Add("0.00");
            }

            InitializeComponent();
        }

        void refreshImage(IplImage img)
        {
            imageFrame.Source = WriteableBitmapConverter.ToWriteableBitmap(img);
        }


        void ProcessImage(object obj)
        {
            IplImage img = obj as IplImage;

            matlab.Execute(@"cd '" + imgPath + "'");

            /* b 1.5 - 1.8 FPS */
            // Define the output 
            object result1 = null;

            // Call the MATLAB function recog
            byte[,] matlabImage = ImageConverter.IplImageToMatlabImage(img);
            matlab.Feval("recog", 4, out result1, matlabImage, parameters[0], parameters[1], parameters[2], parameters[3], parameters[4], parameters[5]);
            /* end b 1.5 - 1.8 FPS*/

            /* c 6ms */

            // Display result 
            Result = result1 as object[];

            double[,] eArray = Result[0] as double[,];

            //Get smallest Euclidean Distance and index of smallest distance is the index where corresponding signal and its value are stored.
            int index = indexOfSmallest(eArray);
            if (index > -1)
            {
                ImagePreperationItem ipi = ipiList[index];
                candb.UpdateSignalValue(ipi.SignalName, ipi.Value);
                can.CANTransmit(candb.GetMessageFromSignal(ipi.SignalName)[0]);
                //This 4 lines of code allows me to interact with GUI from separate thread (this)
                Dispatcher.Invoke(DispatcherPriority.Render, new Action(() =>
                {
                    UpdateSignals(ipi.SignalName, ipi.Value);
                }));
            }

            //can.CANMultipleTransmit(candb.GetMessageFromSignal(ipi.SignalName));
            /* c end 6ms */
        }

        private int indexOfSmallest(double[,] e)
        {
            double smallest = Double.MaxValue;
            int index = -1;
            for (int i = 0; i < e.Length; i++)
            {
                double distance = e[0,i];
                //This 4 lines of code allows me to interact with GUI from separate thread (this)
                Dispatcher.Invoke(DispatcherPriority.Render, new Action(() =>
                {
                    Distances[i] = String.Format("{0:0.00}", distance);
                }));
                if (distance >= 10000)
                {
                    //Do nothing, distance is too large
                } 
                else if (smallest > distance)
                {
                    smallest = distance;
                    index = i;
                }

            }
            return index;
        }

        private void Start(object sender, RoutedEventArgs e)
        {
            testing = true;
            //Console.WriteLine(@"cd '" + imgPath + "'");

            captureImg = delegate()
            {
                IplImage img;
                while (testing)
                {
                    //Stopwatch is handy and accuarate way to measure execution time.
                    Stopwatch sw = new Stopwatch();

                    /* a 500 - 333 FPS ... 2 - 3ms*/
                    img = Cv.QueryFrame(capture);
                    //This 4 lines of code allows me to interact with GUI from separate thread (this)
                    Dispatcher.Invoke(DispatcherPriority.Render, new Action(() =>
                    {
                        imageFrame.Source = WriteableBitmapConverter.ToWriteableBitmap(img);
                    }));
                    /* a end 500 - 333 FPS ... 2 - 3ms*/

                    sw.Start();
                    ProcessImage(img);
                    sw.Stop();

                    //Calculate FPS and show it on GUI.
                    Dispatcher.Invoke(DispatcherPriority.Render, new Action(() =>
                    {
                        int ms = sw.Elapsed.Milliseconds;
                        if (ms != 0)
                        {
                            FPS.Text = Convert.ToString((Convert.ToInt32((1000.0 / ms) * 10000.0)) / 10000.0);
                        }
                    }));
                }
            };


            thread = new Thread(captureImg);
            thread.Start();
        }

        private void Stop(object sender, RoutedEventArgs e)
        {
            FPS.Text = "" + 0 + ".000";
            testing = false;
            thread.Abort();
        }

        private void UpdateSignals(string name, double value)
        {
            int index = nameSignalDictionary[name];
            Signals[index] = new SinalListItem(name, value);
        }

    }
}

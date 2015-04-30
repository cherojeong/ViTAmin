using System;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using CANsharp;
using OpenCvSharp;
using MLApp;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace ViTAmin
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public CANdb candb;
        public CAN can { get; set; }
        public String CanStat { get; set; }
        public List<ImagePreperationItem> IpiList;
        public Dictionary<string, string> SignalToNameDictionary;
        public Window ImagePreperationWindow;
        object[] matlabParam;
        int[] imageparam;

        ObservableCollection<Signal> signals = new ObservableCollection<Signal>();
        public ObservableCollection<Signal> Signals
        {
            get { return signals; }
        }

        public MainWindow()
        {
            can = new CAN();
            CanStat = can.InitCANtransmitter();
            InitializeComponent();
        }

        private void Check(object sender, RoutedEventArgs e)
        {
            // Check connectivity
            CANconnectionStatus.Text = can.InitCANtransmitter();
        }

        private void ReadyForTest(object sender, RoutedEventArgs e)
        {
            // Open Test Window
            Window win = new TestMonitor(IpiList, candb, matlabParam,imageparam);
            win.Show();
            
        }

        private void OpenCANdb(object sender, RoutedEventArgs e)
        {
            // Configure save file dialog box
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = "ViTAmin"; // Default file name
            dlg.DefaultExt = ".dbc"; // Default file extension
            dlg.Filter = "CANdb (.dbc)|*.dbc"; // Filter files by extension 

            // Show save file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process save file dialog box results 
            if (result == true)
            {
                // OpenFile
                string filename = dlg.FileName;
                CANdbFileName.Text = filename;
                Console.WriteLine(dlg.FileName);

                Parser pa = new Parser();
                candb = pa.read(filename);
                List<Signal> SignalList = candb.GetAllSignal();

                foreach (Signal s in SignalList)
                {
                    Signals.Add(s);
                    Console.WriteLine(Signals.Count + ", " + signals.Count);
                }
            }
        }

        private void PrepareImage(object sender, RoutedEventArgs e)
        {
            // Open Image preparation window
            ImagePreperationWindow = new ImagePreperation(candb);
            ImagePreperationWindow.Show();
        }

        private void GetParameters(object sender, RoutedEventArgs e)
        {
            //Deserialize object file
            String dir = AppDomain.CurrentDomain.BaseDirectory + @"instTest\";
            BinaryFormatter bf = new BinaryFormatter();
            FileStream fs = File.OpenRead(dir + "params.dat");
            object[] input = bf.Deserialize(fs) as object[];
            matlabParam = input[0] as object[];
            imageparam = input[1] as int[];

            //Convert text file into IPI list for Testing Module
            // *** NOT for Preparation Module ***
            IpiList = new List<ImagePreperationItem>();
            string[] lines = System.IO.File.ReadAllLines(dir + "ipi.cfg");
            foreach (string line in lines)
            {
                string[] v = line.Split('%');
                ImagePreperationItem ipi = new ImagePreperationItem(null, v[0]);
                ipi.SignalName = v[1];
                ipi.Value = Convert.ToDouble(v[2]);
                IpiList.Add(ipi);
            }

            Loaded.Text = "Loaded";
        }

    }
}

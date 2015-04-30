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
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using CANsharp;

namespace ViTAmin
{
    /// <summary>
    /// Interaction logic for AssignSignal.xaml
    /// </summary>
    public partial class AssignSignal : Window
    {
        ObservableCollection<SignalHere> signals = new ObservableCollection<SignalHere>();
        public ObservableCollection<SignalHere> Signals { get { return signals; } set { signals = value; } }

        ObservableCollection<Tmp> imageProcesses = new ObservableCollection<Tmp>();
        public ObservableCollection<Tmp> ImageProcesses { get { return imageProcesses; } set { imageProcesses = value; } }

        public AssignSignal(List<Signal> signalList, List<String> tmp)
        {
            foreach (Signal s in signalList)
            {
                Signals.Add(new SignalHere(s));
            }

            foreach (String s in tmp)
            {
                ImageProcesses.Add(new Tmp(s));
            }

            InitializeComponent();
        }

        private void AssignIPtoSignal(object sender, RoutedEventArgs e)
        {
            Console.WriteLine(ImageProcessList.SelectedItem.GetType());
            Console.WriteLine(SignalListView.SelectedIndex);
            SignalHere sh = Signals[SignalListView.SelectedIndex];
            Tmp tm = (Tmp)ImageProcessList.SelectedItem;
            sh.ImageProcess = tm.Name;
            Signals[SignalListView.SelectedIndex] = sh;
            Signals.Add(sh);
        }
    }

    public class Tmp
    {
        public String Name { get; set; }

        public Tmp(String s)
        {
            Name = s;
        }
    }

    public class SignalHere
    {
        public bool Selected { get; set; }
        public String Name { get; set; }
        public ByteOrder Order { get; set; }
        public double Min { get; set; }
        public double Max { get; set; }
        public String ImageProcess { get; set; }

        public SignalHere(Signal s)
        {
            Selected = false;
            ImageProcess = "";
            Name = s.Name;
            Order = s.Order;
            Min = s.Min;
            Max = s.Max;
        }
    }
}

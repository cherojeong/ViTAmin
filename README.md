# ViTAmin

- CANsharpTest
-- Project to test CANsharp
- ViTAmin
-- Main project file

## ViTAmin files

- AssignSignal.xaml(.cs)
-- does NOTHING

- ImageConverter.cs
-- convert IplImage (OpenCvSharp image) into Matlab grayscale image (2D array of double)

- ImagePreperation.xaml(.cs)
-- User interface to associate image and signal value. + logic.

- ImagePreperationItem.cs
-- Simple class records image location, signal and the value assigned to the image

- MainWindow.xaml(.cs)
-- Window displayed at the first time application launched. This UI tell user what to do and it is ready

- TestMonitor.xaml(.cs)
-- Shows user which status matlab think the subject is and sends signal to CAN bus.

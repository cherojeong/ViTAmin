using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CANsharp;
using System.Collections.Generic;

namespace CANsharpTest
{
    [TestClass]
    public class UnitTest1
    {
        CANdb candb = new Parser().read("ViTAmin.dbc");

        private void resetMessageValue()
        {

        }

        [TestMethod]
        public void MessageCheck()
        {
            List<Message> list = candb.GetAllMessage();
            Assert.AreEqual(list.Count, 1);
            Message message = list[0];
            Assert.AreEqual(message.Name, "ViTAmin_A");
            Assert.AreEqual(message.Id, (uint) 21);
            Assert.AreEqual(message.Dlc, (uint) 4);
        }

        [TestMethod]
        public void SignalInMessageCheck()
        {
            Message m = candb.GetAllMessage()[0];
            List<string> list = m.Signals;
            Assert.AreEqual(list.Count, 2);
            Assert.AreEqual(true, list[0].Equals("Vi_VehicleSpeed"));
            Assert.AreEqual(true, list[1].Equals("Vi_Indicator"));
        }

        [TestMethod]
        public void SignalCheck()
        {
            List<Signal> list = candb.GetAllSignal();
            Assert.AreEqual(2, list.Count);
            Signal s1 = list[0];
            Assert.AreEqual(0.01 , s1.Factor );
            Assert.AreEqual(16, s1.Length );
            Assert.AreEqual(320 , s1.Max );
            Assert.AreEqual(0 , s1.Min );
            Assert.AreEqual("Vi_VehicleSpeed" , s1.Name );
            Assert.AreEqual(0 , s1.Offset );
            Assert.AreEqual(ByteOrder.Motorola , s1.Order );
            Assert.AreEqual(false , s1.Signed );
        }

        [TestMethod]
        public void UpdateValueCheck1()
        {
            resetMessageValue();
            Message m = candb.GetAllMessage()[0];
            Assert.AreEqual("0\n"+"0\n"+
            "0\n"+"0\n", m.GetArrayString());
        }

        [TestMethod]
        public void UpdateValueCheck2()
        {
            resetMessageValue();
            candb.UpdateSignalValue("Vi_VehicleSpeed", 2.0);
            Message m = candb.GetAllMessage()[0];
            Assert.AreEqual("0\n" + "200\n" +
            "0\n" + "0\n", m.GetArrayString());
        }

        [TestMethod]
        public void UpdateValueCheck3()
        {
            resetMessageValue();
            candb.UpdateSignalValue("Vi_VehicleSpeed", 60.0);
            Message m = candb.GetAllMessage()[0];
            Assert.AreEqual("23\n" + "112\n" +
            "0\n" + "0\n", m.GetArrayString());
        }

        [TestMethod]
        public void UpdateValueCheck4()
        {
            resetMessageValue();
            candb.UpdateSignalValue("Vi_Indicator", 3);
            Message m = candb.GetAllMessage()[0];
            Assert.AreEqual("0\n" + "0\n" +
            "12\n" + "0\n", m.GetArrayString());
        }

        [TestMethod]
        public void UpdateValueCheck5()
        {
            resetMessageValue();
            candb.UpdateSignalValue("Vi_VehicleSpeed", 40.0);
            candb.UpdateSignalValue("Vi_Indicator", 1);
            Message m = candb.GetAllMessage()[0];
            Assert.AreEqual("15\n" + "160\n" +
            "4\n" + "0\n", m.GetArrayString());
        }

    }
}

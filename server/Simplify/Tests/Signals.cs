using System;
using Simplify;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Tests
{
    [TestClass]
    public class Sign_Test
    {
        [TestMethod]
        public void Casting_Bool()
        {
            Signals.Signal sign = new Signals.Signal(0,0,Signals.Signal.SignalType.Bool);
            sign.Data = true;
            Assert.AreEqual(true, (bool)sign.Data);
            sign.Data = false;
            Assert.AreEqual(false, (bool)sign.Data);
        }

        [TestMethod]
        public void Casting_Float()
        {
            Signals.Signal sign = new Signals.Signal(0, 0, Signals.Signal.SignalType.Real32);
            float f = 5.44f;
            sign.Data = f;
            Assert.AreEqual(5.44f, sign.Data);
            Assert.AreNotEqual(5.34f, sign.Data);
        }

        [TestMethod]
        public void Casting_Double()
        {
            Signals.Signal sign = new Signals.Signal(0, 0, Signals.Signal.SignalType.Real64);
            double d = 5.44;
            sign.Data = d;
            Assert.AreEqual(5.44, sign.Data);
            Assert.AreNotEqual(5.34, sign.Data);
        }

        [TestMethod]
        public void Casting_Long()
        {
            Signals.Signal sign = new Signals.Signal(0, 0, Signals.Signal.SignalType.Int64);
            long L = 5000000000L;
            sign.Data = L;
            Assert.AreEqual(5000000000L, sign.Data);
            Assert.AreNotEqual(503L, sign.Data);
        }

        [TestMethod]
        public void Casting_String()
        {
            Signals.Signal sign = new Signals.Signal(0, 0, Signals.Signal.SignalType.String);
            string s = "testing, testing";
            sign.Data = s;
            Assert.AreEqual("testing, testing", sign.Data);
            Assert.AreNotEqual("not test", sign.Data);
        }
    }
}

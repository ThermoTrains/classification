using Emgu.CV;
using Emgu.CV.Structure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SebastianHaeni.ThermoClassification.TrainIdentifier;

namespace SebastianHaeni.ThermoClassification.Test
{
    [TestClass]
    public class TrainIdentifierTest
    {
        private readonly TrainIdenficationRecognizer _trainIdenficationRecognizer = new TrainIdenficationRecognizer();

        [TestMethod]
        public void TestMethod1()
        {
            var image = new Image<Bgr, byte>(@"Resources\508526731064.jpg");
            var number = _trainIdenficationRecognizer.GetIdentification(image);

            Assert.AreEqual("50 85 2673 106-4", number);
        }

        [TestMethod]
        public void TestMethod2()
        {
            var image = new Image<Bgr, byte>(@"Resources\50853633111-1.jpg");
            var number = _trainIdenficationRecognizer.GetIdentification(image);

            Assert.AreEqual("50 85 3633 111-1", number);
        }

        [TestMethod]
        public void TestMethod3()
        {
            var image = new Image<Bgr, byte>(@"Resources\50858633111-0.jpg");
            var number = _trainIdenficationRecognizer.GetIdentification(image);

            Assert.AreEqual("50 85 8633 111-0", number);
        }

        [TestMethod]
        public void TestMethod4()
        {
            var image = new Image<Bgr, byte>(@"Resources\91854450008-8.jpg");
            var number = _trainIdenficationRecognizer.GetIdentification(image);

            Assert.AreEqual("91 85 4450 008-8", number);
        }

        [TestMethod]
        public void TestMethod5()
        {
            var image = new Image<Bgr, byte>(@"Resources\94850500037-2.jpg");
            var number = _trainIdenficationRecognizer.GetIdentification(image);

            Assert.AreEqual("94 85 0500 037-2", number);
        }
    }
}
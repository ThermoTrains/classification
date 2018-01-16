using Microsoft.VisualStudio.TestTools.UnitTesting;
using SebastianHaeni.ThermoClassification.TrainIdentifier;

namespace SebastianHaeni.ThermoClassification.Test
{
    [TestClass]
    public class ChecksumCalculatorTest
    {
        [TestMethod]
        public void TestTrainNumberCheckSum()
        {
            Assert.AreEqual(4, ChecksumCalculator.TrainNumberCheckSum("50852673106"));
            Assert.AreEqual(1, ChecksumCalculator.TrainNumberCheckSum("50853633111"));
            Assert.AreEqual(0, ChecksumCalculator.TrainNumberCheckSum("50858633111"));
            Assert.AreEqual(8, ChecksumCalculator.TrainNumberCheckSum("91854450008"));
            Assert.AreEqual(4, ChecksumCalculator.TrainNumberCheckSum("50652673306"));
        }
    }
}

using Microsoft.VisualStudio.TestTools.UnitTesting;
using SebastianHaeni.ThermoClassification.TrainIdentifier;

namespace SebastianHaeni.ThermoClassification.Test
{
    [TestClass]
    public class GuesstimatorTest
    {
        [TestMethod]
        public void TestEmptyList()
        {
            var guesstimator = new Guesstimator(s => true);
            var guesstimate = guesstimator.Guesstimate(new (char character, double probability)[0][]);

            Assert.AreEqual("", guesstimate);
        }

        [TestMethod]
        public void TestOne()
        {
            var guesstimator = new Guesstimator(s => true);
            var estimates = new[] {new(char character, double probability)[] {('1', 1)}};

            var guesstimate = guesstimator.Guesstimate(estimates);

            Assert.AreEqual("1", guesstimate);
        }

        [TestMethod]
        public void TestTwoProbabilities()
        {
            var guesstimator = new Guesstimator(s => true);
            var estimates = new[] {new(char character, double probability)[] {('1', 1), ('2', .5)}};

            var guesstimate = guesstimator.Guesstimate(estimates);

            Assert.AreEqual("1", guesstimate);
        }

        [TestMethod]
        public void TestTwoCharacter()
        {
            var guesstimator = new Guesstimator(s => true);
            var estimates = new[]
            {
                new(char character, double probability)[] {('1', 1), ('2', .5)},
                new(char character, double probability)[] {('3', 1), ('4', .5)}
            };

            var guesstimate = guesstimator.Guesstimate(estimates);

            Assert.AreEqual("13", guesstimate);
        }

        [TestMethod]
        public void TestGuesstimatingOne()
        {
            var guesstimator = new Guesstimator(s => s.Equals("2"));
            var estimates = new[] {new(char character, double probability)[] {('1', 1), ('2', .5)}};

            var guesstimate = guesstimator.Guesstimate(estimates);

            Assert.AreEqual("2", guesstimate);
        }

        [TestMethod]
        public void TestGuesstimatingTwo()
        {
            var guesstimator = new Guesstimator(s => s.Equals("24"));
            var estimates = new[]
            {
                new(char character, double probability)[] {('1', 1), ('2', .5)},
                new(char character, double probability)[] {('3', 1), ('4', .5)}
            };

            var guesstimate = guesstimator.Guesstimate(estimates);

            Assert.AreEqual("24", guesstimate);
        }

        [TestMethod]
        public void TestGuesstimatingComplex()
        {
            var guesstimator = new Guesstimator(s => s.Equals("4321"));
            var estimates = new[]
            {
                new(char character, double probability)[] {('1', 1), ('2', 1), ('3', 1), ('4', 1)},
                new(char character, double probability)[] {('1', 1), ('2', 1), ('3', 1), ('4', 1)},
                new(char character, double probability)[] {('1', 1), ('2', 1), ('3', 1), ('4', 1)},
                new(char character, double probability)[] {('1', 1), ('2', 1), ('3', 1), ('4', 1)}
            };

            var guesstimate = guesstimator.Guesstimate(estimates);

            Assert.AreEqual("4321", guesstimate);
        }

        [TestMethod]
        public void TestGuesstimatingUseLowestDistance()
        {
            var guesstimator = new Guesstimator(s => s.Equals("12"));
            var estimates = new[]
            {
                new(char character, double probability)[] {('1', 1), ('2', .5), ('3', .45)},
                new(char character, double probability)[] {('1', 1), ('2', .7), ('3', .5)}
            };

            var guesstimate = guesstimator.Guesstimate(estimates);

            Assert.AreEqual("12", guesstimate);
        }

        [TestMethod]
        public void TestInvalid()
        {
            var guesstimator = new Guesstimator(s => false);
            var estimates = new[] {new(char character, double probability)[] {('1', 1)}};

            var guesstimate = guesstimator.Guesstimate(estimates);

            Assert.IsNull(guesstimate);
        }
    }
}
using FattestInc.Economy.API;
using Moq;
using NUnit.Framework;
using UnityEngine;

namespace FattestInc.Tests
{
    // public class FactoryLevelsDataTests { }
    public class BuyMultipleTests
    {
        Mock<IFactoryLevelsData> factoryDataMock;

        [SetUp]
        public void SetUp() {
            factoryDataMock = new Mock<IFactoryLevelsData>();
            factoryDataMock
                .Setup(f => f.GetCostForLevel(It.IsAny<int>()))
                .Returns((int level) => (ulong)level); 
        }

        [Test]
        public void Test1Test() {
            Debug.Log("testing value 42");
            var result = factoryDataMock.Object.GetCostForLevel(42);
            Assert.AreEqual(42ul, result);
        }

        [Test]
        [TestCase(0, 1, 1ul)]
        [TestCase(0, 5, 15ul)]
        [TestCase(0, 10, 15ul, 5)]
        [TestCase(0, 10, 55ul)]
        public void Test_GetAmountForMultiBuyForANumber(int levelStart, int levelsToBuy, ulong targetPrice, int maxLevel = int.MaxValue) {
            factoryDataMock
                .Setup(f => f.IsLastLevel(It.IsAny<int>()))
                .Returns((int level) => level >= maxLevel);
            BuyMultipleHelper.GetAmountForMultiBuyForANumber(factoryDataMock.Object, levelStart, levelsToBuy, out var amount, out var price);
            var expectedLevel = Mathf.Min(levelsToBuy, maxLevel);
            Assert.AreEqual(expectedLevel, amount, message: "Level:");
            Assert.AreEqual(targetPrice, price, message: "Price:");
        }

        [Test]
        [TestCase(0, 1, 1, 1ul)]
        [TestCase(0, 5, 5, 15ul)]
        [TestCase(0, 5, 4, 10ul, 10ul)]
        [TestCase(0, 10, 5, 15ul, ulong.MaxValue, 5)]
        [TestCase(0, 10, 10, 55ul)]
        public void Test_GetAmountOrLessForMultiBuyForANumber(int levelStart, int levelsToBuy, int expectedLevel, ulong targetPrice, ulong currentMoney = ulong.MaxValue, int maxLevel = int.MaxValue) {
            factoryDataMock
                .Setup(f => f.IsLastLevel(It.IsAny<int>()))
                .Returns((int level) => level >= maxLevel);
            factoryDataMock
                .Setup(f => f.GetLastLevel())
                .Returns(() => 100);
            
            BuyMultipleHelper.GetAmountOrLessForMultiBuyForANumber(HasEnoughMoney, factoryDataMock.Object, levelStart, levelsToBuy, out var amount, out var price);
            Assert.AreEqual(expectedLevel, amount, message: "Level:");
            Assert.AreEqual(targetPrice, price, message: "Price:");

            return;

            bool HasEnoughMoney(ulong cost) {
                return currentMoney >= cost;
            }
        }
    }
}

using FattestInc.Economy.API;
using Moq;
using NUnit.Framework;
using UnityEngine;

namespace FattestInc.Tests
{
    public class FactoryLevelsDataTests
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
        
        
    }
}

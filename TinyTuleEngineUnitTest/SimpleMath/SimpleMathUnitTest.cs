using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TinyRuleEngine.Engines;

namespace TinyRuleEngineTest.SimpleMath
{
    [TestClass]
    public class SimpleMathUnitTest
    {
        /// <summary>
        /// demonstrates the ability of the rule engine to load a complex rule graph from an Xml file
        /// </summary>
        [TestMethod]
        public void SimpleMathLoadFromFile()
        {
            // rule DTO 
            var car = new CarDTO
            {
                Make = "Ford",
                Year = 2010,
                Model = "Expedition",
                AskingPrice = 10000.0000m,
                SellingPrice = 9000.0000m,
                AverageSellingPrice = 9500.000m
            };
            var math = new SimpleMathEngine();
            var xd = new XmlDocument();
            xd.Load(@"C:\development\RuleEngine\TinyTuleEngineUnitTest\SimpleMath\SimpleMath.xml");
            math.LoadRulesFromElementList<CarDTO>(xd, "/mathexps/mathexp");
            Func<CarDTO, double> check = math.GetRule<CarDTO>("AskingVersusSelling").Compile();
            var result = check(car);
            Assert.AreEqual(check(car), -1000.000);
        }
    }
}

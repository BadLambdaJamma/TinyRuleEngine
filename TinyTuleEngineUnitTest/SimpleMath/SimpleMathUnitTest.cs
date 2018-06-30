using System;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TinyRuleEngine.Engines;
using System.IO;

namespace TinyRuleEngineTest.SimpleMath
{
    [TestClass]
    public class SimpleMathUnitTest : BaseUnitTest
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
                AverageSellingPrice = 9500.000m,
                FixedComission = 100
            };
            var math = new SimpleMathEngine();
            var xd = new XmlDocument();
            xd.Load(Path.Combine(this.testFilePath,@"SimpleMath\SimpleMath.xml"));
            math.LoadRulesFromElementList<CarDTO>(xd, "/mathexps/mathexp");
            Func<CarDTO, double> check = math.GetRule<CarDTO>("AskingMinusSellingPlusComission").Compile();
            var result = check(car);
            Assert.AreEqual(check(car), -900.000);
        }
    }

    [TestClass]
    public class CosMathUnitTest : BaseUnitTest
    {
        /// <summary>
        /// demonstrates the ability of the rule engine to load a complex rule graph from an Xml file
        /// </summary>
        [TestMethod]
        public void CosMathLoadFromFile()
        {
            // rule DTO 
            var car = new CarDTO
            {
                Make = "Ford",
                Year = 2010,
                Model = "Expedition",
                AskingPrice = 10000.0000m,
                SellingPrice = 9000.0000m,
                AverageSellingPrice = 9500.000m,
                FixedComission = 100
            };
            var math = new SimpleMathEngine();
            var xd = new XmlDocument();
            xd.Load(Path.Combine(this.testFilePath,@"SimpleMath\SimpleMath.xml"));
            math.LoadRulesFromElementList<CarDTO>(xd, "/mathexps/mathexp");
            Func<CarDTO, double> check = math.GetRule<CarDTO>("costest").Compile();
            var result = check(car);
        }
    }

    [TestClass]
    public class PiMathUnitTest :BaseUnitTest
    {
        /// <summary>
        /// demonstrates the ability of the rule engine to load a complex rule graph from an Xml file
        /// </summary>
        [TestMethod]
        public void PiAndOneMathLoadFromFile()
        {
            // rule DTO 
            var car = new CarDTO
            {
                Make = "Ford",
                Year = 2010,
                Model = "Expedition",
                AskingPrice = 10000.0000m,
                SellingPrice = 9000.0000m,
                AverageSellingPrice = 9500.000m,
                FixedComission = 100
            };
            var math = new SimpleMathEngine();
            var xd = new XmlDocument();
            xd.Load(Path.Combine(this.testFilePath,@"SimpleMath\SimpleMath.xml"));
            math.LoadRulesFromElementList<CarDTO>(xd, "/mathexps/mathexp");
            Func<CarDTO, double> check = math.GetRule<CarDTO>("pitest").Compile();
            var result = check(car);
        }
    }


    [TestClass]
    public class LiteralMathUnitTest :BaseUnitTest
    {
        /// <summary>
        /// demonstrates the ability of the rule engine to load a complex rule graph from an Xml file
        /// </summary>
        [TestMethod]
        public void LiteralMathLoadFromFile()
        {
            // rule DTO 
            var car = new CarDTO
            {
                Make = "Ford",
                Year = 2010,
                Model = "Expedition",
                AskingPrice = 10000.0000m,
                SellingPrice = 9000.0000m,
                AverageSellingPrice = 9500.000m,
                FixedComission = 100
            };
            var math = new SimpleMathEngine();
            var xd = new XmlDocument();
            xd.Load(Path.Combine(this.testFilePath,@"SimpleMath\SimpleMath.xml"));
            math.LoadRulesFromElementList<CarDTO>(xd, "/mathexps/mathexp");
            Func<CarDTO, double> check = math.GetRule<CarDTO>("literal").Compile();
            var result = check(car);
        }
    }

}

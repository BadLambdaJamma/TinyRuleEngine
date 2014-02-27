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
                AverageSellingPrice = 9500.000m,
                FixedComission = 100
            };
            var math = new SimpleMathEngine();
            var xd = new XmlDocument();
            xd.Load(@"C:\development\RuleEngine\TinyTuleEngineUnitTest\SimpleMath\SimpleMath.xml");
            math.LoadRulesFromElementList<CarDTO>(xd, "/mathexps/mathexp");
            Func<CarDTO, double> check = math.GetRule<CarDTO>("AskingMinusSellingPlusComission").Compile();
            var result = check(car);
            Assert.AreEqual(check(car), -900.000);
        }
    }

    [TestClass]
    public class CosMathUnitTest
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
            xd.Load(@"C:\development\RuleEngine\TinyTuleEngineUnitTest\SimpleMath\SimpleMath.xml");
            math.LoadRulesFromElementList<CarDTO>(xd, "/mathexps/mathexp");
            Func<CarDTO, double> check = math.GetRule<CarDTO>("costest").Compile();
            var result = check(car);
        }
    }

    [TestClass]
    public class PiMathUnitTest
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
            xd.Load(@"C:\development\RuleEngine\TinyTuleEngineUnitTest\SimpleMath\SimpleMath.xml");
            math.LoadRulesFromElementList<CarDTO>(xd, "/mathexps/mathexp");
            Func<CarDTO, double> check = math.GetRule<CarDTO>("pitest").Compile();
            var result = check(car);
        }
    }


    [TestClass]
    public class LiteralMathUnitTest
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
            xd.Load(@"C:\development\RuleEngine\TinyTuleEngineUnitTest\SimpleMath\SimpleMath.xml");
            math.LoadRulesFromElementList<CarDTO>(xd, "/mathexps/mathexp");
            Func<CarDTO, double> check = math.GetRule<CarDTO>("literal").Compile();
            var result = check(car);
        }
    }

}

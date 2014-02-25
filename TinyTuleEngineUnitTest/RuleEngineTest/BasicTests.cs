using System;
using System.Linq.Expressions;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TinyRuleEngine.Engines;

namespace TinyRuleEngineTest.RuleEngineTest
{
    [TestClass]
    public class BasicTests
    {
        /// <summary>
        /// this test demostrates the basic functions of the rule engine without a rule xml file.
        /// </summary>
        [TestMethod]
        public void BasicOperatorTest()
        {
            // rule DTO 
            var car = new CarDTO
            {
                Make = "Ford", 
                Year=2010,
                Model="Expedition", 
                AskingPrice=10000.0000m, 
                SellingPrice=9000.0000m
            };


            // build up some rules
            var carRule1 = new Rule("Year", "2010", "GreaterThanOrEqual");
            var carRule2 = new Rule("AskingPrice", "10000.0000", "LessThanOrEqual");
            var carRule3= new Rule("Make", "Ford", "Equals");
            var carRule4 = new Rule("Model", "Ex", "StartsWith");  //gets the excursion, explorer, expedition -gass guzzlers

            var re = new RuleEngine();

            // Compile the rules as a seperate step 
            Func<CarDTO, bool> carRule1Compiled = re.Compile<CarDTO>(carRule1);
            Func<CarDTO, bool> carRule2Compiled = re.Compile<CarDTO>(carRule2);
            Func<CarDTO, bool> carRule3Compiled = re.Compile<CarDTO>(carRule3);
            Func<CarDTO, bool> carRule4Compiled = re.Compile<CarDTO>(carRule4);

            Assert.AreEqual(true,carRule1Compiled(car));
            Assert.AreEqual(true,carRule2Compiled(car));
            Assert.AreEqual(true,carRule3Compiled(car));
            Assert.AreEqual(true,carRule4Compiled(car));
        }


        /// <summary>
        /// this test demonstrates the ability to compose your own rules - extra syntax sauce really.
        /// </summary>
        [TestMethod]
        public void PredicatesTest()
        {  
            // rule DTO 
            var car = new CarDTO
            {
                Make = "Ford",
                Year = 2010,
                Model = "Expedition",
                AskingPrice = 10000.0000m,
                SellingPrice = 9000.0000m
            };

            // build up some rules
            var carRule1 = new Rule("Year", "2010", "GreaterThanOrEqual");
            var carRule2 = new Rule("AskingPrice", "10000.0000", "LessThanOrEqual");
            var carRule3 = new Rule("Make", "Ford", "Equals");
            var carRule4 = new Rule("Model", "Ex", "StartsWith");  //gets the excursion, explorer, expedition -gas guzzlers

            var re = new RuleEngine();

            // Compile the rules as a seperate step 
            Expression<Func<CarDTO, bool>> carRule1Exp = re.GetExpression<CarDTO>(carRule1);
            Expression<Func<CarDTO, bool>> carRule2Exp = re.GetExpression<CarDTO>(carRule2);
            Expression<Func<CarDTO, bool>> carRule3Exp = re.GetExpression<CarDTO>(carRule3);
            Expression<Func<CarDTO, bool>> carRule4Exp = re.GetExpression<CarDTO>(carRule4);

            // join the rules with fluent syntax ala predicate builder
            Expression<Func<CarDTO, bool>> compundedRule = carRule1Exp.Or(carRule2Exp).Or(carRule3Exp).Or(carRule4Exp);
            Func<CarDTO, bool> compiledRule = compundedRule.Compile();
            Assert.AreEqual(true, compiledRule(car), "car");
        }

        /// <summary>
        /// this test demonstrates the ability to compose your own rules - extra syntax sauce really.
        /// </summary>
        [TestMethod]
        public void EngineSpeedTest()
        {
            var start = DateTime.Now;
            for (int x = 0; x < 10000; x++)
            {
                PredicatesTest();
            }
            var end = DateTime.Now;
            var ts = end - start;
        }





        /// <summary>
        /// demonstrates the ability of the rule engine to maintain a stateful list of rules (as expression trees)
        /// </summary>
        [TestMethod]
        public void StoreSomeRules()
        {
            // rule DTO 
            var car = new CarDTO
            {
                Make = "Ford",
                Year = 2010,
                Model = "Expedition",
                AskingPrice = 10000.0000m,
                SellingPrice = 9000.0000m
            };

            // build up some rules
            var carRule1 = new Rule("Year", "2010", "GreaterThanOrEqual");

            var re = new RuleEngine();

            // Get the rule expressions
            Expression<Func<CarDTO, bool>> carRule1Exp =re.GetExpression<CarDTO>(carRule1);

            // Save a rule Expression to the the list
            re.LoadRule("carrule", carRule1Exp);
            Assert.AreEqual(true, (re.GetRule<CarDTO>("carrule").Compile()(car)),
                "This car is greater than or equal to year 2010 but failed to rule as such");
        }

        /// <summary>
        /// demonstrates the ability of the rule engine to load a complex rule graph from an Xml file
        /// </summary>
        [TestMethod]
        public void LoadFromFile()
        {
            // rule DTO 
            var car = new CarDTO
            {
                Make = "Ford",
                Year = 2010,
                Model = "Expedition",
                AskingPrice = 10000.0000m,
                SellingPrice = 9000.0000m
            };
            var re = new RuleEngine();
            var xd = new XmlDocument();
            xd.Load(@"C:\development\RuleEngine\TinyTuleEngineUnitTest\RuleEngineTest\RuleSetBasic.xml");
            re.LoadRulesFromElementList<CarDTO>(xd,"/rules/rule");
            Func<CarDTO, bool> isGasGuzzler = re.GetRule<CarDTO>("IsGasGuzzler").Compile();
            Assert.AreEqual(isGasGuzzler(car),true);
         }
    }    
}

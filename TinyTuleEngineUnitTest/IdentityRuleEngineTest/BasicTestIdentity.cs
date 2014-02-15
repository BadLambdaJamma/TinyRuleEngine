using System;
using System.Linq.Expressions;
using System.Xml;
using Microsoft.IdentityModel.Claims;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TinyRuleEngine.Engines;

namespace TinyRuleEngineTest.IdentityRuleEngineTest
{
    [TestClass]
    public class BasicTestIdentity
    {
        /// <summary>
        /// this test demostrates the basic functions of the identity (claimsprincipal) rule engine without a rule xml file.
        /// </summary>
        [TestMethod]
        public void IdentityBasicOperatorTest()
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

            // programatic mix in of claims rule and rule against the DTO user
            var carRule1 = new Rule("Year", "2010", "GreaterThanOrEqual");
            var claimRule = new Rule("@User", "S-1-5-21-2493390151-660934664-2262481224-513", "http://schemas.microsoft.com/ws/2008/06/identity/claims/groupsid");

            // Compile the rules as a seperate step 
            Expression<Func<CarDTO, IClaimsPrincipal, bool>> carRule1Expression = TinyRuleEngine.Engines.IdentityRuleEngine.GetExpression<CarDTO>(carRule1);
            Expression<Func<CarDTO, IClaimsPrincipal, bool>> claimRuleExpression = TinyRuleEngine.Engines.IdentityRuleEngine.GetExpression<CarDTO>(claimRule);
            Expression<Func<CarDTO, IClaimsPrincipal, bool>> compositeRule = carRule1Expression.Or(claimRuleExpression);
            
            // invoke the rules as a third step
            IClaimsPrincipal id = new ClaimsPrincipal(System.Threading.Thread.CurrentPrincipal);
            Assert.AreEqual(true,compositeRule.Compile()(car,id));
         
        }


        /// <summary>
        /// demonstrates the ability of the rule engine to load a complex rule graph from an Xml file
        /// </summary>
        [TestMethod]
        public void IdentityLoadFromFile()
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

            // Load all rules applied to the user type.
            XmlDocument xd = new XmlDocument();
            xd.Load(@"C:\development\RuleEngine\TinyTuleEngineUnitTest\IdentityRuleEngineTest\RuleSetBasicIdentity.xml");
            TinyRuleEngine.Engines.IdentityRuleEngine.LoadRulesFromElementList<CarDTO>(xd, "/rules/rule");

            Func<CarDTO, IClaimsPrincipal, bool> fordSaleApprover = TinyRuleEngine.Engines.IdentityRuleEngine.GetRule<CarDTO>("FordSaleApprover").Compile();
            IClaimsPrincipal id = new ClaimsPrincipal(System.Threading.Thread.CurrentPrincipal);
            Assert.AreEqual(true, fordSaleApprover(car, id));

        }
    }
}

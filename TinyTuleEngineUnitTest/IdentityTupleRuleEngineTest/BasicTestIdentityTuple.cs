using System;
using System.Linq.Expressions;
using System.Xml;
using Microsoft.IdentityModel.Claims;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TinyRuleEngine.Engines;

namespace TinyRuleEngineTest.IdentityTuppleRuleEngineTest
{
    [TestClass]
    public class BasicTestIdentityTupple
    {
        /// <summary>
        /// this test demostrates the basic functions of the identity (claimsprincipal) rule engine without a rule xml file.
        /// </summary>
        [TestMethod]
        public void IdentityTupleBasicOperatorTest()
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

            var salesperson = new SalesPersonDTO
            {
                State = "PA",
                IsManager = true,
                MaximumDiscount = 1000.0000m
            };

            // programatic mix in of claims rule and rule against the DTO for Car and SalesPerson
            var carRule1 = new Rule("Year", "2010", "GreaterThanOrEqual","CarDTO");
            var claimRule = new Rule("@User", "S-1-5-21-2493390151-660934664-2262481224-513", "http://schemas.microsoft.com/ws/2008/06/identity/claims/groupsid");
            var salePersonRule1 = new Rule("State", "PA", "Equals","SalesPersonDTO");
            var salePersonRule2 = new Rule("IsManager", "true", "Equals", "SalesPersonDTO");
            
            var re = new IdentityTupleRuleEngine();

            // build Some Expressions 
            Expression<Func<CarDTO, SalesPersonDTO,IClaimsPrincipal, bool>> carRule1Expression = re.GetExpression<CarDTO,SalesPersonDTO>(carRule1);
            Expression<Func<CarDTO, SalesPersonDTO, IClaimsPrincipal, bool>> claimRuleExpression = re.GetExpression<CarDTO, SalesPersonDTO>(claimRule);
            Expression<Func<CarDTO, SalesPersonDTO, IClaimsPrincipal, bool>> sp1Expression = re.GetExpression<CarDTO, SalesPersonDTO>(salePersonRule1);
            Expression<Func<CarDTO, SalesPersonDTO, IClaimsPrincipal, bool>> sp2Expression = re.GetExpression<CarDTO, SalesPersonDTO>(salePersonRule2);


            Expression<Func<CarDTO, SalesPersonDTO, IClaimsPrincipal, bool>> coumpoundExpr = 
                carRule1Expression.Or(claimRuleExpression).Or(sp1Expression).Or(sp2Expression);
            
            IClaimsPrincipal id = new ClaimsPrincipal(System.Threading.Thread.CurrentPrincipal);
            Assert.AreEqual(true, coumpoundExpr.Compile()(car,salesperson,id));
        }


        /// <summary>
        /// demonstrates the ability of the rule engine to load a complex rule graph from an Xml file
        /// </summary>
        [TestMethod]
        public void IndentityTupleLoadFromFile()
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

            var salesperson = new SalesPersonDTO
            {
                State = "PA",
                IsManager = true,
                MaximumDiscount = 1000.0000m
            };

            // Load all rules applied to the user type.
            var re = new IdentityTupleRuleEngine();
            var xd = new XmlDocument();
            xd.Load(@"C:\development\RuleEngine\TinyTuleEngineUnitTest\IdentityTupleRuleEngineTest\RuleSetBasicIdentityTuple.xml");
            re.LoadRulesFromElementList<CarDTO, SalesPersonDTO>(xd, "/rules/rule");
            IClaimsPrincipal id = new ClaimsPrincipal(System.Threading.Thread.CurrentPrincipal);
            Func<CarDTO, SalesPersonDTO, IClaimsPrincipal,bool> fordSaleApproverWithSalesPersonInfo = re.GetRule<CarDTO, SalesPersonDTO>("FordSaleApproverSalesPerson").Compile();
            Assert.AreEqual(true, fordSaleApproverWithSalesPersonInfo(car, salesperson,id));

        }

    }
}

﻿using System;
using System.Linq.Expressions;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TinyRuleEngine.Engines;

namespace TinyRuleEngineTest.TuppleRuleEngineTest
{
    [TestClass]
    public class BasicTestTupples
    {
        /// <summary>
        /// this test demostrates the basic functions of the identity (claimsprincipal) rule engine without a rule xml file.
        /// </summary>
        [TestMethod]
        public void TupplesBasicOperatorTest()
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

            // programatic mix in of claims rule and rule against the DTO user
            var carRule1 = new Rule("Year", "2010", "GreaterThanOrEqual","CarDTO");
            var salePersonRule1 = new Rule("State", "PA", "Equals", "SalesPersonDTO");
            var salePersonRule2 = new Rule("IsManager", "true", "Equals", "SalesPersonDTO");

            // Compile the rules as a seperate step 
            Expression<Func<CarDTO, SalesPersonDTO, bool>> carRule1Expression = TinyRuleEngine.Engines.TuppleRuleEngine.GetExpression<CarDTO, SalesPersonDTO>(carRule1);
            Expression<Func<CarDTO, SalesPersonDTO, bool>> salePersonRuleExpression1 = TinyRuleEngine.Engines.TuppleRuleEngine.GetExpression<CarDTO, SalesPersonDTO>(salePersonRule1);
            Expression<Func<CarDTO, SalesPersonDTO, bool>> salePersonRuleExpression2 = TinyRuleEngine.Engines.TuppleRuleEngine.GetExpression<CarDTO, SalesPersonDTO>(salePersonRule2);
            Expression<Func<CarDTO, SalesPersonDTO, bool>> compositeRule
                = carRule1Expression.Or(salePersonRuleExpression1).Or(salePersonRuleExpression2);

            Assert.AreEqual(true, compositeRule.Compile()(car, salesperson));

        }


        /// <summary>
        /// demonstrates the ability of the rule engine to load a complex rule graph from an Xml file
        /// </summary>
        [TestMethod]
        public void TupplesLoadFromFile()
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
            var xd = new XmlDocument();
            xd.Load(@"C:\development\RuleEngine\TinyTuleEngineUnitTest\TuppleRuleEngineTest\RuleSetTupple.xml");
            TinyRuleEngine.Engines.TuppleRuleEngine.LoadRulesFromElementList<CarDTO,SalesPersonDTO>(xd, "/rules/rule");

            Func<CarDTO, SalesPersonDTO, bool> fordSaleApproverWithSalesPersonInfo = TinyRuleEngine.Engines.TuppleRuleEngine.GetRule<CarDTO,SalesPersonDTO>("FordSaleApproverWithSalesPersonInfo").Compile();
            Assert.AreEqual(true, fordSaleApproverWithSalesPersonInfo(car, salesperson));

        }


    }
}
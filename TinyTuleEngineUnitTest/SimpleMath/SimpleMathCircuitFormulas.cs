using System;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TinyRuleEngine.Engines;

namespace TinyRuleEngineTest.SimpleMath
{
    [TestClass]
    public class CircuitMathUnitTest
    {
        /// <summary>
        /// demonstartes the math engine calculate the Resonant Frequncy of a Tank circuit
        /// 
        ///            1
        ///  ==========================
        /// 2 * Pi * SquareRoot((L*C)
        /// 
        /// 
        /// 
        /// </summary>
        [TestMethod]
        public void ResonantFrequencyOfATankCircuit()
        {
            // circuitDTO 
            var circuit = new CircuitDTO
            {
                InductanceInHenries = .1000m,  // 100 millihenries
                CapacitanceInFarads = .00001m  // 10 micro farads
            };

            var math = new SimpleMathEngine();
            var xd = new XmlDocument();
            xd.Load(@"C:\development\RuleEngine\TinyTuleEngineUnitTest\SimpleMath\SimpleMath.xml");
            math.LoadRulesFromElementList<CircuitDTO>(xd, "/mathexps/mathexp");
            Func<CircuitDTO, double> resonantFrequencyOfATankCircuit = math.GetRule<CircuitDTO>("ResonantFrequencyOfATankCircuit").Compile();
            var result = resonantFrequencyOfATankCircuit(circuit);
            Assert.AreEqual(159.0,result);
        }
    }
}
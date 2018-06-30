using System;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TinyRuleEngine.Engines;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace TinyRuleEngineTest.SimpleMath
{
    [TestClass]
    public class CircuitMathUnitTest : BaseUnitTest
    {
        /// <summary>
        /// Demonstrates the math engine calculating the Resonant Frequncy of a Tank circuit
        /// 
        ///             1
        /// =========================
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

            var s = new DataContractSerializer(typeof(CircuitDTO));
            var m = new MemoryStream();
            s.WriteObject(m,circuit);
            m.Position = 0;
            var sr = new StreamReader(m);
            var resulta = sr.ReadToEnd();

            var math = new SimpleMathEngine();
            var xd = new XmlDocument();
            xd.Load(Path.Combine(this.testFilePath,@"SimpleMath\SimpleMath.xml"));
            math.LoadRulesFromElementList<CircuitDTO>(xd, "/mathexps/mathexp");
            Func<CircuitDTO, double> resonantFrequencyOfATankCircuit = math.GetRule<CircuitDTO>("ResonantFrequencyOfATankCircuit").Compile();
            var result = resonantFrequencyOfATankCircuit(circuit);
            Assert.AreEqual(159.0,result);
        }
    }
}
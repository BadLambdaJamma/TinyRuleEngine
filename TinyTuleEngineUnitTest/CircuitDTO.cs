namespace TinyRuleEngineTest
{

    using System;
    using System.Runtime.Serialization;
    using System.Xml.Serialization;
    using System.Linq;
    using System.Linq.Expressions;

    /// <summary>
    /// sample DTO class for testing
    /// </summary>
    public class CircuitDTO : IXmlSerializable
    {

        public decimal InductanceInHenries { get; set; }

        public decimal CapacitanceInFarads { get; set; }

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(System.Xml.XmlReader reader) { }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            writer.WriteAttributeString("inductanceinhenries", Convert.ToString(InductanceInHenries));
            writer.WriteAttributeString("capacitanceinfarads", Convert.ToString(CapacitanceInFarads));
        }

    }
}

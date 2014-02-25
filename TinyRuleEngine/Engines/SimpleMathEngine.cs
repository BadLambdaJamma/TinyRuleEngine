using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Xml;
using TinyRuleEngine.Readers;

namespace TinyRuleEngine.Engines
{
    //
    // tiny rule engine -  a basic rule engine for .NET 
    //
    public class SimpleMathEngine
    {
        /// <summary>
        /// hold a list of rules
        /// </summary>
        private readonly Dictionary<string, object> MathExpr = new Dictionary<string, object>();

        public Expression<Func<T, double>> GetExpression<T>(MathValue m)
        {
            var param = Expression.Parameter(typeof(T));
            Expression expr = BuildExpression(m, param);
            return Expression.Lambda<Func<T, double>>(expr, param);
        }

        private Expression BuildExpression(MathValue r, ParameterExpression param)
        {
            return Expression.PropertyOrField(param, r.MemberName);
        }

        public void LoadRulesFromFile<T>(string fileName, string nodePath)
        {
            var xd = new XmlDocument();
            xd.Load(fileName);
            LoadRulesFromElementList<T>(xd, nodePath);
        }

        public void LoadRulesFromElementList<T>(XmlDocument xd, string nodePath)
        {
            if (xd.DocumentElement != null)
            {
                XmlNodeList rulesnodes = xd.DocumentElement.SelectNodes(nodePath);
                string matchedTypeName = typeof(T).ToString();
                if (rulesnodes != null)
                    foreach (XmlNode node in rulesnodes)
                    {
                        if (node.NodeType == XmlNodeType.Comment) continue;
                        if (node.Attributes != null && matchedTypeName == node.Attributes["appliesto"].Value)
                        {
                            XmlDictionaryReader rdr = XmlDictionaryReader.CreateDictionaryReader(new XmlTextReader(new StringReader(node.OuterXml)));
                            rdr.MoveToContent();
                            LoadRule(node.Attributes["name"].Value, new SimpleMathExpressionReader<T>().ReadRule(rdr));
                        }
                    }
            }
        }

        public void LoadRule<T>(string ruleKey, Expression<Func<T, double>> rule)
        {
            MathExpr.Add(ruleKey, rule);
        }

        public Expression<Func<T, double>> GetRule<T>(string ruleKey)
        {
            return MathExpr[ruleKey] as Expression<Func<T, double>>;
        }
    }


}
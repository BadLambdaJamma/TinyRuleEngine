using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Xml;
using TinyRuleEngine.Readers;

namespace TinyRuleEngine.Engines
{
    //
    // simple math engine -  supports a math expression available in the CLR expressed as recursive descent XML
    //
    public class SimpleMathEngine
    {
        /// <summary>
        /// hold a list of rules
        /// </summary>
        private readonly Dictionary<string, object> _mathExpr = new Dictionary<string, object>();

        /// <summary>
        /// Gets the expression
        /// </summary>
        /// <typeparam name="T">The expression DTO type.</typeparam>
        /// <param name="m">The DTO member information</param>
        /// <returns>The expression</returns>
        public Expression<Func<T, double>> GetExpression<T>(MathValue m)
        {
            var param = Expression.Parameter(typeof(T));
            Expression expr = BuildExpression(m, param);
            return Expression.Lambda<Func<T, double>>(expr, param);
        }

        /// <summary>
        /// builds the actual expression supports special @operators
        /// </summary>
        /// <param name="r">The math value</param>
        /// <param name="param">The parameter expression.</param>
        /// <returns>The built expression.</returns>
        private Expression BuildExpression(MathValue r, ParameterExpression param)
        {
            // we support some built in value items via the "@" symbol :
            // @Pi = Math.Pi
            // @1 = the double value of 1
            // @LogBase = the natural logarithmic base
            // @@Literal value
            if (r.MemberName.StartsWith("@@"))
            {
                var value = Double.Parse(r.MemberName.Substring(2));
                return Expression.Constant(value);
            }
            switch (r.MemberName)
            {
                case "@Pi":
                    return Expression.Convert(Expression.Constant(Math.PI), typeof(double));
                case "@1":
                    return Expression.Convert(Expression.Constant(1), typeof(double));
                case "@LogBase":
                    return Expression.Convert(Expression.Constant(Math.E), typeof(double));
                default:
                    return Expression.Convert(Expression.PropertyOrField(param, r.MemberName), typeof (double));
            }
        }

        /// <summary>
        /// loads math expressions from a file
        /// </summary>
        /// <typeparam name="T">The DTO for the expression.</typeparam>
        /// <param name="fileName">The file name.</param>
        /// <param name="nodePath">The xPath of the expressions to load</param>
        public void LoadRulesFromFile<T>(string fileName, string nodePath)
        {
            var xd = new XmlDocument();
            xd.Load(fileName);
            LoadRulesFromElementList<T>(xd, nodePath);
        }

        /// <summary>
        /// Loads Math expressions from a list of elements
        /// </summary>
        /// <typeparam name="T">The DTO for the expression</typeparam>
        /// <param name="xd">The XML document</param>
        /// <param name="nodePath">The xPath expression</param>
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

        /// <summary>
        /// Loads a math expression into the list
        /// </summary>
        /// <typeparam name="T">The DTO type for the math expression</typeparam>
        /// <param name="ruleKey">The key for the math expression</param>
        /// <param name="rule">The math expression being loaded</param>
        public void LoadRule<T>(string ruleKey, Expression<Func<T, double>> rule)
        {
            _mathExpr.Add(ruleKey, rule);
        }

        /// <summary>
        /// Gets a math expression from the list
        /// </summary>
        /// <typeparam name="T">The DTO rule type.</typeparam>
        /// <param name="ruleKey">The rule key</param>
        /// <returns>The rule expression</returns>
        public Expression<Func<T, double>> GetRule<T>(string ruleKey)
        {
            return _mathExpr[ruleKey] as Expression<Func<T, double>>;
        }
    }
}
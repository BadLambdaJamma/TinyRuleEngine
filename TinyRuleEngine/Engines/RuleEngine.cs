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
    public static class RuleEngine
    {
        /// <summary>
        /// hold a list of rules
        /// </summary>
        private static readonly Dictionary<string, object> Rules =  new Dictionary<string,object>();

        /// <summary>
        /// compile a rule
        /// </summary>
        /// <typeparam name="T">the type to compile the rule against</typeparam>
        /// <param name="r">The rule</param>
        /// <returns>A Func to the method</returns>
        static public Func<T, bool> Compile<T>(Rule r)
        {
            var param = Expression.Parameter(typeof(T));
            Expression expr = BuildExpression(r, param);
            return Expression.Lambda<Func<T, bool>>(expr, param).Compile();
        }

        /// <summary>
        /// get an expression given a rule
        /// </summary>
        /// <typeparam name="T">The type the rule is matched to</typeparam>
        /// <param name="r">the rule</param>
        /// <returns>An Expression tree of Func"/></returns>
        static public Expression<Func<T, bool>> GetExpression<T>(Rule r)
        {
            var param = Expression.Parameter(typeof(T));
            Expression expr = BuildExpression(r, param);
            return Expression.Lambda<Func<T, bool>>(expr, param);
        }

        /// <summary>
        ///  builds the rule expression
        /// </summary>
        /// <param name="r">the rule</param>
        /// <param name="param">the parameter expression for the rule type</param>
        /// <returns></returns>
        private static Expression BuildExpression(Rule r, ParameterExpression param)
        {
            Expression propExpression = Expression.PropertyOrField(param, r.MemberName);
            Type propType = propExpression.Type;
            ExpressionType tBinary;
            if (Enum.TryParse(r.Operator, out tBinary))
            {
                var right = Expression.Constant(Convert.ChangeType(r.TargetValue, propType));
                return Expression.MakeBinary(tBinary, propExpression, right);
            }
            else
            {
                var method = propType.GetMethod(r.Operator, new[] { propType });
                var tParam = method.GetParameters()[0].ParameterType;
                var right = Expression.Constant(Convert.ChangeType(r.TargetValue, tParam));
                return Expression.Call(propExpression, method, right);
            }
        }

        /// <summary>
        /// load a rule into the rule engine
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ruleKey"></param>
        /// <param name="rule"></param>
        public static void LoadRule<T>(string ruleKey, Expression<Func<T, bool>> rule)
        {
            Rules.Add(ruleKey, rule);
        }

        /// <summary>
        /// load rules from an XML file
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fileName"></param>
        /// <param name="nodePath"></param>
        public static void LoadRulesFromFile<T>(string  fileName, string nodePath)
        {
            var xd = new XmlDocument();
            xd.Load(fileName);
            LoadRulesFromElementList<T>(xd, nodePath);
        }

        /// <summary>
        /// Load the rules from an XML document
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="xd"></param>
        /// <param name="nodePath"></param>
        public static void LoadRulesFromElementList<T>(XmlDocument xd, string nodePath)
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
                            LoadRule(node.Attributes["name"].Value,new RuleReader<T>().ReadRule(rdr));
                        }
                    }
            }
        }

        public static Expression<Func<T, bool>> GetRule<T>(string ruleKey)
        {
            return Rules[ruleKey] as Expression<Func<T, bool>>;
        }
    }



    // Allows expressions to be joined together in an ad-hoc fashion
    public static class PredicateBuilder
    {
        public static Expression<Func<T, bool>> True<T>() { return f => true; }
        public static Expression<Func<T, bool>> False<T>() { return f => false; }

        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
        {
            var invokedExpr = Expression.Invoke(expr2, expr1.Parameters);
            return Expression.Lambda<Func<T, bool>> (Expression.OrElse(expr1.Body, invokedExpr), expr1.Parameters);
        }

        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> expr1,Expression<Func<T, bool>> expr2)
        {
            var invokedExpr = Expression.Invoke(expr2, expr1.Parameters);
            return Expression.Lambda<Func<T, bool>> (Expression.AndAlso(expr1.Body, invokedExpr), expr1.Parameters);
        }

        public static Expression<Func<T, bool>> Xor<T>(this Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
        {
            var invokedExpr = Expression.Invoke(expr2, expr1.Parameters);
            return Expression.Lambda<Func<T, bool>>(Expression.ExclusiveOr(expr1.Body, invokedExpr), expr1.Parameters);
        }
    }
}
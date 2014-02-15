using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Xml;
using TinyRuleEngine.Readers;

namespace TinyRuleEngine.Engines
{
    //
    // tiny rule engine -  a tuple variation for evaluating a rule set against two Rule DTOs in the same rule
    //
    public static class TuppleRuleEngine
    {
        /// <summary>
        /// hold a list of rules
        /// </summary>
        private static readonly Dictionary<string, object> Rules = new Dictionary<string, object>();

        /// <summary>
        /// compile a tuple rule of T, TK
        /// </summary>
        /// <typeparam name="T">the type to compile the rule against</typeparam>
        /// <typeparam name="TK">the second type to compile the rule against</typeparam>
        /// <param name="rule">The rule DTO</param> 
        /// <returns>A Func to the method</returns>
        static public Func<T,TK, bool> Compile<T,TK>(Rule rule)
        {
            var param1 = Expression.Parameter(typeof(T));
            var param2 = Expression.Parameter(typeof(TK));
            Expression expr = BuildExpression(rule, param1,param2);
            return Expression.Lambda<Func<T, TK, bool>>(expr, param1,param2).Compile();
        }

        /// <summary>
        /// get an expression given a rule
        /// </summary>
        /// <typeparam name="T">The type the rule is matched to</typeparam>
        /// <typeparam name="TK">The second type the rule is matched to</typeparam>
        /// <param name="rule">the first rule</param>
        /// <returns>An Expression tree of Func"/></returns>
        static public Expression<Func<T, TK,bool>> GetExpression<T,TK>(Rule rule)
        {
            var param1 = Expression.Parameter(typeof(T));
            var param2 = Expression.Parameter(typeof(TK));
            Expression expr = BuildExpression(rule, param1, param2);
            return Expression.Lambda<Func<T, TK,bool>>(expr, param1, param2);
        }

        /// <summary>
        ///  builds the tupple rule expression
        /// </summary>
        /// <param name="rule">the rule</param>
        /// <param name="param">the first expression for the tupple rule type</param>
        /// <param name="param2">the second expression for the tupple rule type</param>
        /// <returns></returns>
        private static Expression BuildExpression(Rule rule, ParameterExpression param, ParameterExpression param2)
        {
            // the rule dictates what parameter expression is uses with its "uses" attribute
            if (rule.Uses == param.Type.Name)
            {
                var propExpression = Expression.PropertyOrField(param, rule.MemberName);
                Type propType = propExpression.Type;
                ExpressionType tBinary;
                if (Enum.TryParse(rule.Operator, out tBinary))
                {
                    var right = Expression.Constant(Convert.ChangeType(rule.TargetValue, propType));
                    return Expression.MakeBinary(tBinary, propExpression, right);
                }
                else
                {
                    var method = propType.GetMethod(rule.Operator, new[] {propType});
                    var tParam = method.GetParameters()[0].ParameterType;
                    var right = Expression.Constant(Convert.ChangeType(rule.TargetValue, tParam));
                    return Expression.Call(propExpression, method, right);
                }
            }
            else
            {
                var propExpression = Expression.PropertyOrField(param2, rule.MemberName);
                Type propType = propExpression.Type;
                ExpressionType tBinary;
                if (Enum.TryParse(rule.Operator, out tBinary))
                {
                    var right = Expression.Constant(Convert.ChangeType(rule.TargetValue, propType));
                    return Expression.MakeBinary(tBinary, propExpression, right);
                }
                else
                {
                    var method = propType.GetMethod(rule.Operator, new[] { propType });
                    var tParam = method.GetParameters()[0].ParameterType;
                    var right = Expression.Constant(Convert.ChangeType(rule.TargetValue, tParam));
                    return Expression.Call(propExpression, method, right);
                }
                
            }
        }


        /// <summary>
        /// load a rule into the rule engine
        /// </summary>
        /// <typeparam name="T">The type of the Rule</typeparam>
        /// <typeparam name="TK">The tupple type of the Rule</typeparam>
        /// <param name="ruleKey">the rule key</param>
        /// <param name="rule">the expression to be saved</param>
        public static void LoadRule<T,TK>(string ruleKey, Expression<Func<T, TK, bool>> rule)
        {
            Rules.Add(ruleKey, rule);
        }

        /// <summary>
        /// load rules from an XML file
        /// </summary>
        /// <typeparam name="T">the rule DTO type</typeparam>
        /// <typeparam name="TK">the tupple rule DTO type</typeparam>
        /// <param name="fileName">the file name to get the rules from</param>
        /// <param name="nodePath">the xpath filter for nodes</param>
        public static void LoadRulesFromFile<T,TK>(string fileName, string nodePath)
        {
            var xd = new XmlDocument();
            xd.Load(fileName);
            LoadRulesFromElementList<T,TK>(xd, nodePath);
        }

        /// <summary>
        /// Load the rules from an XML document
        /// </summary>
        /// <typeparam name="T">the rule DTO type</typeparam>
        /// <typeparam name="TK">the tupple rule DTO type</typeparam>
        /// <param name="xd">the xml document containing the nodes</param>
        /// <param name="nodePath">the xpath expression for nodes</param>
        public static void LoadRulesFromElementList<T,TK>(XmlDocument xd, string nodePath)
        {
            if (xd.DocumentElement != null)
            {
                XmlNodeList rulesnodes = xd.DocumentElement.SelectNodes(nodePath);
                string matchedTypeName1 = typeof(T).Name;
                string matchedTypeName2 = typeof(TK).Name;
                if (rulesnodes != null)
                    foreach (XmlNode node in rulesnodes)
                    {
                        if (node.NodeType == XmlNodeType.Comment) continue;
                        if (node.Attributes != null && (node.Attributes["appliesto"].Value.StartsWith(matchedTypeName1) || node.Attributes["appliesto"].Value.EndsWith(matchedTypeName2)))
                        {
                            XmlDictionaryReader rdr =
                                XmlDictionaryReader.CreateDictionaryReader(
                                    new XmlTextReader(new StringReader(node.OuterXml)));
                            rdr.MoveToContent();
                            LoadRule(node.Attributes["name"].Value, new TuppleRuleReader<T,TK>().ReadRule(rdr));
                        }
                    }
            }
        }
        

        /// <summary>
        /// gets a saved rule from the rule engine
        /// </summary>
        /// <typeparam name="T">the rule DTO type</typeparam>
        /// <typeparam name="TK">the tupple rule DTO type</typeparam>
        /// <param name="ruleKey">the key of the rule to return</param>
        /// <returns></returns>
        public static Expression<Func<T, TK, bool>> GetRule<T,TK>(string ruleKey)
        {
            return Rules[ruleKey] as Expression<Func<T, TK, bool>>;
        }
    }



    // Allows expressions to be joined together in an ad-hoc fashion
    public static class TupplePredicateBuilder
    {
        public static Expression<Func<T, TK,bool>> True<T,TK>() { return (f,x) => true; }
        public static Expression<Func<T, TK,bool>> False<T,TK>() { return (f,x) => false; }

        public static Expression<Func<T, TK,bool>> Or<T,TK>(this Expression<Func<T, TK,bool>> expr1, Expression<Func<T, TK,bool>> expr2)
        {
            var invokedExpr = Expression.Invoke(expr2, expr1.Parameters);
            return Expression.Lambda<Func<T, TK,bool>>(Expression.OrElse(expr1.Body, invokedExpr), expr1.Parameters);
        }

        public static Expression<Func<T, TK,bool>> And<T,TK>(this Expression<Func<T,TK, bool>> expr1, Expression<Func<T, TK,bool>> expr2)
        {
            var invokedExpr = Expression.Invoke(expr2, expr1.Parameters);
            return Expression.Lambda<Func<T, TK,bool>>(Expression.AndAlso(expr1.Body, invokedExpr), expr1.Parameters);
        }

        public static Expression<Func<T, TK,bool>> Xor<T,TK>(this Expression<Func<T, TK,bool>> expr1, Expression<Func<T, TK,bool>> expr2)
        {
            var invokedExpr = Expression.Invoke(expr2, expr1.Parameters);
            return Expression.Lambda<Func<T, TK,bool>>(Expression.ExclusiveOr(expr1.Body, invokedExpr), expr1.Parameters);
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Xml;
using Microsoft.IdentityModel.Claims;
using TinyRuleEngine.Readers;

namespace TinyRuleEngine.Engines
{
    /// <summary>
    /// supports rule with DTO rule types comingled with a claims principal
    /// </summary>
    public class IdentityTupleRuleEngine
    {
        /// <summary>
        /// hold a list of rules
        /// </summary>
        private readonly Dictionary<string, object> Rules = new Dictionary<string, object>();

        /// <summary>
        /// compile a rule
        /// </summary>
        /// <typeparam name="T">the type to compile the rule against</typeparam>
        /// <param name="r">The rule</param>
        /// <returns>A Func to the method</returns>
        public Func<T, TK, IClaimsPrincipal, bool> Compile<T,TK>(Rule r)
        {
            var param1 = Expression.Parameter(typeof(T));
            var param2 = Expression.Parameter(typeof(TK));
            var idParam = Expression.Parameter(typeof(IClaimsPrincipal));
            Expression expr = BuildExpression(r, param1,param2, idParam);
            return Expression.Lambda<Func<T,TK, IClaimsPrincipal, bool>>(expr, param1, param2,idParam).Compile();
        }

        /// <summary>
        /// Gets a rule Expression supporting T and an IClaimsPrincipal
        /// </summary>
        /// <typeparam name="T">The Rule DTO type</typeparam>
        /// <typeparam name="TK">The tupple Rule DTO type</typeparam>
        /// <param name="r">the rule to derive the expression from</param>
        /// <returns>An expression represnting the rule and an IClaimsPrincipal</returns>
        public Expression<Func<T, TK,IClaimsPrincipal, bool>> GetExpression<T,TK>(Rule r)
        {
            var param1 = Expression.Parameter(typeof(T));
            var param2 = Expression.Parameter(typeof(TK));

            var idParam = Expression.Parameter(typeof(IClaimsPrincipal));
            Expression expr = BuildExpression(r, param1, param2, idParam);
            return Expression.Lambda<Func<T,TK, IClaimsPrincipal, bool>>(expr, param1,param2, idParam);
        }

        /// <summary>
        ///  builds the rule expression
        /// </summary>
        /// <param name="rule">the rule</param>
        /// <param name="param">the parameter expression for the rule type</param>
        /// <param name="param2">the parameter expression for the rule type</param>
        /// <param name="idparam">the second parameter expression for the IClaimsPrincipal</param>
        /// <returns>An Expression for the T,IClaimsPrincipal</returns>
        private Expression BuildExpression(Rule rule, ParameterExpression param, ParameterExpression param2, ParameterExpression idparam)
        {
            // check to see if the special '@User'  token is used
            bool isClaimRule = rule.MemberName.Equals("@User");
            if (isClaimRule)
            {
                Expression<Func<IClaimsPrincipal, string, string, bool>> hasClaimTest
                    = (p, ct, cv) => p.Identities.Any(s => s.Claims.Any(c => c.ClaimType == ct && c.ValueType == ClaimValueTypes.String && c.Value == cv));

                return Expression.Equal(Expression.Invoke(hasClaimTest, idparam, Expression.Constant(rule.Operator), Expression.Constant(rule.TargetValue)), Expression.Constant(true));
            }
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
                    var method = propType.GetMethod(rule.Operator, new[] { propType });
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
        /// <typeparam name="TK">The type of the Rule</typeparam>
        /// <param name="ruleKey">the rule key</param>
        /// <param name="rule">the expression to be saved</param>
        public void LoadRule<T,TK>(string ruleKey, Expression<Func<T,TK, IClaimsPrincipal, bool>> rule)
        {
            Rules.Add(ruleKey, rule);
        }

        /// <summary>
        /// load rules from an XML file
        /// </summary>
        /// <typeparam name="T">the rule DTO type</typeparam>
        /// <typeparam name="TK">the second rule DTO type</typeparam>
        /// <param name="fileName">the file name to get the rules from</param>
        /// <param name="nodePath">the xpath filter for nodes</param>
        public void LoadRulesFromFile<T,TK>(string fileName, string nodePath)
        {
            var xd = new XmlDocument();
            xd.Load(fileName);
            LoadRulesFromElementList<T,TK>(xd, nodePath);
        }

        /// <summary>
        /// Load the rules from an XML document
        /// </summary>
        /// <typeparam name="T">the rule DTO type</typeparam>
        /// <typeparam name="TK">the second rule DTO type</typeparam>
        /// <param name="xd">the xml document containing the nodes</param>
        /// <param name="nodePath">the xpath expression for nodes</param>
        public void LoadRulesFromElementList<T,TK>(XmlDocument xd, string nodePath)
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
                            LoadRule(node.Attributes["name"].Value, new IdentityTuppleRuleReader<T, TK>().ReadRule(rdr));
                        }
                    }
            }
        }

        /// <summary>
        /// gets a saved rule from the rule engine
        /// </summary>
        /// <typeparam name="T">the rule DTO type</typeparam>
        /// <typeparam name="TK">the second rule DTO type</typeparam>
        /// <param name="ruleKey">the key of the rule to return</param>
        /// <returns></returns>
        public Expression<Func<T, TK,IClaimsPrincipal, bool>> GetRule<T,TK>(string ruleKey)
        {
            return Rules[ruleKey] as Expression<Func<T, TK,IClaimsPrincipal, bool>>;
        }
    }

    /// <summary>
    ///  Allows expressions to be joined together in an ad-hoc fashion - Forked from predicate builder to support IClaimsPrincipal + Tupple DTO T,TK in logical joins
    /// </summary>
    public static class IdentityTupplePredicateBuilder
    {
        public static Expression<Func<T, TK,IClaimsPrincipal, bool>> True<T,TK>() { return (p, f, x) => true; }
        public static Expression<Func<T, TK,IClaimsPrincipal, bool>> False<T,TK>() { return (p, f, x) => false; }

        public static Expression<Func<T,TK,IClaimsPrincipal, bool>> Or<T,TK>(this Expression<Func<T, TK,IClaimsPrincipal, bool>> expr1, Expression<Func<T, TK,IClaimsPrincipal, bool>> expr2)
        {
            var invokedExpr = Expression.Invoke(expr2, expr1.Parameters);
            return Expression.Lambda<Func<T, TK,IClaimsPrincipal, bool>>(Expression.OrElse(expr1.Body, invokedExpr), expr1.Parameters);
        }

        public static Expression<Func<T,TK,IClaimsPrincipal, bool>> And<T,TK>(this Expression<Func<T, TK,IClaimsPrincipal, bool>> expr1, Expression<Func<T, TK,IClaimsPrincipal, bool>> expr2)
        {
            var invokedExpr = Expression.Invoke(expr2, expr1.Parameters);
            return Expression.Lambda<Func<T, TK,IClaimsPrincipal, bool>>(Expression.AndAlso(expr1.Body, invokedExpr), expr1.Parameters);
        }

        public static Expression<Func<T, TK,IClaimsPrincipal, bool>> Xor<T,TK>(this Expression<Func<T, TK,IClaimsPrincipal, bool>> expr1, Expression<Func<T,TK, IClaimsPrincipal, bool>> expr2)
        {
            var invokedExpr = Expression.Invoke(expr2, expr1.Parameters);
            return Expression.Lambda<Func<T, TK,IClaimsPrincipal, bool>>(Expression.ExclusiveOr(expr1.Body, invokedExpr), expr1.Parameters);
        }
    }
}
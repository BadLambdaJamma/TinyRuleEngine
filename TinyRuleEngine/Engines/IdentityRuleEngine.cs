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
    public class IdentityRuleEngine
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
        public Func<T, IClaimsPrincipal, bool> Compile<T>(Rule r)
        {
            var param = Expression.Parameter(typeof(T));
            var idParam = Expression.Parameter(typeof(IClaimsPrincipal));
            Expression expr = BuildExpression(r, param,idParam);
            return Expression.Lambda<Func<T, IClaimsPrincipal,bool>>(expr, param, idParam).Compile();
        }

        /// <summary>
        /// Gets a rule Expression supporting T and an IClaimsPrincipal
        /// </summary>
        /// <typeparam name="T">The Rule DTO type</typeparam>
        /// <param name="r">the rule to derive the expression from</param>
        /// <returns>An expression represnting the rule and an IClaimsPrincipal</returns>
        public Expression<Func<T, IClaimsPrincipal, bool>> GetExpression<T>(Rule r)
        {
            var param = Expression.Parameter(typeof(T));
            var idParam = Expression.Parameter(typeof(IClaimsPrincipal));
            Expression expr = BuildExpression(r, param, idParam);
            return Expression.Lambda<Func<T, IClaimsPrincipal, bool>>(expr, param, idParam);
        }

        /// <summary>
        ///  builds the rule expression
        /// </summary>
        /// <param name="r">the rule</param>
        /// <param name="param">the parameter expression for the rule type</param>
        /// <param name="idparam">the parameter expression for the IClaimsPrincipal</param>
        /// <returns>An Expression for the T,IClaimsPrincipal</returns>
        private Expression BuildExpression(Rule r, ParameterExpression param, ParameterExpression idparam)
        {
            // check to see if the special '@User'  token is used
            bool isClaimRule = r.MemberName.Equals("@User");   
            if (isClaimRule)
            {
                Expression<Func<IClaimsPrincipal,string, string, bool>> hasClaimTest 
                    = (p, ct, cv) => p.Identities.Any(s => s.Claims.Any(c => c.ClaimType == ct &&  c.ValueType == ClaimValueTypes.String && c.Value == cv));

               return Expression.Equal(Expression.Invoke(hasClaimTest, idparam, Expression.Constant(r.Operator), Expression.Constant(r.TargetValue)), Expression.Constant(true));
            }

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
        /// <typeparam name="T">The type of the Rule</typeparam>
        /// <param name="ruleKey">the rule key</param>
        /// <param name="rule">the expression to be saved</param>
        public void LoadRule<T>(string ruleKey, Expression<Func<T,IClaimsPrincipal, bool>> rule)
        {
            Rules.Add(ruleKey, rule);
        }

        /// <summary>
        /// load rules from an XML file
        /// </summary>
        /// <typeparam name="T">the rule DTO type</typeparam>
        /// <param name="fileName">the file name to get the rules from</param>
        /// <param name="nodePath">the xpath filter for nodes</param>
        public void LoadRulesFromFile<T>(string fileName, string nodePath)
        {
            var xd = new XmlDocument();
            xd.Load(fileName);
            LoadRulesFromElementList<T>(xd, nodePath);
        }

        /// <summary>
        /// Load the rules from an XML document
        /// </summary>
        /// <typeparam name="T">the rule DTO type</typeparam>
        /// <param name="xd">the xml document containing the nodes</param>
        /// <param name="nodePath">the xpath expression for nodes</param>
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
                            LoadRule(node.Attributes["name"].Value, new IdentityRuleReader<T>().ReadRule(rdr));
                        }
                    }
            }
        }

        /// <summary>
        /// gets a saved rule from the rule engine
        /// </summary>
        /// <typeparam name="T">the rule DTO type</typeparam>
        /// <param name="ruleKey">the key of the rule to return</param>
        /// <returns></returns>
        public Expression<Func<T,IClaimsPrincipal,bool>> GetRule<T>(string ruleKey)
        {
            return Rules[ruleKey] as Expression<Func<T, IClaimsPrincipal, bool>>;
        }
    }

    /// <summary>
    ///  Allows expressions to be joined together in an ad-hoc fashion - Forked from predicate builder to support IClaimsPrincipal in logical joins
    /// </summary>
    public static class IdentityPredicateBuilder
    {
        public static Expression<Func<T, IClaimsPrincipal, bool>> True<T>() { return (p, f) => true; }
        public static Expression<Func<T, IClaimsPrincipal, bool>> False<T>() { return (p, f) => false; }

        public static Expression<Func<T, IClaimsPrincipal, bool>> Or<T>(this Expression<Func<T, IClaimsPrincipal, bool>> expr1, Expression<Func<T, IClaimsPrincipal, bool>> expr2)
        {
            var invokedExpr = Expression.Invoke(expr2, expr1.Parameters);
            return Expression.Lambda<Func<T, IClaimsPrincipal, bool>>(Expression.OrElse(expr1.Body, invokedExpr), expr1.Parameters);
        }

        public static Expression<Func<T, IClaimsPrincipal, bool>> And<T>(this Expression<Func<T, IClaimsPrincipal, bool>> expr1, Expression<Func<T, IClaimsPrincipal, bool>> expr2)
        {
            var invokedExpr = Expression.Invoke(expr2, expr1.Parameters);
            return Expression.Lambda<Func<T, IClaimsPrincipal, bool>>(Expression.AndAlso(expr1.Body, invokedExpr), expr1.Parameters);
        }

        public static Expression<Func<T, IClaimsPrincipal, bool>> Xor<T>(this Expression<Func<T, IClaimsPrincipal, bool>> expr1, Expression<Func<T, IClaimsPrincipal, bool>> expr2)
        {
            var invokedExpr = Expression.Invoke(expr2, expr1.Parameters);
            return Expression.Lambda<Func<T, IClaimsPrincipal, bool>>(Expression.ExclusiveOr(expr1.Body, invokedExpr), expr1.Parameters);
        }
    }
}
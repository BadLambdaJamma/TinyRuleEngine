using System;
using System.Linq.Expressions;
using System.Xml;
using TinyRuleEngine.Engines;

namespace TinyRuleEngine.Readers
{
    /// <summary>
    /// Classic rule reader
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RuleReader<T>
    {
        static readonly Expression<Func<T, bool>> DefaultGeneric = def => false;

        /// <summary>
        /// Read the rule and returns Expression of func of T
        /// </summary>
        /// <param name="rdr">The rule reader</param>
        /// <returns></returns>
        public Expression<Func<T, bool>> ReadRule(XmlDictionaryReader rdr)
        {
            rdr.Read();
            ParameterExpression subject = Expression.Parameter(typeof(T), "subject");
            Expression<Func<T, bool>> result = ReadNode(rdr, subject);
            rdr.ReadEndElement();
            return result;
        }
        
        /// <summary>
        /// make the expression or joins it to an existing expression based on join logic
        /// </summary>
        /// <param name="rdr">the rule reader</param>
        /// <param name="subject">The Expression being built</param>
        /// <returns></returns>
        private Expression<Func<T, bool>> ReadNode(XmlDictionaryReader rdr, ParameterExpression subject)
        {
            switch (rdr.Name)
            {
                case "and":
                     rdr.Read();
                     BinaryExpression andlambda1 = Expression.AndAlso(Expression.Invoke(ReadNode(rdr, subject), subject),Expression.Invoke(ReadNode(rdr, subject), subject));
                     rdr.ReadEndElement();
                     return Expression.Lambda<Func<T, bool>>(andlambda1, subject);

                case "or":
                    rdr.Read();
                    BinaryExpression orlambda1 = Expression.OrElse(Expression.Invoke(ReadNode(rdr, subject), subject),Expression.Invoke(ReadNode(rdr, subject), subject));
                    rdr.ReadEndElement();
                    return  Expression.Lambda<Func<T, bool>>(orlambda1, subject);

                case "xor":
                    rdr.Read();
                    BinaryExpression xorlambda1 = Expression.ExclusiveOr(Expression.Invoke(ReadNode(rdr, subject), subject), Expression.Invoke(ReadNode(rdr, subject), subject));
                    rdr.ReadEndElement();
                    return Expression.Lambda<Func<T, bool>>(xorlambda1, subject);

                case "ruleitem":
                     var r = new Rule(rdr.GetAttribute("membername"),rdr.GetAttribute("targetvalue"), rdr.GetAttribute("operator"));
                     rdr.Read();
                     return new RuleEngine().GetExpression<T>(r);

                default:
                    return DefaultGeneric;
            }
        }
    }
}

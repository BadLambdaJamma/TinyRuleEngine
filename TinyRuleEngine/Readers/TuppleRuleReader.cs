using System;
using System.Linq.Expressions;
using System.Xml;
using TinyRuleEngine.Engines;

namespace TinyRuleEngine.Readers
{
    /// <summary>b
    /// Identity rule reader
    /// </summary>
    /// <typeparam name="T">the DTO of the rule type</typeparam>
    /// <typeparam name="TK">the second DTO of the rule type</typeparam>
    public class TuppleRuleReader<T,TK>
    {
        static readonly Expression<Func<T,TK, bool>> DefaultFalse = (def, p) => false;

        /// <summary>
        /// Read the rule and returns Expression of Func of T
        /// </summary>
        /// <param name="rdr">The rule reader</param>
        /// <returns></returns>
        public Expression<Func<T, TK, bool>> ReadRule(XmlDictionaryReader rdr)
        {
            rdr.Read();
            ParameterExpression subject1 = Expression.Parameter(typeof(T), "subject1");
            ParameterExpression subject2 = Expression.Parameter(typeof(TK), "subject2");
            Expression<Func<T, TK, bool>> result = ReadNode(rdr, subject1, subject2);
            rdr.ReadEndElement();
            return result;
        }

        /// <summary>
        /// make the expression or joins it to an existing expression based on join logic
        /// </summary>
        /// <param name="rdr">the rule reader</param>
        /// <param name="subject">The Expression being built</param>
        /// <param name="subject2">The Second Expression</param>
        /// <returns></returns>
        private Expression<Func<T, TK, bool>> ReadNode(XmlDictionaryReader rdr, ParameterExpression subject, ParameterExpression subject2)
        {
            switch (rdr.Name)
            {
                case "and":
                    rdr.Read();
                    BinaryExpression andlambda1 = Expression.AndAlso(Expression.Invoke(ReadNode(rdr, subject, subject2), subject, subject2), Expression.Invoke(ReadNode(rdr, subject, subject2), subject, subject2));
                    rdr.ReadEndElement();
                    return Expression.Lambda<Func<T, TK, bool>>(andlambda1, subject, subject2);

                case "or":
                    rdr.Read();
                    BinaryExpression orlambda1 = Expression.OrElse(Expression.Invoke(ReadNode(rdr, subject, subject2), subject, subject2), Expression.Invoke(ReadNode(rdr, subject, subject2), subject, subject2));
                    rdr.ReadEndElement();
                    return Expression.Lambda<Func<T, TK, bool>>(orlambda1, subject, subject2);

                case "xor":
                    rdr.Read();
                    BinaryExpression xorlambda1 = Expression.ExclusiveOr(Expression.Invoke(ReadNode(rdr, subject, subject2), subject, subject2), Expression.Invoke(ReadNode(rdr, subject, subject2), subject, subject2));
                    rdr.ReadEndElement();
                    return Expression.Lambda<Func<T, TK, bool>>(xorlambda1, subject, subject2);

                case "ruleitem":
                    var r = new Rule(rdr.GetAttribute("membername"), rdr.GetAttribute("targetvalue"), rdr.GetAttribute("operator"), rdr.GetAttribute("uses"));
                    rdr.Read();
                    var item = TuppleRuleEngine.GetExpression<T,TK>(r);
                    return item;

                default:
                    return DefaultFalse;
            }
        }
    }
}

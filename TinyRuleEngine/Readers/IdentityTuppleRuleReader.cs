using System;
using System.Linq.Expressions;
using System.Xml;
using Microsoft.IdentityModel.Claims;
using TinyRuleEngine.Engines;

namespace TinyRuleEngine.Readers
{
    /// <summary>b
    /// Identity rule reader
    /// </summary>
    /// <typeparam name="T">the DTO of the rule type</typeparam>
    /// <typeparam name="TK">the second DTO of the rule type</typeparam>
    public class IdentityTuppleRuleReader<T,TK>
    {
        static readonly Expression<Func<T, TK, IClaimsPrincipal, bool>> DefaultFalse = (t1, t2, icp) => false;

        /// <summary>
        /// Read the rule and returns Expression of Func of T,TK
        /// </summary>
        /// <param name="rdr">The rule reader</param>
        /// <returns></returns>
        public Expression<Func<T, TK,IClaimsPrincipal, bool>> ReadRule(XmlDictionaryReader rdr)
        {
            rdr.Read();
            ParameterExpression subject1 = Expression.Parameter(typeof(T), "subject1");
            ParameterExpression subject2 = Expression.Parameter(typeof(TK), "subject2");
            ParameterExpression idsubject = Expression.Parameter(typeof(IClaimsPrincipal), "idsubject");
            Expression<Func<T, TK,IClaimsPrincipal, bool>> result = ReadNode(rdr, subject1,subject2, idsubject);
            rdr.ReadEndElement();
            return result;
        }

        /// <summary>
        /// make the expression or joins it to an existing expression based on join logic
        /// </summary>
        /// <param name="rdr">the rule reader</param>
        /// <param name="subject">The Expression being built against </param>
        /// <param name="subject2">The second Expression being built against</param>
        /// <param name="idsubject">The claims pincipal</param>
        /// <returns></returns>
        private Expression<Func<T, TK,IClaimsPrincipal, bool>> ReadNode(XmlDictionaryReader rdr, ParameterExpression subject, ParameterExpression subject2, ParameterExpression idsubject)
        {
            switch (rdr.Name)
            {
                case "and":
                    rdr.Read();
                    BinaryExpression andlambda1 = Expression.AndAlso(Expression.Invoke(ReadNode(rdr, subject, subject2,idsubject), subject, subject2,idsubject), Expression.Invoke(ReadNode(rdr, subject, subject2, idsubject), subject, subject2, idsubject));
                    rdr.ReadEndElement();
                    return Expression.Lambda<Func<T, TK,IClaimsPrincipal, bool>>(andlambda1, subject, subject2,idsubject);

                case "or":
                    rdr.Read();
                    BinaryExpression orlambda1 = Expression.OrElse(Expression.Invoke(ReadNode(rdr, subject, subject2,idsubject), subject, subject2,idsubject), Expression.Invoke(ReadNode(rdr, subject, subject2,idsubject), subject, subject2,idsubject));
                    rdr.ReadEndElement();
                    return Expression.Lambda<Func<T, TK,IClaimsPrincipal, bool>>(orlambda1, subject, subject2,idsubject);

                case "xor":
                    rdr.Read();
                    BinaryExpression xorlambda1 = Expression.ExclusiveOr(Expression.Invoke(ReadNode(rdr, subject, subject2,idsubject), subject, subject2,idsubject), Expression.Invoke(ReadNode(rdr, subject, subject2,idsubject), subject, subject2, idsubject));
                    rdr.ReadEndElement();
                    return Expression.Lambda<Func<T, TK,IClaimsPrincipal, bool>>(xorlambda1, subject, idsubject);

                case "ruleitem":
                    var r = new Rule(rdr.GetAttribute("membername"), rdr.GetAttribute("targetvalue"), rdr.GetAttribute("operator"), rdr.GetAttribute("uses"));
                    rdr.Read();
                    var item = new IdentityTuppleRuleEngine().GetExpression<T,TK>(r);
                    return item;

                default:
                    return DefaultFalse;
            }
        }
    }
}

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
    public class IdentityRuleReader<T>
    {
        static readonly Expression<Func<T, IClaimsPrincipal, bool>> DefaultFalse = (def,p) => false;

        /// <summary>
        /// Read the rule and returns Expression of Func of T
        /// </summary>
        /// <param name="rdr">The rule reader</param>
        /// <returns></returns>
        public Expression<Func<T, IClaimsPrincipal,bool>> ReadRule(XmlDictionaryReader rdr)
        {
            rdr.Read();
            ParameterExpression subject = Expression.Parameter(typeof(T), "subject");
            ParameterExpression idsubject = Expression.Parameter(typeof(IClaimsPrincipal), "idsubject");
            Expression<Func<T, IClaimsPrincipal,bool>> result = ReadNode(rdr, subject, idsubject);
            rdr.ReadEndElement();
            return result;
        }

        /// <summary>
        /// make the expression or joins it to an existing expression based on join logic
        /// </summary>
        /// <param name="rdr">the rule reader</param>
        /// <param name="subject">The Expression being built</param>
        /// <param name="idsubject">The claims pincipal</param>
        /// <returns></returns>
        private Expression<Func<T, IClaimsPrincipal, bool>> ReadNode(XmlDictionaryReader rdr, ParameterExpression subject,ParameterExpression idsubject)
        {
            switch (rdr.Name)
            {
                case "and":
                    rdr.Read();
                    BinaryExpression andlambda1 = Expression.AndAlso(Expression.Invoke(ReadNode(rdr, subject, idsubject), subject,idsubject), Expression.Invoke(ReadNode(rdr, subject, idsubject), subject,idsubject));
                    rdr.ReadEndElement();
                    return Expression.Lambda<Func<T, IClaimsPrincipal,bool>>(andlambda1, subject, idsubject);

                case "or":
                    rdr.Read();
                    BinaryExpression orlambda1 = Expression.OrElse(Expression.Invoke(ReadNode(rdr, subject, idsubject), subject,idsubject), Expression.Invoke(ReadNode(rdr, subject, idsubject), subject, idsubject));
                    rdr.ReadEndElement();
                    return Expression.Lambda<Func<T, IClaimsPrincipal,bool>>(orlambda1, subject, idsubject);

                case "xor":
                    rdr.Read();
                    BinaryExpression xorlambda1 = Expression.ExclusiveOr(Expression.Invoke(ReadNode(rdr, subject, idsubject), subject,idsubject), Expression.Invoke(ReadNode(rdr, subject, idsubject), subject,idsubject ));
                    rdr.ReadEndElement();
                    return Expression.Lambda<Func<T, IClaimsPrincipal,bool>>(xorlambda1, subject, idsubject);

                case "ruleitem":
                    var r = new Rule(rdr.GetAttribute("membername"), rdr.GetAttribute("targetvalue"), rdr.GetAttribute("operator"));
                    rdr.Read();
                    var item = IdentityRuleEngine.GetExpression<T>(r);
                    return item;

                default:
                    return DefaultFalse;
            }
        }
    }
}

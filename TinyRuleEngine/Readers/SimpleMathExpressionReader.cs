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
    public class SimpleMathExpressionReader<T>
    {
        static readonly Expression<Func<T, double>> Zero = def => 0;

        /// <summary>
        /// Read the rule and returns Expression of func of T
        /// </summary>
        /// <param name="rdr">The rule reader</param>
        /// <returns></returns>
        public Expression<Func<T, double>> ReadRule(XmlDictionaryReader rdr)
        {
            rdr.Read();
            ParameterExpression subject = Expression.Parameter(typeof(T), "subject");
            Expression<Func<T, double>> result = ReadNode(rdr, subject);
            rdr.ReadEndElement();
            return result;
        }
        
        /// <summary>
        /// make the expression or joins it to an existing expression based on join logic
        /// </summary>
        /// <param name="rdr">the rule reader</param>
        /// <param name="subject">The Expression being built</param>
        /// <returns></returns>
        private Expression<Func<T, double>> ReadNode(XmlDictionaryReader rdr, ParameterExpression subject)
        {
            switch (rdr.Name)
            {
                case "plus":
                     rdr.Read();
                     BinaryExpression andlambda1 = Expression.Add(Expression.Invoke(ReadNode(rdr, subject), subject),Expression.Invoke(ReadNode(rdr, subject), subject));
                     rdr.ReadEndElement();
                     return Expression.Lambda<Func<T, double>>(andlambda1, subject);

                case "minus":
                    rdr.Read();
                    BinaryExpression minuslambda1 = Expression.Subtract(Expression.Invoke(ReadNode(rdr, subject), subject),Expression.Invoke(ReadNode(rdr, subject), subject));
                    rdr.ReadEndElement();
                    return Expression.Lambda<Func<T, double>>(minuslambda1, subject);

                case "multiply":
                    rdr.Read();
                    BinaryExpression mullambda1 = Expression.Multiply(Expression.Invoke(ReadNode(rdr, subject), subject), Expression.Invoke(ReadNode(rdr, subject), subject));
                    rdr.ReadEndElement();
                    return Expression.Lambda<Func<T, double>>(mullambda1, subject);

                case "divide":
                    rdr.Read();
                    BinaryExpression divlambda1 = Expression.Divide(Expression.Invoke(ReadNode(rdr, subject), subject), Expression.Invoke(ReadNode(rdr, subject), subject));
                    rdr.ReadEndElement();
                    return Expression.Lambda<Func<T, double>>(divlambda1, subject);

                case "power":
                    rdr.Read();
                    BinaryExpression powerlambda1 = Expression.Power(Expression.Invoke(ReadNode(rdr, subject), subject), Expression.Invoke(ReadNode(rdr, subject), subject));
                    rdr.ReadEndElement();
                    return Expression.Lambda<Func<T, double>>(powerlambda1, subject);

                case "value":
                    var m = new MathValue(rdr.GetAttribute("item"));
                    rdr.Read();
                    return new SimpleMathEngine().GetExpression<T>(m);

                default:
                    return Zero;
            }
        }
    }
}

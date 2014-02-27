using System;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
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
        private static readonly Expression<Func<T, double>> Zero = def => 0;
        private static readonly Expression<Func<double, double>> Cos = x => Math.Cos(x);                                    // Cosine of the specified angle
        private static readonly Expression<Func<double, double>> Sin = x => Math.Sin(x);                                    // Sine of the specified angle
        private static readonly Expression<Func<double, double>> Tan = x => Math.Tan(x);                                    // Tangent of the specified angle
        private static readonly Expression<Func<double, double>> Cosh = x => Math.Cosh(x);                                  // Hyperbolic Cosine of the specified angle
        private static readonly Expression<Func<double, double>> Sinh = x => Math.Sinh(x);                                  // Hyperbolic Sine of the specified angle
        private static readonly Expression<Func<double, double>> Tanh = x => Math.Tanh(x);                                  // Hyperbolic Tangent of the specified angle
        private static readonly Expression<Func<double, double>> Exp = x => Math.Exp(x);                                    // Exponentional function
        private static readonly Expression<Func<double, double>> Sqrt = x => Math.Sqrt(x);                                  // Square Root
        private static readonly Expression<Func<double, double>> Asinh = x => Math.Log(x + Math.Sqrt(x * x + 1));           // inverse hyperbolic sine
        private static readonly Expression<Func<double, double>> Acosh = x => Math.Log( x + Math.Sqrt(x*x-1) ) ;            // inverse hyperbolic cosine
        private static readonly Expression<Func<double, double>> Atanh = x => 0.5*( Math.Log( 1 + x ) - Math.Log( 1 -x ) ) ;// inverse hyperbolic tangant
        private static readonly Expression<Func<double, double>> Abs = x => Math.Abs(x);                                    // absolute value
        private static readonly Expression<Func<double, double>> Log = x => Math.Log(x);                                    // logarithm base of a number
        private static readonly Expression<Func<double, double>> Log10 = x => Math.Log10(x);                                // base ten logarithm of a number
        private static readonly Expression<Func<double, double, double>> Max = (x,y) => Math.Max(x,y);                      // gets the Max of two operands as doubles
        private static readonly Expression<Func<double, double, double>> Min = (x, y) => Math.Min(x, y);                    // gets the min of two operands as doubles
        private static readonly Expression<Func<double, double>> Round = x => Math.Round(x);                                // rounds to an integer
        private static readonly Expression<Func<double, double>> Cieling = x => Math.Ceiling(x);                            // gets the Cieling
        private static readonly Expression<Func<double, double>> Floor = x => Math.Floor(x);                                // gets the Floor

        
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

                // ## functionas that accept two operands (left and right)
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

                case "max":
                    rdr.Read();
                    var maxexp = Expression.Invoke(Max, Expression.Invoke(ReadNode(rdr, subject), subject), Expression.Invoke(ReadNode(rdr, subject), subject));
                    rdr.ReadEndElement();
                    return Expression.Lambda<Func<T, double>>(maxexp, subject);

                case "min":
                    rdr.Read();
                    var minexp = Expression.Invoke(Min, Expression.Invoke(ReadNode(rdr, subject), subject), Expression.Invoke(ReadNode(rdr, subject), subject));
                    rdr.ReadEndElement();
                    return Expression.Lambda<Func<T, double>>(minexp, subject);


                // ## funcitons that accept one operand
                case "sqrt":
                    rdr.Read();
                    var sqrtexp = Expression.Invoke(Sqrt,Expression.Invoke(ReadNode(rdr, subject), subject));
                    rdr.ReadEndElement();
                    return Expression.Lambda<Func<T, double>>(sqrtexp, subject);
   
                case "exp":
                    rdr.Read();
                    var expexp = Expression.Invoke(Exp, Expression.Invoke(ReadNode(rdr, subject), subject));
                    rdr.ReadEndElement();
                    return Expression.Lambda<Func<T, double>>(expexp, subject);

                case "cos":
                    rdr.Read();
                    var exp = Expression.Invoke(Cos, Expression.Invoke(ReadNode(rdr, subject), subject));
                    rdr.ReadEndElement();
                    return Expression.Lambda<Func<T, double>>(exp, subject);

                case "sin":
                    rdr.Read();
                    var sinexp = Expression.Invoke(Sin, Expression.Invoke(ReadNode(rdr, subject), subject));
                    rdr.ReadEndElement();
                    return Expression.Lambda<Func<T, double>>(sinexp, subject);
                
                case "tan":
                    rdr.Read();
                    var tanexp = Expression.Invoke(Tan, Expression.Invoke(ReadNode(rdr, subject), subject));
                    rdr.ReadEndElement();
                    return Expression.Lambda<Func<T, double>>(tanexp, subject);

                case "cosh":
                    rdr.Read();
                    var coshexp = Expression.Invoke(Cosh, Expression.Invoke(ReadNode(rdr, subject), subject));
                    rdr.ReadEndElement();
                    return Expression.Lambda<Func<T, double>>(coshexp, subject);

                case "sinh":
                    rdr.Read();
                    var sinhexp = Expression.Invoke(Sinh, Expression.Invoke(ReadNode(rdr, subject), subject));
                    rdr.ReadEndElement();
                    return Expression.Lambda<Func<T, double>>(sinhexp, subject);

                case "tanh":
                    rdr.Read();
                    var tanhexp = Expression.Invoke(Tanh, Expression.Invoke(ReadNode(rdr, subject), subject));
                    rdr.ReadEndElement();
                    return Expression.Lambda<Func<T, double>>(tanhexp, subject);

                case "acosh":
                    rdr.Read();
                    var acoshexp = Expression.Invoke(Acosh, Expression.Invoke(ReadNode(rdr, subject), subject));
                    rdr.ReadEndElement();
                    return Expression.Lambda<Func<T, double>>(acoshexp, subject);

                case "asinh":
                    rdr.Read();
                    var asinhexp = Expression.Invoke(Asinh, Expression.Invoke(ReadNode(rdr, subject), subject));
                    rdr.ReadEndElement();
                    return Expression.Lambda<Func<T, double>>(asinhexp, subject);

                case "atanh":
                    rdr.Read();
                    var atanhexp = Expression.Invoke(Atanh, Expression.Invoke(ReadNode(rdr, subject), subject));
                    rdr.ReadEndElement();
                    return Expression.Lambda<Func<T, double>>(atanhexp, subject);

                case "abs":
                    rdr.Read();
                    var absexp = Expression.Invoke(Abs, Expression.Invoke(ReadNode(rdr, subject), subject));
                    rdr.ReadEndElement();
                    return Expression.Lambda<Func<T, double>>(absexp, subject);

                case "log":
                    rdr.Read();
                    var logexp = Expression.Invoke(Log, Expression.Invoke(ReadNode(rdr, subject), subject));
                    rdr.ReadEndElement();
                    return Expression.Lambda<Func<T, double>>(logexp, subject);

                case "log10":
                    rdr.Read();
                    var log10exp = Expression.Invoke(Log10, Expression.Invoke(ReadNode(rdr, subject), subject));
                    rdr.ReadEndElement();
                    return Expression.Lambda<Func<T, double>>(log10exp, subject);


                case "round":
                    rdr.Read();
                    var roundexp = Expression.Invoke(Round, Expression.Invoke(ReadNode(rdr, subject), subject));
                    rdr.ReadEndElement();
                    return Expression.Lambda<Func<T, double>>(roundexp, subject);

                case "cieling":
                    rdr.Read();
                    var celexp = Expression.Invoke(Cieling, Expression.Invoke(ReadNode(rdr, subject), subject));
                    rdr.ReadEndElement();
                    return Expression.Lambda<Func<T, double>>(celexp, subject);
                
                case "floor":
                    rdr.Read();
                    var floorexp = Expression.Invoke(Floor, Expression.Invoke(ReadNode(rdr, subject), subject));
                    rdr.ReadEndElement();
                    return Expression.Lambda<Func<T, double>>(floorexp, subject);


                // ## a value
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

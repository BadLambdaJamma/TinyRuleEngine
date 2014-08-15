using System;
using System.Linq.Expressions;
using System.Xml;
using TinyRuleEngine.Engines;
using MathNet;

namespace TinyRuleEngine.Readers
{
    /// <summary>
    /// Simple Math rule reader
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SimpleMathExpressionReader<T>
    {
        private readonly Expression<Func<T, double>> _zero = def => 0;
        private readonly Expression<Func<double, double>> _cos = x => Math.Cos(x);                                                                          // Cosine of the specified angle
        private readonly Expression<Func<double, double>> _sin = x => Math.Sin(x);                                                                          // Sine of the specified angle
        private readonly Expression<Func<double, double>> _tan = x => Math.Tan(x);                                                                          // Tangent of the specified angle
        private readonly Expression<Func<double, double>> _cosh = x => Math.Cosh(x);                                                                        // Hyperbolic Cosine of the specified angle
        private readonly Expression<Func<double, double>> _sinh = x => Math.Sinh(x);                                                                        // Hyperbolic Sine of the specified angle
        private readonly Expression<Func<double, double>> _tanh = x => Math.Tanh(x);                                                                        // Hyperbolic Tangent of the specified angle
        private readonly Expression<Func<double, double>> _exp = x => Math.Exp(x);                                                                          // Exponentional function
        private readonly Expression<Func<double, double>> _sqrt = x => Math.Sqrt(x);                                                                        // Square Root
        private readonly Expression<Func<double, double>> _asinh = x => Math.Log(x + Math.Sqrt(x * x + 1));                                                 // inverse hyperbolic sine
        private readonly Expression<Func<double, double>> _acosh = x => Math.Log( x + Math.Sqrt(x*x-1) ) ;                                                  // inverse hyperbolic cosine
        private readonly Expression<Func<double, double>> _atanh = x => 0.5*( Math.Log( 1 + x ) - Math.Log( 1 -x ) ) ;                                      // inverse hyperbolic tangant
        private readonly Expression<Func<double, double>> _abs = x => Math.Abs(x);                                                                          // absolute value
        private readonly Expression<Func<double, double>> _log = x => Math.Log(x);                                                                          // logarithm base of a number
        private readonly Expression<Func<double, double>> _log10 = x => Math.Log10(x);                                                                      // base ten logarithm of a number
        private readonly Expression<Func<double, double, double>> _max = (x,y) => Math.Max(x,y);                                                            // gets the Max of two operands as doubles
        private readonly Expression<Func<double, double, double>> _min = (x, y) => Math.Min(x, y);                                                          // gets the min of two operands as doubles
        private readonly Expression<Func<double, double>> _round = x => Math.Round(x);                                                                      // rounds to an integer
        private readonly Expression<Func<double, double>> _cieling = x => Math.Ceiling(x);                                                                  // gets the Cieling
        private readonly Expression<Func<double, double>> _floor = x => Math.Floor(x);                                                                      // gets the Floor
        
        private readonly Expression<Func<double, double>> _besselI0 = x => MathNet.Numerics.SpecialFunctions.BesselI0(x);
        private readonly Expression<Func<double, double>> _besselI0MStruveL0 = x => MathNet.Numerics.SpecialFunctions.BesselI0MStruveL0(x);
        private readonly Expression<Func<double, double>> _struveL1 = x => MathNet.Numerics.SpecialFunctions.StruveL1(x);
        private readonly Expression<Func<double, double>> _struveL0 = x => MathNet.Numerics.SpecialFunctions.StruveL0(x);
        private readonly Expression<Func<double, double>> _logit = x => MathNet.Numerics.SpecialFunctions.Logit(x);
        private readonly Expression<Func<double, double>> _logistic= x => MathNet.Numerics.SpecialFunctions.Logistic(x);
        private readonly Expression<Func<double, double>> _harmonic = x => MathNet.Numerics.SpecialFunctions.Harmonic(Convert.ToInt32(x));
        private readonly Expression<Func<double, double>> _gammaLN = x => MathNet.Numerics.SpecialFunctions.GammaLn(x);
        private readonly Expression<Func<double, double>> _gamma = x => MathNet.Numerics.SpecialFunctions.Gamma(x);
        private readonly Expression<Func<double, double>> _exponentialMinusOne = x => MathNet.Numerics.SpecialFunctions.ExponentialMinusOne(x);

        /// <summary>
        /// Read the rule and returns Expression of Func of T
        /// </summary>
        /// <param name="rdr">The rule reader</param>
        /// <returns></returns>
        public Expression<Func<T, double>> ReadRule(XmlDictionaryReader rdr)
        {
            //var math = MathNet.Numerics.LinearAlgebra.Complex.Factorization.GramSchmidt()
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
                    var maxexp = Expression.Invoke(_max, Expression.Invoke(ReadNode(rdr, subject), subject), Expression.Invoke(ReadNode(rdr, subject), subject));
                    rdr.ReadEndElement();
                    return Expression.Lambda<Func<T, double>>(maxexp, subject);

                case "min":
                    rdr.Read();
                    var minexp = Expression.Invoke(_min, Expression.Invoke(ReadNode(rdr, subject), subject), Expression.Invoke(ReadNode(rdr, subject), subject));
                    rdr.ReadEndElement();
                    return Expression.Lambda<Func<T, double>>(minexp, subject);


                // ## funcitons that accept one operand
                case "sqrt":
                    rdr.Read();
                    var sqrtexp = Expression.Invoke(_sqrt,Expression.Invoke(ReadNode(rdr, subject), subject));
                    rdr.ReadEndElement();
                    return Expression.Lambda<Func<T, double>>(sqrtexp, subject);
   
                case "exp":
                    rdr.Read();
                    var expexp = Expression.Invoke(_exp, Expression.Invoke(ReadNode(rdr, subject), subject));
                    rdr.ReadEndElement();
                    return Expression.Lambda<Func<T, double>>(expexp, subject);

                case "cos":
                    rdr.Read();
                    var exp = Expression.Invoke(_cos, Expression.Invoke(ReadNode(rdr, subject), subject));
                    rdr.ReadEndElement();
                    return Expression.Lambda<Func<T, double>>(exp, subject);

                case "sin":
                    rdr.Read();
                    var sinexp = Expression.Invoke(_sin, Expression.Invoke(ReadNode(rdr, subject), subject));
                    rdr.ReadEndElement();
                    return Expression.Lambda<Func<T, double>>(sinexp, subject);
                
                case "tan":
                    rdr.Read();
                    var tanexp = Expression.Invoke(_tan, Expression.Invoke(ReadNode(rdr, subject), subject));
                    rdr.ReadEndElement();
                    return Expression.Lambda<Func<T, double>>(tanexp, subject);

                case "cosh":
                    rdr.Read();
                    var coshexp = Expression.Invoke(_cosh, Expression.Invoke(ReadNode(rdr, subject), subject));
                    rdr.ReadEndElement();
                    return Expression.Lambda<Func<T, double>>(coshexp, subject);

                case "sinh":
                    rdr.Read();
                    var sinhexp = Expression.Invoke(_sinh, Expression.Invoke(ReadNode(rdr, subject), subject));
                    rdr.ReadEndElement();
                    return Expression.Lambda<Func<T, double>>(sinhexp, subject);

                case "tanh":
                    rdr.Read();
                    var tanhexp = Expression.Invoke(_tanh, Expression.Invoke(ReadNode(rdr, subject), subject));
                    rdr.ReadEndElement();
                    return Expression.Lambda<Func<T, double>>(tanhexp, subject);

                case "acosh":
                    rdr.Read();
                    var acoshexp = Expression.Invoke(_acosh, Expression.Invoke(ReadNode(rdr, subject), subject));
                    rdr.ReadEndElement();
                    return Expression.Lambda<Func<T, double>>(acoshexp, subject);

                case "asinh":
                    rdr.Read();
                    var asinhexp = Expression.Invoke(_asinh, Expression.Invoke(ReadNode(rdr, subject), subject));
                    rdr.ReadEndElement();
                    return Expression.Lambda<Func<T, double>>(asinhexp, subject);

                case "atanh":
                    rdr.Read();
                    var atanhexp = Expression.Invoke(_atanh, Expression.Invoke(ReadNode(rdr, subject), subject));
                    rdr.ReadEndElement();
                    return Expression.Lambda<Func<T, double>>(atanhexp, subject);

                case "abs":
                    rdr.Read();
                    var absexp = Expression.Invoke(_abs, Expression.Invoke(ReadNode(rdr, subject), subject));
                    rdr.ReadEndElement();
                    return Expression.Lambda<Func<T, double>>(absexp, subject);

                case "log":
                    rdr.Read();
                    var logexp = Expression.Invoke(_log, Expression.Invoke(ReadNode(rdr, subject), subject));
                    rdr.ReadEndElement();
                    return Expression.Lambda<Func<T, double>>(logexp, subject);

                case "log10":
                    rdr.Read();
                    var log10Exp = Expression.Invoke(_log10, Expression.Invoke(ReadNode(rdr, subject), subject));
                    rdr.ReadEndElement();
                    return Expression.Lambda<Func<T, double>>(log10Exp, subject);


                case "round":
                    rdr.Read();
                    var roundexp = Expression.Invoke(_round, Expression.Invoke(ReadNode(rdr, subject), subject));
                    rdr.ReadEndElement();
                    return Expression.Lambda<Func<T, double>>(roundexp, subject);

                case "cieling":
                    rdr.Read();
                    var celexp = Expression.Invoke(_cieling, Expression.Invoke(ReadNode(rdr, subject), subject));
                    rdr.ReadEndElement();
                    return Expression.Lambda<Func<T, double>>(celexp, subject);
                
                case "floor":
                    rdr.Read();
                    var floorexp = Expression.Invoke(_floor, Expression.Invoke(ReadNode(rdr, subject), subject));
                    rdr.ReadEndElement();
                    return Expression.Lambda<Func<T, double>>(floorexp, subject);

                case "besselI0":
                    rdr.Read();
                    var besselI0exp = Expression.Invoke(_besselI0, Expression.Invoke(ReadNode(rdr, subject), subject));
                    rdr.ReadEndElement();
                    return Expression.Lambda<Func<T, double>>(besselI0exp, subject);

                case "besselI0MStruveL0":
                    rdr.Read();
                    var besselI0MStruveL0 = Expression.Invoke(_besselI0MStruveL0, Expression.Invoke(ReadNode(rdr, subject), subject));
                    rdr.ReadEndElement();
                    return Expression.Lambda<Func<T, double>>(besselI0MStruveL0, subject);
               
                case "struveL1":
                    rdr.Read();
                    var struveL1 = Expression.Invoke(_struveL1, Expression.Invoke(ReadNode(rdr, subject), subject));
                    rdr.ReadEndElement();
                    return Expression.Lambda<Func<T, double>>(struveL1, subject);

                case "struveL0":
                    rdr.Read();
                    var struveL0 = Expression.Invoke(_struveL0, Expression.Invoke(ReadNode(rdr, subject), subject));
                    rdr.ReadEndElement();
                    return Expression.Lambda<Func<T, double>>(struveL0, subject);
                
                case "logit":
                    rdr.Read();
                    var logit = Expression.Invoke(_logit, Expression.Invoke(ReadNode(rdr, subject), subject));
                    rdr.ReadEndElement();
                    return Expression.Lambda<Func<T, double>>(logit, subject);

                case "logistic":
                    rdr.Read();
                    var logistic = Expression.Invoke(_logistic, Expression.Invoke(ReadNode(rdr, subject), subject));
                    rdr.ReadEndElement();
                    return Expression.Lambda<Func<T, double>>(logistic, subject);

                case "harmonic":
                    rdr.Read();
                    var harmonic = Expression.Invoke(_harmonic, Expression.Invoke(ReadNode(rdr, subject), subject));
                    rdr.ReadEndElement();
                    return Expression.Lambda<Func<T, double>>(harmonic, subject);

                case "gammaLN":
                    rdr.Read();
                    var gammaLN = Expression.Invoke(_gammaLN, Expression.Invoke(ReadNode(rdr, subject), subject));
                    rdr.ReadEndElement();
                    return Expression.Lambda<Func<T, double>>(gammaLN, subject);

                case "gamma":
                    rdr.Read();
                    var gamma = Expression.Invoke(_gamma, Expression.Invoke(ReadNode(rdr, subject), subject));
                    rdr.ReadEndElement();
                    return Expression.Lambda<Func<T, double>>(gamma, subject);

                case "exponentialMinusOne":
                    rdr.Read();
                    var exponentialMinusOne = Expression.Invoke(_exponentialMinusOne, Expression.Invoke(ReadNode(rdr, subject), subject));
                    rdr.ReadEndElement();
                    return Expression.Lambda<Func<T, double>>(exponentialMinusOne, subject);

                // ## a value
                case "value":
                    var m = new MathValue(rdr.GetAttribute("item"));
                    rdr.Read();
                    return new SimpleMathEngine().GetExpression<T>(m);

                default:
                    return _zero;
            }
        }
    }
}

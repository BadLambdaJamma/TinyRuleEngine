
namespace TinyRuleEngine.Engines
{
    /// <summary>
    /// defines a rule used by all engines and readers
    /// </summary>
    public class Rule
    {
        public string MemberName { get; set; }
        public string TargetValue { get; set; }
        public string Operator { get; set; }
        public string Uses { get; set; }

        public Rule(string memberName, string targetValue, string @operator)
        {
            MemberName = memberName; TargetValue = targetValue; Operator = @operator; Uses="";
        }
        public Rule(string memberName, string targetValue, string @operator, string uses)
        {
            MemberName = memberName; TargetValue = targetValue; Operator = @operator; Uses=uses;
        }
    }
}
    
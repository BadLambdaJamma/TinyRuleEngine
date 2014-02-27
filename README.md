TinyRuleEngine - The simple math engine has been added!
========================================================


With the simplemathengine, most any mathmatical equation can be dynamically built, converted to an
abstract syntax tree, compiled, and evaluated,   NO REFLECTION REQUIRED!

Simple Math Expression for the Resonant Freuqncy of a "Tank" Circuit:

<mathexp name="ResonantFrequencyOfATankCircuit" appliesto="TinyRuleEngineTest.CircuitDTO">
    <round>
      <divide>
        <value item="@@1" />
        <multiply>
          <value item="@@2" />
          <multiply>
            <value item="@Pi"/>
            <sqrt>
              <multiply>
                <value item="InductanceInHenries" />
                <value item="CapacitanceInFarads" />
              </multiply>
            </sqrt>
          </multiply>
        </multiply>
      </divide>
    </round>
  </mathexp>



Version .9 Beta release

This is a minimal feature rule engine based on speed and since.


A simple C# rules engine for .NET using Linq Expressions, Lambdas, and Delegates.  small and fast.

1.  Supports a Code first model, or an all Code approach, to building, compiling and executing rules as discrete steps
2.  Supports XML Driven rules with deep logical nesting
3.  Supports Mix of Code and XML rules with logical operators and fluent syntax


Supported Engines:

1. RuleEngine - A Simple rule engine that evaluates rules against a given type (DTO type)
2. IdentityRuleEngine - Included support for making IClaimsPrincipal claim checks mixed with DTO rules.
3. TuppleRuleEngine - Supports Rules compiled against two DTO types (e.g. Car and SalesPerson )
4. IdentityTuppleRuleEngine - Support two DTO types (.e.g. car and Saleperson) +  IClaimsPrincipal

====================================================================================================
Licensed as Open Source software under the Apache License.
Please contact me with bugs, improvements or bitcoins - jonathananewell@hotmail.com

Rule reader uses Predicate Builder to allow you to Join rule expressions in Code.

===================================================================================================   
Each type of rule engine is illistrated with unit tests:

1. RuleEngine - A rule engine that evaluates against a single type defined by the developer or rule writer
2. IdentityRuleEngine - A rule engine that supports claims principal checks mixed in with a generic type
3. TuppleRuleEngine - A rule engine that evaulates against two types defined by the developer or rule writer
4. IdentityTuppleRuleEngine - A Rule Engine that evaluates against two types and supports claims pincipal claim checks

Here are the main features:

1. Pure code model supported:  define, compile and execute rules in code. 
2. XML based model: Load complex rule graphs with deep cyclomatic logic from simple Xml files.
3. Hybrid model:  mix-in your code and XML rules anyway you want with any logical operators. 
4. Pluggable rule readers.  Define your own rule language or variation without impacting consuming code.
5. Performant - Uses .NET expression trees to create fast executeable code on the fly.


====================================================================================================
Supported join operators:
====================================================================================================
and
or
xor

===================================================================================================
Supported comparison Operators
===================================================================================================
string properties:

Equals
Startswith
EndsWith
Contains

Numeric properties:
Equals
Lessthan
GreaterThan
LessthanOrEqual
GreaterthanOrEqual

======================================================================================================
Simple rule: 1 rule shown
======================================================================================================
<rules>
	<rule name="IsApprover" appliesto="UnitTestProject1.User">
		<ruleitem membername="Age" operator="Equals" targetvalue="22" />
	<rule>
</rules>

======================================================================================================
Complex rule: 2 rules shown
======================================================================================================
<rules>
	<rule name="IsApprover" appliesto="UnitTestProject1.User">
		<and>
		<and>
			<ruleitem membername="Age" operator="Equals" targetvalue="21" />
			<ruleitem membername="Name" operator="StartsWith" targetvalue="on"/>
		</and>
		<and>
			<ruleitem membername="Age" operator="Equals" targetvalue="22" />
			<ruleitem membername="Name" operator="StartsWith" targetvalue="Jon"/>
		</and>
		</and>
	</rule>
	<rule name="IsApproverSuperUser" appliesto="UnitTestProject1.User">
		<ruleitem membername="Age" operator="Equals" targetvalue="22" />
	</rule>
</rules>

======================================================================================================
Complex rule: 3 -Identity rules shown with a group SID claim (any claim including role may be used)
member name is set to the special value of @user
operator is the claimType
TargetValue is the claim value.

Claims on the Principal are processed with this lambda:

   Expression<Func<IClaimsPrincipal,string, string, bool>> HasClaimTest 
	= (p, ct, cv) => p.Identities.Any(s => s.Claims.Any(c => c.ClaimType == ct &&  c.ValueType == ClaimValueTypes.String && c.Value == cv));

	p = the claim principal
	ct = the claim type
	cv = the claim value

======================================================================================================
<rules>
  <rule name="IsApprover" appliesto="TinyRuleEngineTest.User">
    <or>
      <and>
        <ruleitem membername="@User" operator="http://schemas.microsoft.com/ws/2008/06/identity/claims/groupsid" targetvalue="S-1-5-21-2493390151-660934664-2262481224-513" />
        <ruleitem membername="Name" operator="StartsWith" targetvalue="Jon"/>
      </and>
      <and>
          <ruleitem membername="@User" operator="http://schemas.microsoft.com/ws/2008/06/identity/claims/groupsid" targetvalue="S-1-5-21-2493390151-660934664-2262481224-513" />
          <ruleitem membername="Name" operator="StartsWith" targetvalue="on"/>
      </and>
    </or>
  </rule>
</rules>

=======================================================================================================
FAQuestions
=======================================================================================================
1. How come rule builder does not just call compile for me?

    Rule builder assumes a few things when it comes to calling compile:
	
	a. The developer knows best when to call compile.  Compiled rules should usually be backed by
	   a lazy loaded cache to avoid calling compile.
	
	b. Compile will sometimes invoke reflection, sometimes not.  For any operator but 'Equals' on a string comparison,
	   reflection will be used to build the expression.  None of the numeric operators invoke reflection when compile 
	   is called.  None of the claims based rules invoke reflection.

	c. mix-ins are only suppoorted against expressions not compiled rules.  you can use mix-ins all you want and then 
	   you call compile on the newly composed rule.

2. Your unit test foo seems rather week. 
	a. More unit test are coming. 

3. Future Plans?
	a. Some more - More unit tests
	b. More checks against 'Built in' Types like IClaimsPrincipal.
	c. Support rule "mix-in" at the XML level instead of just code.
	D. support a rule paradym with multiple generic<T>



Raw rule engine base performance:
1. rule engine predicate test  - execute this test 10K times in 5.550 seconds.  the test consists of:
   a. defining a DTO
   b. defining 4 rules, 
   c. call the rule engine to get the expressions from the defined rules.  
   d. Using fluent syntax to join the expressions together
   e. Compiling the rule
 


 





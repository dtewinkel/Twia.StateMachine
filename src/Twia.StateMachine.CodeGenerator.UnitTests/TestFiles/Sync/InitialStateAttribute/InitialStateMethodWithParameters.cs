---
Name: 'SMG0009 - InitialState Method With Parameters'
Output: Source
Diagnostics:
- SMG0009, 9, 26, "State1"
---

#pragma warning disable CS1591

namespace Twia.StateMachine.CodeGenerator.UnitTests;

[StateMachine]
public partial class UnitTestStateMachine
{
    [InitialState]
    private partial void State1(string name);
}

---
Name: 'SMG0009 - State Method With Parameters'
Output: Source
Diagnostics:
- SMG0009, 12, 26, "State2"
---

#pragma warning disable CS1591

namespace Twia.StateMachine.CodeGenerator.UnitTests;

[StateMachine]
public partial class UnitTestStateMachine
{
    [InitialState]
    private partial void State1();

    [State]
    private partial void State2(string name);
}

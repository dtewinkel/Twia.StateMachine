---
Name: 'SMG0009 - Trigger Method With Parameters'
Output: Source
Diagnostics:
- SMG0009, 12, 26, "Trigger1"
---

#pragma warning disable CS1591

namespace Twia.StateMachine.CodeGenerator.UnitTests;

[StateMachine]
public partial class UnitTestStateMachine
{
    [InitialState]
    private partial void State1();

    [Trigger]
    private partial void Trigger1(string name);
}

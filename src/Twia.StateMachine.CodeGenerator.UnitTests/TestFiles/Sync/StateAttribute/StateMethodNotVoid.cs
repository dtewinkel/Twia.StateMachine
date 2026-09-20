---
Name: 'SMG0008 - State Method Not Void'
Output: Source
Diagnostics:
- SMG0008, 12, 26, "State2"
---

#pragma warning disable CS1591

namespace Twia.StateMachine.CodeGenerator.UnitTests;

[StateMachine]
public partial class UnitTestStateMachine
{
    [InitialState]
    private partial void State1();

    [State]
    private partial bool State2();
}

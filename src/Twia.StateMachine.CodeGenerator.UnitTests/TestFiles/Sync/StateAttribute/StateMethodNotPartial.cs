---
Name: 'SMG0007 - State Method Not Partial'
Output: Source
Diagnostics:
- SMG0007, 12, 18, "State2"
---

#pragma warning disable CS1591

namespace Twia.StateMachine.CodeGenerator.UnitTests;

[StateMachine]
public partial class UnitTestStateMachine
{
    [InitialState]
    private partial void State1();

    [State]
    private void State2()
    {
    }
}

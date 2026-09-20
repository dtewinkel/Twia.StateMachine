---
Name: 'SMG0007 - Trigger Method Not Partial'
Output: Source
Diagnostics:
- SMG0007, 12, 18, "Trigger1"
---

#pragma warning disable CS1591

namespace Twia.StateMachine.CodeGenerator.UnitTests;

[StateMachine]
public partial class UnitTestStateMachine
{
    [InitialState]
    private partial void State1();

    [Trigger]
    private void Trigger1()
    {
    }
}

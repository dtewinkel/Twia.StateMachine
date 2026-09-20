---
Name: 'SMG0008 - Trigger Method Not Void'
Output: Source
Diagnostics:
- SMG0008, 12, 26, "Trigger1"
---

#pragma warning disable CS1591

namespace Twia.StateMachine.CodeGenerator.UnitTests;

[StateMachine]
public partial class UnitTestStateMachine
{
    [InitialState]
    private partial void State1();

    [Trigger]
    private partial bool Trigger1();
}

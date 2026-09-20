---
Name: 'SMG0002 - Multiple InitialStates'
Output: Source
Diagnostics:
- SMG0002, 12, 26, "State2", "State1"
---

#pragma warning disable CS1591

namespace Twia.StateMachine.CodeGenerator.UnitTests;

[StateMachine]
public partial class UnitTestStateMachine
{
    [InitialState]
    private partial void State1();

    [InitialState]
    private partial void State2();
}

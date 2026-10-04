---
Name: 'SMG0010 - Unknown trigger name in InternalTransitionAttribute'
Output: Source
Diagnostics:
- SMG0010, 10, 26, Trigger1, State1
---

#pragma warning disable CS1591

namespace Twia.StateMachine.CodeGenerator.UnitTests;

[StateMachine]
public partial class UnitTestStateMachine
{
    [InitialState]
    [InternalTransition("Trigger1", "nothing")]
    private partial void State1();
}

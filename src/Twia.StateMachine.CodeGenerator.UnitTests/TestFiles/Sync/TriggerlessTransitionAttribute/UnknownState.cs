---
Name: 'SMG0011 - Unknown state name in TriggerlessTransitionAttribute'
Output: Source
Diagnostics:
- SMG0011, 10, 26, UnknownState, State1
---

#pragma warning disable CS1591

namespace Twia.StateMachine.CodeGenerator.UnitTests;

[StateMachine]
public partial class UnitTestStateMachine
{
    [InitialState]
    [TriggerlessTransition("UnknownState")]
    private partial void State1();
}

---
Name: 'SMG0011 - Unknown state name in TransitionAttribute'
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
    [Transition(nameof(Trigger1), "UnknownState")]
    private partial void State1();

    [Trigger]
    public partial void Trigger1();
}

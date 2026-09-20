---
Name: 'SMG0004 - TransitionAfterAttribute on a trigger'
Output: Source
Diagnostics:
- SMG0006, 13, 25, Trigger1
---

#pragma warning disable CS1591

namespace Twia.StateMachine.CodeGenerator.UnitTests;

[StateMachine]
public partial class UnitTestStateMachine
{
    [InitialState]
    private partial void State1();

    [Trigger]
    [TransitionAfter("PT1H", nameof(State1))]
    public partial void Trigger1();

}

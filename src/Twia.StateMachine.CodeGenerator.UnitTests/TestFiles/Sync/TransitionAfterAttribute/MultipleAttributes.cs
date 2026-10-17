---
Name: 'Multiple TransitionAfterAttribute'
Output: Source
---

#pragma warning disable CS1591

namespace Twia.StateMachine.CodeGenerator.UnitTests;

[StateMachine]
public partial class UnitTestStateMachine
{
    [InitialState]
    [TransitionAfter("00:01:01", nameof(State2), Condition = nameof(CanTrigger))]
    [TransitionAfter("00:01:03", nameof(State1))]
    private partial void State1();

    [State]
    [TransitionAfter("00:01:02", nameof(State1))]
    private partial void State2();

    public bool CanTrigger => true;
}

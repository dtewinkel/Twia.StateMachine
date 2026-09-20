---
Name: StateMachine with observability disabled, and state not accessible
Output: Source
---

#pragma warning disable CS1591

namespace Twia.StateMachine.CodeGenerator.UnitTests;

[StateMachine(Observable = false, StateAccessible = false)]
internal partial class UnitTestStateMachine
{
    public bool CanTransition { get; set; } = true;

    [Trigger]
    public partial void ButtonPressed();

    [State, InitialState]
    [Transition(nameof(ButtonPressed), nameof(On), Condition = "CanTransition == true", Action = "CanTransition = false")]
    internal partial void Off();

    [State]
    partial void On();
}
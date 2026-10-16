---
Name: 'Multiple TriggerlessTransitionAttributes'
Output: Source
---

#pragma warning disable CS1591

namespace Twia.StateMachine.CodeGenerator.UnitTests;

[StateMachine]
public partial class UnitTestStateMachine
{
    [InitialState]
    [TriggerlessTransition(nameof(State2))]
    private partial void State1();

    [State]
    [TriggerlessTransition(nameof(State3))]
    private partial void State2();

    [State]
    private partial void State3();
}

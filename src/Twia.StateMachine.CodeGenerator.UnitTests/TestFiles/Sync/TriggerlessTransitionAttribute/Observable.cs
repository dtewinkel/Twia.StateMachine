---
Name: 'Observable TriggerlessTransitionAttribute'
Output: Source
---

#pragma warning disable CS1591

namespace Twia.StateMachine.CodeGenerator.UnitTests;

[StateMachine(Observable = true)]
public partial class UnitTestStateMachine
{
    [InitialState]
    [TriggerlessTransition(nameof(State2))]
    private partial void State1();

    [State]
    private partial void State2();
}

---
Name: 'Condition and Action on TriggerlessTransitionAttribute'
Output: Source
---

#pragma warning disable CS1591

namespace Twia.StateMachine.CodeGenerator.UnitTests;

[StateMachine]
public partial class UnitTestStateMachine
{
    [InitialState]
    [TriggerlessTransition(nameof(State2), Condition = $"{nameof(HasCondition)}()", Action = $"{nameof(DoAction)}()")]
    private partial void State1();

    [State]
    private partial void State2();

    private bool HasCondition()
    {
        return true;
    }

    private void DoAction()
    {
        // Do something useful here.
    }
}

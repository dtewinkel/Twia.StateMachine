---
Name: 'Condition and Action on TransitionAfterAttribute'
Output: Source
---

#pragma warning disable CS1591

namespace Twia.StateMachine.CodeGenerator.UnitTests;

[StateMachine]
public partial class UnitTestStateMachine
{
    [InitialState]
    [TransitionAfter("00:01:02", nameof(State1), Condition = $"{nameof(HasCondition)}()", Action = $"{nameof(DoAction)}()")]
    private partial void State1();

    private bool HasCondition()
    {
        return true;
    }

    private void DoAction()
    {
        // Do something useful here.
    }
}

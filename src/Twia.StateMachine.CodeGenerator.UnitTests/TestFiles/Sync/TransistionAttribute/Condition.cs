---
Name: 'Condition on TransitionAttribute'
Output: Source
---

#pragma warning disable CS1591

namespace Twia.StateMachine.CodeGenerator.UnitTests;

[StateMachine]
public partial class UnitTestStateMachine
{
    [InitialState]
    [Transition(nameof(Trigger1), nameof(State1), Condition = $"{nameof(HasCondition)}()")]
    private partial void State1();

    [Trigger]
    public partial void Trigger1();

    private bool HasCondition()
    {
        return true;
    }
}

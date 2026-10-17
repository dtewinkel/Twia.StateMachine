---
Name: 'Condition and Action on OnExitAttribute'
Output: Source
---

#pragma warning disable CS1591

namespace Twia.StateMachine.CodeGenerator.UnitTests;

[StateMachine]
public partial class UnitTestStateMachine
{
    [InitialState]
    [OnExit($"{nameof(DoAction)}()", Condition = $"{nameof(HasCondition)}()")]
    [Transition(nameof(Trigger1), nameof(State1))]
    private partial void State1();

    [Trigger]
    public partial void Trigger1();

    private bool HasCondition()
    {
        return true;
    }

    private void DoAction()
    {
        // Do something useful here.
    }
}

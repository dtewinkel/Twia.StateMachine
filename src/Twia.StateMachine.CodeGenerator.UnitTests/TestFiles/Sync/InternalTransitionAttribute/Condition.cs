---
Name: 'Condition on InternalTransitionAttribute'
Output: Source
---

#pragma warning disable CS1591

namespace Twia.StateMachine.CodeGenerator.UnitTests;

[StateMachine]
public partial class UnitTestStateMachine
{
    [InitialState]
    [InternalTransition(nameof(Trigger1), "Work()", Condition = $"{nameof(HasCondition)}()")]
    private partial void State1();

    [Trigger]
    public partial void Trigger1();

    private bool HasCondition()
    {
        return true;
    }

    private void Work()
    {
        // Do nothing
    }
}

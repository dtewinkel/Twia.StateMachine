---
Name: 'Condition and Action on OnEntryAttribute'
Output: Source
---

#pragma warning disable CS1591

namespace Twia.StateMachine.CodeGenerator.UnitTests;

[StateMachine]
public partial class UnitTestStateMachine
{
    [InitialState]
    [OnEntry($"{nameof(DoAction)}()", Condition = $"{nameof(HasCondition)}()")]
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

---
Name: 'Single TransitionAfterAttribute'
Output: Source
---

#pragma warning disable CS1591

namespace Twia.StateMachine.CodeGenerator.UnitTests;

[StateMachine]
public partial class UnitTestStateMachine
{
    [InitialState]
    [TransitionAfter("00:01:02", nameof(State1))]
    private partial void State1();
}

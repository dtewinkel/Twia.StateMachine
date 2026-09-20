---
Name: class without StateMachineAttribute
Output: None
Diagnostics:
- CS8795, 8, 26, "Twia.StateMachine.CodeGenerator.UnitTests.UnitTestStateMachine.State1()"
- CS8795, 11, 26, "Twia.StateMachine.CodeGenerator.UnitTests.UnitTestStateMachine.State2()"
- CS8795, 14, 26, "Twia.StateMachine.CodeGenerator.UnitTests.UnitTestStateMachine.Trigger1()"
---

#pragma warning disable CS1591

namespace Twia.StateMachine.CodeGenerator.UnitTests;

public partial class UnitTestStateMachine
{
    [InitialState]
    private partial void State1();

    [State]
    private partial void State2();

    [Trigger]
    private partial void Trigger1();

}

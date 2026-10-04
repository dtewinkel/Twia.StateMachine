---
Name: With Mix Of Errors
Output: None
Diagnostics:
- SMG0001, 6, 14, "UnitTestStateMachine"
- SMG0003, 6, 14, "UnitTestStateMachine"
- SMG0006, 9, 25, "State1"
- SMG0005, 12, 25, "State2"
- SMG0007, 15, 17, "State3"
---

#pragma warning disable CS1591

namespace Twia.StateMachine.CodeGenerator.UnitTests;

[StateMachine]
public class UnitTestStateMachine
{
    [Trigger, OnEntry("DoNothing()")]
    public partial void State1();

    [OnExit("DoNothing()")]
    public partial void State2();

    [State]
    public void State3();
}

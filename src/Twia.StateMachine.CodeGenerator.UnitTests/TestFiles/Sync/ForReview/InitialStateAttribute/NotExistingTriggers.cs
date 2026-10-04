---
Name: Not existing Triggers
Output: None
Diagnostics:
- SMG0010, 10, 25, "Trigger1", "State1"
- SMG0010, 14, 25, "Trigger2", "State2"
---

#pragma warning disable CS1591

namespace Twia.StateMachine.CodeGenerator.UnitTests;

[StateMachine]
public partial class UnitTestStateMachine
{
    [InitialState]
    [Transition("Trigger1", "State2")]
    public partial void State1();

    [State]
    [InternalTransition("Trigger2", "Work()")]
    public partial void State2();

    public void Work() {}
}

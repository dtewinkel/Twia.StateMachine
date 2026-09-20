---
Name: Not existing Triggers
Output: None
Diagnostics:
- SMG0010, 8, 25, "Trigger1", "State1"
- SMG0010, 12, 25, "Trigger2", "State2"
---

using Twia.StateMachine;

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

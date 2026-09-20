---
Name: Invalid Period
Output: None
Diagnostics:
- SMG0012, 10, 25, "a", "State1"
- SMG0012, 15, 25, "T1H", "State2"
- SMG0012, 15, 25, "2 seconds", "State2"
---

using Twia.StateMachine;

namespace Twia.StateMachine.CodeGenerator.UnitTests;

[StateMachine]
public partial class UnitTestStateMachine
{
    [InitialState]
    [TransitionAfter("a", "State2")]
    public partial void State1();

    [State]
    [TransitionAfter("T1H", "State2")]
    [TransitionAfter("2 seconds", "State2")] 
    public partial void State2();
}

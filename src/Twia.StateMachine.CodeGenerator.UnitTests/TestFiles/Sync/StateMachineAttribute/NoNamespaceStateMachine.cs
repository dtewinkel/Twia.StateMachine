---
Name: StateMachine not in a namespace
Output: Source
---

#pragma warning disable CS1591

using Twia.StateMachine;

[StateMachine]
internal partial class UnitTestStateMachine
{
    [InitialState]
    public partial void State1();

    [Trigger]
    public partial void Trigger1();
}
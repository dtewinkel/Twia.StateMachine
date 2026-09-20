---
Name: StateMachine in nested classes
Output: Source
---

namespace Twia.StateMachine.CodeGenerator.UnitTests;

#pragma warning disable CS1591

public partial class GrandParentClass
{
    public partial class ParentClass
    {
        [StateMachine]
        public partial class UnitTestStateMachine
        {
            [Trigger]
            public partial void ButtonPressed();

            #pragma warning disable CS1591
            [Transition("ButtonPressed", "Off")]
            [InitialState]
            private partial void Off();
        }
    }
}
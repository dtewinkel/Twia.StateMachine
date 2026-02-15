namespace Twia
{
    namespace StateMachine
    {
        namespace IntegrationTests
        {
            namespace Nested
            {
                public partial class NestedStateMachineParent
                {
                    [StateMachine]
                    public partial class NestedStateMachineChild
                    {
                        private NestedStateMachineGrandchild? _nestedState;

                        [StateMachine]
                        private partial class NestedStateMachineGrandchild
                        {
                            [Trigger]
                            public partial void Start();

                            [Trigger]
                            public partial void Stop();

                            [State, InitialState]
                            [Transition(nameof(Start), nameof(Running))]
                            private partial void Stopped();

                            [State]
                            [Transition(nameof(Stop), nameof(Stopped))]
                            private partial void Running();
                        }


                        [Trigger]
                        public partial void StartEngine();

                        [Trigger]
                        public partial void StopEngine();

                        [OnEntry($"{nameof(OnEntryState1)}()")]
                        [OnExit($"{nameof(OnExitState1)}()")]
                        [InitialState]
                        private partial void State1();

                        private void OnEntryState1()
                        { 
                            _nestedState = new NestedStateMachineGrandchild();
                            _nestedState.InitializeStateMachine();
                        }

                        private void OnExitState1()
                        {
                            _nestedState = null;
                        }
                    }

                }
            }
        }
    }
}

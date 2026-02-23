using Microsoft.CodeAnalysis.Testing;
using Twia.StateMachine.CodeGenerator.UnitTests.Verifiers;

namespace Twia.StateMachine.CodeGenerator.UnitTests;

[TestClass]
public sealed class StateMachineIncrementalCodeGeneratorTests
{
    private readonly IncrementalGeneratorVerifier<StateMachineIncrementalCodeGenerator> _verifier = new(
        "Twia.StateMachine.CodeGenerator.UnitTests.", 
        "_StateMachine.g.cs",
        // Suppress all diagnostics from the compiler itself. We only want to test the generator diagnostics.
        // Risk: This might hide useful information if the input sources are invalid.
        CompilerDiagnostics.None);

    public StateMachineIncrementalCodeGeneratorTests()
    {
        _verifier.AddAdditionalFileReferences("Twia.StateMachine.dll");
        _verifier.DisabledDiagnostics.Add("CS0759");
    }

    [TestMethod]
    public async Task Generator_WithNoAttribute_GeneratesNoCode()
    {
        const string code = """
            namespace Twia.StateMachine.CodeGenerator.UnitTests;
            
            public partial class UnitTestEmptyStateMachine
            {
            }
            """;

        await _verifier.VerifyGeneratorAsyncWithEmptyResult([code]);
    }

    [TestMethod]
    public async Task Generator_OnRecordType_GeneratesNoCode()
    {
        const string code = """
                            using Twia.StateMachine;

                            namespace Twia.StateMachine.CodeGenerator.UnitTests;

                            [StateMachine]
                            public partial record UnitTestEmptyStateMachine
                            {
                            }
                            """;

        await _verifier.VerifyGeneratorAsyncWithEmptyResult([code]);
    }

    [TestMethod]
    public async Task Generator_NotPartialClass_GeneratesNoCode()
    {
        const string code = """
                            using Twia.StateMachine;
                            
                            namespace Twia.StateMachine.CodeGenerator.UnitTests;
                            
                            [StateMachine]
                            public class UnitTestEmptyStateMachine
                            {
                            }
                            """;

        var diagnostics = DiagnosticResult
            .CompilerError("SMG0001")
            .WithLocation(6, 14)
            .WithArguments("UnitTestEmptyStateMachine");
        await _verifier.VerifyGeneratorAsyncWithOnlyDiagnostics([code], [diagnostics]);
    }

    [TestMethod]
    public async Task Generator_NoInitialStates_GeneratesNoCode()
    {
        const string code = """
                            using Twia.StateMachine;

                            namespace Twia.StateMachine.CodeGenerator.UnitTests;

                            [StateMachine]
                            public partial class UnitTestEmptyStateMachine
                            {
                                [State]
                                public partial void State1()
                                {
                                }

                                [State]
                                public partial void State2()
                                {
                                }
                            }
                            """;

        var diagnostics = DiagnosticResult
            .CompilerError("SMG0003")
            .WithLocation(6, 22)
            .WithArguments("UnitTestEmptyStateMachine");
        await _verifier.VerifyGeneratorAsyncWithOnlyDiagnostics([code], [diagnostics]);
    }

    [TestMethod]
    public async Task Generator_MultipleInitialStates_GeneratesNoCode()
    {
        const string code = """
                            using Twia.StateMachine;

                            namespace Twia.StateMachine.CodeGenerator.UnitTests;

                            [StateMachine]
                            public partial class UnitTestEmptyStateMachine
                            {
                                [State, InitialState]
                                public partial void State1()
                                {
                                }

                                [State, InitialState]
                                public partial void State2()
                                {
                                }
                            }
                            """;

        var diagnostics = DiagnosticResult
            .CompilerError("SMG0002")
            .WithLocation(14, 25)
            .WithArguments("State2", "State1");
        await _verifier.VerifyGeneratorAsyncWithOnlyDiagnostics([code], [diagnostics]);
    }

    [TestMethod]
    public async Task Generator_MethodIsStateAndTrigger_GeneratesNoCode()
    {
        const string code = """
                            using Twia.StateMachine;

                            namespace Twia.StateMachine.CodeGenerator.UnitTests;

                            [StateMachine]
                            public partial class UnitTestEmptyStateMachine
                            {
                                [Trigger, State]
                                public partial void State1()
                                {
                                }

                                [State, InitialState]
                                public partial void State2()
                                {
                                }
                            }
                            """;

        var diagnostics = DiagnosticResult
            .CompilerError("SMG0004")
            .WithLocation(9, 25)
            .WithArguments("State1");
        await _verifier.VerifyGeneratorAsyncWithOnlyDiagnostics([code], [diagnostics]);
    }

    [TestMethod]
    public async Task Generator_MethodIsNotStateButHasTransitionAttributes_GeneratesNoCode()
    {
        const string code = """
                            using Twia.StateMachine;

                            namespace Twia.StateMachine.CodeGenerator.UnitTests;

                            [StateMachine]
                            public partial class UnitTestEmptyStateMachine
                            {
                                [OnEntry("DoNothing()")]
                                public partial void State1();

                                [OnExit("DoNothing()")]
                                public partial void State2();
                            
                                [Transition("Trigger1", "State1")]
                                public partial void State3();
                                
                                [TransitionAfter("PT00:00:01", "State1")]
                                public partial void State4();

                                [InitialState]
                                public partial void State5();
                                
                                [Trigger]
                                public partial void Trigger1();
                            }
                            """;

        var diagnostics1 = DiagnosticResult
            .CompilerError("SMG0005")
            .WithLocation(9, 25)
            .WithArguments("State1");
        var diagnostics2 = DiagnosticResult
            .CompilerError("SMG0005")
            .WithLocation(12, 25)
            .WithArguments("State2");
        var diagnostics3 = DiagnosticResult
            .CompilerError("SMG0005")
            .WithLocation(15, 25)
            .WithArguments("State3");
        var diagnostics4 = DiagnosticResult
            .CompilerError("SMG0005")
            .WithLocation(18, 25)
            .WithArguments("State4");
        await _verifier.VerifyGeneratorAsyncWithOnlyDiagnostics([code], [diagnostics1, diagnostics2, diagnostics3, diagnostics4]);
    }

    [TestMethod]
    public async Task Generator_MethodIsTriggerButHasTransitionAttributes_GeneratesNoCode()
    {
        const string code = """
                            using Twia.StateMachine;

                            namespace Twia.StateMachine.CodeGenerator.UnitTests;

                            [StateMachine]
                            public partial class UnitTestEmptyStateMachine
                            {
                                [Trigger, OnEntry("DoNothing()")]
                                public partial void State1();

                                [Trigger, OnExit("DoNothing()")]
                                public partial void State2();
                            
                                [Trigger, Transition("Trigger1", "State1")]
                                public partial void State3();
                                
                                [Trigger, TransitionAfter("PT00:00:01", "State1")]
                                public partial void State4();

                                [InitialState]
                                public partial void State5();
                                
                                [Trigger]
                                public partial void Trigger1();
                            }
                            """;

        var diagnostics1 = DiagnosticResult
            .CompilerError("SMG0006")
            .WithLocation(9, 25)
            .WithArguments("State1");
        var diagnostics2 = DiagnosticResult
            .CompilerError("SMG0006")
            .WithLocation(12, 25)
            .WithArguments("State2");
        var diagnostics3 = DiagnosticResult
            .CompilerError("SMG0006")
            .WithLocation(15, 25)
            .WithArguments("State3");
        var diagnostics4 = DiagnosticResult
            .CompilerError("SMG0006")
            .WithLocation(18, 25)
            .WithArguments("State4");
        await _verifier.VerifyGeneratorAsyncWithOnlyDiagnostics([code], [diagnostics1, diagnostics2, diagnostics3, diagnostics4]);
    }

    [TestMethod]
    public async Task Generator_TriggerOrStateMethodNotPartial_GeneratesNoCode()
    {
        const string code = """
                            using Twia.StateMachine;

                            namespace Twia.StateMachine.CodeGenerator.UnitTests;

                            [StateMachine]
                            public partial class UnitTestEmptyStateMachine
                            {
                                [State, InitialState]
                                public void State1();

                                [Trigger]
                                public void Trigger1();
                            }
                            """;

        var diagnostics1 = DiagnosticResult
            .CompilerError("SMG0007")
            .WithLocation(9, 17)
            .WithArguments("State1");
        var diagnostics2 = DiagnosticResult
            .CompilerError("SMG0007")
            .WithLocation(12, 17)
            .WithArguments("Trigger1");
        await _verifier.VerifyGeneratorAsyncWithOnlyDiagnostics([code], [diagnostics1, diagnostics2]);
    }
    
    [TestMethod]
    public async Task Generator_TriggerOrStateMethodNotVoid_GeneratesNoCode()
    {
        const string code = """
                            using Twia.StateMachine;

                            namespace Twia.StateMachine.CodeGenerator.UnitTests;

                            [StateMachine]
                            public partial class UnitTestEmptyStateMachine
                            {
                                [State, InitialState]
                                public partial bool State1();

                                [Trigger]
                                public partial Task Trigger1Async();
                            }
                            """;

        var diagnostics1 = DiagnosticResult
            .CompilerError("SMG0008")
            .WithLocation(9, 25)
            .WithArguments("State1");
        var diagnostics2 = DiagnosticResult
            .CompilerError("SMG0008")
            .WithLocation(12, 25)
            .WithArguments("Trigger1Async");
        await _verifier.VerifyGeneratorAsyncWithOnlyDiagnostics([code], [diagnostics1, diagnostics2]);
    }

    [TestMethod]
    public async Task Generator_TriggerOrStateWithParameters_GeneratesNoCode()
    {
        const string code = """
                            using Twia.StateMachine;
                            using System.Threading;

                            namespace Twia.StateMachine.CodeGenerator.UnitTests;

                            [StateMachine]
                            public partial class UnitTestEmptyStateMachine
                            {
                                [State, InitialState]
                                public partial void State1(string name);

                                [Trigger]
                                public partial void Trigger1Async(CancellationToken ct);
                            }
                            """;

        var diagnostics1 = DiagnosticResult
            .CompilerError("SMG0009")
            .WithLocation(10, 25)
            .WithArguments("State1");
        var diagnostics2 = DiagnosticResult
            .CompilerError("SMG0009")
            .WithLocation(13, 25)
            .WithArguments("Trigger1Async");
        await _verifier.VerifyGeneratorAsyncWithOnlyDiagnostics([code], [diagnostics1, diagnostics2]);
    }

    [TestMethod]
    public async Task Generator_WithMixOfErrors_ReportsThemAll()
    {
        const string code = """
                            using Twia.StateMachine;

                            namespace Twia.StateMachine.CodeGenerator.UnitTests;

                            [StateMachine]
                            public class UnitTestEmptyStateMachine
                            {
                                [Trigger, OnEntry("DoNothing()")]
                                public partial void State1();

                                [OnExit("DoNothing()")]
                                public partial void State2();
                            
                                [State]
                                public void State3();
                            }
                            """;

        var diagnostics1 = DiagnosticResult
            .CompilerError("SMG0001")
            .WithLocation(6, 14)
            .WithArguments("UnitTestEmptyStateMachine");
        var diagnostics2 = DiagnosticResult
            .CompilerError("SMG0003")
            .WithLocation(6, 14)
            .WithArguments("UnitTestEmptyStateMachine");
        var diagnostics3 = DiagnosticResult
            .CompilerError("SMG0006")
            .WithLocation(9, 25)
            .WithArguments("State1");
        var diagnostics4 = DiagnosticResult
            .CompilerError("SMG0005")
            .WithLocation(12, 25)
            .WithArguments("State2");
        var diagnostics5 = DiagnosticResult
            .CompilerError("SMG0007")
            .WithLocation(15, 17)
            .WithArguments("State3");
        await _verifier.VerifyGeneratorAsyncWithOnlyDiagnostics([code], [diagnostics1, diagnostics2, diagnostics3, diagnostics4, diagnostics5]);
    }

    [TestMethod]
    public async Task Generator_WithAttribute_GeneratesCode()
    {
        const string code = """
                            using Twia.StateMachine;
                            
                            namespace Twia.StateMachine.CodeGenerator.UnitTests;
                            
                            [StateMachine]
                            internal partial class UnitTestEmptyStateMachine
                            {
                                [Trigger]
                                public partial void ButtonPressed();
                                
                                [State, InitialState]
                                internal partial void Off();
                                
                                [State]
                                partial void On();
                            }
                            """;

        const string expectedCode = """
                                    // <auto-generated/>
                                    // Generated by Twia.StateMachine.CodeGenerator version {{Version}} at {{Time}} UTC.
                                    #nullable enable

                                    namespace Twia.StateMachine.CodeGenerator.UnitTests;

                                    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Twia.StateMachine.CodeGenerator", "{{Version}}")]
                                    internal partial class UnitTestEmptyStateMachine
                                    {
                                        private enum __{{GeneratedId}}_Trigger
                                        {
                                            // Undefined value.
                                            __{{GeneratedId}}_Undefined = 0,
                                            // User defined triggers.
                                            ButtonPressed,
                                            // System defined triggers.
                                            __{{GeneratedId}}_Entry
                                        }

                                        private __{{GeneratedId}}_Trigger _current__{{GeneratedId}}_Trigger = __{{GeneratedId}}_Trigger.__{{GeneratedId}}_Undefined;

                                        /// <summary>
                                        /// Enumeration of the states that the state machine can be in.
                                        /// </summary>
                                        /// <remarks>
                                        /// The names of the members of the <see cref="State" /> enum are the names of the state methods of the <see cref="UnitTestEmptyStateMachine" /> class.
                                        /// </remarks>
                                        public enum State
                                        {
                                            Off = 1,
                                            On
                                        }

                                        private State __{{GeneratedId}}_CurrentStateBacking = 0;

                                        /// <summary>
                                        /// Readonly property to get the current state the state machine is in.
                                        /// </summary>
                                        /// <value>
                                        /// The current state of the state machine.
                                        /// </value>
                                        /// <exception cref="global::System.InvalidOperationException">
                                        /// the state is not initialized yet
                                        /// </exception>
                                        public State CurrentState
                                        {
                                            get
                                            {
                                                __{{GeneratedId}}_AssertIsInitialized();

                                                return __{{GeneratedId}}_CurrentStateBacking;
                                            }

                                            private set
                                            {
                                                __{{GeneratedId}}_CurrentStateBacking = value;
                                            }
                                        }

                                        /// <summary>
                                        /// Initialize the state machine before it is used.
                                        /// </summary>
                                        /// <remarks>
                                        /// The state machine must be initialize once (and only once) before any of the other generated methods and properties can be used.
                                        /// </remarks>
                                        /// <exception cref="global::System.InvalidOperationException">
                                        /// InitializeStateMachine() can only be called one in the life of a state machine
                                        /// </exception>
                                        public void InitializeStateMachine()
                                        {
                                            if (__{{GeneratedId}}_CurrentStateBacking != 0)
                                            {
                                                throw new global::System.InvalidOperationException("'InitializeStateMachine()' can only be called once in the lifecycle of an instance.");
                                            }

                                            // Move to initial state 'Off'.
                                            __{{GeneratedId}}_CurrentStateBacking = State.Off;
                                            __{{GeneratedId}}_InvokeTrigger(__{{GeneratedId}}_Trigger.__{{GeneratedId}}_Entry);
                                        }

                                        private void __{{GeneratedId}}_AssertIsInitialized()
                                        {
                                            if (__{{GeneratedId}}_CurrentStateBacking == 0)
                                            {
                                                throw new global::System.InvalidOperationException("The state machine is not initialized yet. Call 'InitializeStateMachine()' before using it.");
                                            }
                                        }

                                        public partial void ButtonPressed()
                                        {
                                            __{{GeneratedId}}_AssertIsInitialized();

                                            __{{GeneratedId}}_InvokeTrigger(__{{GeneratedId}}_Trigger.ButtonPressed);
                                        }

                                        private void __{{GeneratedId}}_EnterState(State state)
                                        {
                                            CurrentState = state;
                                            __{{GeneratedId}}_InvokeTrigger(__{{GeneratedId}}_Trigger.__{{GeneratedId}}_Entry);
                                        }

                                        private void __{{GeneratedId}}_InvokeTrigger(__{{GeneratedId}}_Trigger trigger)
                                        {
                                            _current__{{GeneratedId}}_Trigger = trigger;

                                            switch (CurrentState)
                                            {
                                                case State.Off:
                                                    Off();
                                                    break;

                                                case State.On:
                                                    On();
                                                    break;
                                            }
                                        }

                                        internal partial void Off()
                                        {
                                        }

                                        partial void On()
                                        {
                                        }
                                    }

                                    """;

        await _verifier.VerifyGeneratorAsync([code], ("*UnitTestEmptyStateMachine*", expectedCode));
    }

    [TestMethod]
    public async Task Generator_WithFullAttributeNames_GeneratesCode()
    {
        const string code = """
                            using Twia.StateMachine;
                            
                            namespace Twia.StateMachine.CodeGenerator.UnitTests;
                            
                            [StateMachine.StateMachine]
                            internal partial class UnitTestStateMachine
                            {
                                private int _offCount = 0;
                                private int _onCount = 0;
                            
                                [StateMachine.TriggerAttribute]
                                public partial void ButtonPressed();
                                
                                [Twia.StateMachine.Transition("ButtonPressed", "On", Condition = "_offCount < 100", Action = "_offCount++")]
                                [Twia.StateMachine.InitialState]
                                internal partial void Off();
                                
                                [StateMachine.OnEntry("_onCount++")]
                                [StateMachine.Transition("ButtonPressed", "On", Condition = "_offCount < 100", Action = "_offCount++")]
                                [Twia.StateMachine.StateAttribute]
                                partial void On();
                            }
                            """;

        var expectedCode = """
                             // <auto-generated/>
                             // Generated by Twia.StateMachine.CodeGenerator version {{Version}} at {{Time}} UTC.
                             #nullable enable
                             
                             namespace Twia.StateMachine.CodeGenerator.UnitTests;
                             
                             [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Twia.StateMachine.CodeGenerator", "{{Version}}")]
                             internal partial class UnitTestStateMachine
                             {
                                 private enum __{{GeneratedId}}_Trigger
                                 {
                                     // Undefined value.
                                     __{{GeneratedId}}_Undefined = 0,
                                     // User defined triggers.
                                     ButtonPressed,
                                     // System defined triggers.
                                     __{{GeneratedId}}_Entry
                                 }
                             
                                 private __{{GeneratedId}}_Trigger _current__{{GeneratedId}}_Trigger = __{{GeneratedId}}_Trigger.__{{GeneratedId}}_Undefined;
                             
                                 /// <summary>
                                 /// Enumeration of the states that the state machine can be in.
                                 /// </summary>
                                 /// <remarks>
                                 /// The names of the members of the <see cref="State" /> enum are the names of the state methods of the <see cref="UnitTestStateMachine" /> class.
                                 /// </remarks>
                                 public enum State
                                 {
                                     Off = 1,
                                     On
                                 }
                             
                                 private State __{{GeneratedId}}_CurrentStateBacking = 0;
                             
                                 /// <summary>
                                 /// Readonly property to get the current state the state machine is in.
                                 /// </summary>
                                 /// <value>
                                 /// The current state of the state machine.
                                 /// </value>
                                 /// <exception cref="global::System.InvalidOperationException">
                                 /// the state is not initialized yet
                                 /// </exception>
                                 public State CurrentState
                                 {
                                     get
                                     {
                                         __{{GeneratedId}}_AssertIsInitialized();
                             
                                         return __{{GeneratedId}}_CurrentStateBacking;
                                     }
                             
                                     private set
                                     {
                                         __{{GeneratedId}}_CurrentStateBacking = value;
                                     }
                                 }
                             
                                 /// <summary>
                                 /// Initialize the state machine before it is used.
                                 /// </summary>
                                 /// <remarks>
                                 /// The state machine must be initialize once (and only once) before any of the other generated methods and properties can be used.
                                 /// </remarks>
                                 /// <exception cref="global::System.InvalidOperationException">
                                 /// InitializeStateMachine() can only be called one in the life of a state machine
                                 /// </exception>
                                 public void InitializeStateMachine()
                                 {
                                     if (__{{GeneratedId}}_CurrentStateBacking != 0)
                                     {
                                         throw new global::System.InvalidOperationException("'InitializeStateMachine()' can only be called once in the lifecycle of an instance.");
                                     }
                             
                                     // Move to initial state 'Off'.
                                     __{{GeneratedId}}_CurrentStateBacking = State.Off;
                                     __{{GeneratedId}}_InvokeTrigger(__{{GeneratedId}}_Trigger.__{{GeneratedId}}_Entry);
                                 }
                             
                                 private void __{{GeneratedId}}_AssertIsInitialized()
                                 {
                                     if (__{{GeneratedId}}_CurrentStateBacking == 0)
                                     {
                                         throw new global::System.InvalidOperationException("The state machine is not initialized yet. Call 'InitializeStateMachine()' before using it.");
                                     }
                                 }
                             
                                 public partial void ButtonPressed()
                                 {
                                     __{{GeneratedId}}_AssertIsInitialized();
                             
                                     __{{GeneratedId}}_InvokeTrigger(__{{GeneratedId}}_Trigger.ButtonPressed);
                                 }
                             
                                 private void __{{GeneratedId}}_EnterState(State state)
                                 {
                                     CurrentState = state;
                                     __{{GeneratedId}}_InvokeTrigger(__{{GeneratedId}}_Trigger.__{{GeneratedId}}_Entry);
                                 }
                             
                                 private void __{{GeneratedId}}_InvokeTrigger(__{{GeneratedId}}_Trigger trigger)
                                 {
                                     _current__{{GeneratedId}}_Trigger = trigger;
                             
                                     switch (CurrentState)
                                     {
                                         case State.Off:
                                             Off();
                                             break;
                             
                                         case State.On:
                                             On();
                                             break;
                                     }
                                 }
                             
                                 internal partial void Off()
                                 {
                                     switch (_current__{{GeneratedId}}_Trigger)
                                     {
                                         case __{{GeneratedId}}_Trigger.ButtonPressed:
                                             if (_offCount < 100)
                                             {
                                                 _offCount++;
                                                 __{{GeneratedId}}_EnterState(State.On);
                                             }
                                             break;
                                     }
                                 }
                             
                                 partial void On()
                                 {
                                     switch (_current__{{GeneratedId}}_Trigger)
                                     {
                                         case __{{GeneratedId}}_Trigger.__{{GeneratedId}}_Entry:
                                             _onCount++;
                                             break;
                             
                                         case __{{GeneratedId}}_Trigger.ButtonPressed:
                                             if (_offCount < 100)
                                             {
                                                 _offCount++;
                                                 __{{GeneratedId}}_EnterState(State.On);
                                             }
                                             break;
                                     }
                                 }
                             }
                             
                             """;

        await _verifier.VerifyGeneratorAsync([code], ("*UnitTestStateMachine*", expectedCode));
    }
}
using Twia.StateMachine.CodeGenerator.Declarations;

namespace Twia.StateMachine.CodeGenerator.Builders.Sync;

public class ObservableBuilder : BuilderBase
{
    private readonly CSharpDocumentWriter _document;
    private readonly StatesBuilder _statesBuilder;
    private readonly string _stateChangedMethodName;


    public ObservableBuilder(CSharpDocumentWriter document, StateMachineDeclaration declaration, ClassCommonBuilder classCommonBuilder, StatesBuilder statesBuilder)
    {
        _document = document;
        IsEnabled = declaration.Observable;
        _statesBuilder = statesBuilder;

        _stateChangedMethodName = classCommonBuilder.ToPrivateName("StateChanged");
    }

    public override bool IsEnabled { get; }

    public override bool AddPrivateMethods()
    {
        if (_statesBuilder.HasStates)
        {
            _document.WriteLine($"private void {_stateChangedMethodName}(");
            _document.Indent++;
            _document.WriteLine($"{_statesBuilder.StateFullTypeName} fromState,");
            _document.WriteLine($"{_statesBuilder.StateFullTypeName} toState,");
            _document.WriteLine("string reason");
            _document.WriteLine(")");
            _document.Indent--;
            _document.WriteLineBlockOpen();
            _document.WriteLine(
                $"var nullableFromState = fromState == {_statesBuilder.UndefinedStateName} ? ({_statesBuilder.StateFullTypeName}?)null : fromState;");
            InvokeOnStateChanged("nullableFromState", "toState", "reason");
            _document.WriteLineBlockClose();
            return true;
        }
        return false;
    }

    public void InvokeOnStateChanged(string fromState, string toState, string reason, bool addLine = false)
    {
        if (IsEnabled)
        {
            if(addLine)
            {
                _document.WriteLineNoTabs();
            }

            _document.WriteLine("OnStateChanged?.Invoke(this,");
            _document.Indent++;
            _document.WriteLine($"new global::Twia.StateMachine.StateChangedEventArgs<{_statesBuilder.StateFullTypeName}>({fromState}, {toState}, {reason})");
            _document.Indent--;
            _document.WriteLine(");");
        }
    }


    public override bool AddEvents()
    {
        _document.WriteLine("/// <summary>");
        _document.WriteLine("/// Occurs when the state of the state machine changes.");
        _document.WriteLine("/// </summary>");
        _document.WriteLine($"public event global::System.EventHandler<global::Twia.StateMachine.StateChangedEventArgs<{_statesBuilder.StateFullTypeName}>>? OnStateChanged;");
        return true;
    }

    public void AddObserveStateChange(string newStateParameterName, string reasonParameterName)
    {
        if (IsEnabled)
        {
            _document.WriteLine($"{_stateChangedMethodName}({_statesBuilder.StateFieldName}, {newStateParameterName}, {reasonParameterName});");
            _document.WriteLineNoTabs();
        }
    }
}

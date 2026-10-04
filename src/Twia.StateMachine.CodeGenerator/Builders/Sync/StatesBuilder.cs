using Twia.StateMachine.CodeGenerator.Declarations;

namespace Twia.StateMachine.CodeGenerator.Builders.Sync;

public class StatesBuilder : BuilderBase
{
    private readonly CSharpDocumentWriter _document;
    private readonly ClassCommonBuilder _classCommonBuilder;
    private readonly bool _stateIsPublic;
    private readonly Dictionary<string, MethodDeclaration> _states;
    private readonly string _stateTypeName;

    public StatesBuilder(CSharpDocumentWriter document, StateMachineDeclaration declaration, ClassCommonBuilder classCommonBuilder)
    {
        _document = document;
        _classCommonBuilder = classCommonBuilder;
        _stateIsPublic = declaration.StateAccessible || declaration.Observable;
        StateFieldName = _classCommonBuilder.ToPrivateName("CurrentState");
        EnterStateMethodName = _classCommonBuilder.ToPrivateName("EnterState");
        _stateTypeName = _stateIsPublic ? "State" : _classCommonBuilder.ToPrivateName("State");
        StateFullTypeName = $"{classCommonBuilder.StateMachineName}.{_stateTypeName}";
        UndefinedStateName = _classCommonBuilder.ToPrivateName("StateUndefined");

        _states = declaration.Methods.Where(method => method.IsState).ToDictionary(state => state.Name);

        HasInitialState = declaration.Methods.Count(method => method.IsInitial) == 1;
        InitialStateName = declaration.Methods.FirstOrDefault(method => method.IsInitial)?.Name;

        StateNames = [.. _states.Keys];
    }

    public string StateFullTypeName { get; }

    public string StateFieldName { get; }

    public string UndefinedStateName { get; }

    public bool HasInitialState { get; }

    public string? InitialStateName { get; }

    public List<string> StateNames { get; }

    public string EnterStateMethodName { get; }

    public MethodDeclaration GetState(string stateName) => _states[stateName];

    public bool TryGetState(string stateName, out MethodDeclaration? state) => _states.TryGetValue(stateName, out state);

    public bool StateExists(string stateName) => _states.ContainsKey(stateName);

    public override bool IsEnabled => true;

    public bool HasStates => StateNames.Count > 0;

    public override bool AddTypes()
    {
        var stateVisibility = _stateIsPublic ? "public" : "private";

        if (HasStates || _stateIsPublic)
        {
            _document.WriteLine("/// <summary>");
            _document.WriteLine("/// Enumeration of the states that the state machine can be in.");
            _document.WriteLine("/// </summary>");
            _document.WriteLine("/// <remarks>");
            _document.WriteLine($"/// The names of the members of the <see cref=\"{StateFullTypeName}\" /> enum are the names of the state methods of the <see cref=\"{_classCommonBuilder.StateMachineName}\" /> class.");
            _document.WriteLine("/// </remarks>");
            _document.WriteLine($"{stateVisibility} enum {_stateTypeName}");
            _document.WriteLineBlockOpen();
            _document.AddEnumMembers([.. _states.Keys], firstValue: 1, summaryTemplate: "The state machine is in the '{0}' state.");
            _document.WriteLineBlockClose();

            return true;
        }

        return false;
    }

    public override bool AddConstants()
    {
        _document.WriteLine($"private const {StateFullTypeName} {UndefinedStateName} = ({StateFullTypeName})0;");

        return true;
    }

    public override bool AddFields()
    {
        _document.WriteLine($"private {StateFullTypeName} {StateFieldName} = {UndefinedStateName};");

        return true;
    }
}

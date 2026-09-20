using Twia.StateMachine.CodeGenerator.Declarations;

namespace Twia.StateMachine.CodeGenerator.Builders.Sync;

public class TriggersBuilder : BuilderBase, ITriggersProvider
{
    private readonly CSharpDocumentWriter _document;
    private readonly ClassCommonBuilder _classCommonBuilder;
    private readonly StatesBuilder _statesBuilder;

    private readonly MethodDeclaration[] _triggerMethods;
    private readonly bool _hasStates;
    private readonly bool _hasTriggers;

    public TriggersBuilder(CSharpDocumentWriter document, StateMachineDeclaration declaration, ClassCommonBuilder classCommonBuilder, StatesBuilder statesBuilder)
    {
        _document = document;
        _classCommonBuilder = classCommonBuilder;
        _statesBuilder = statesBuilder;

        _triggerMethods = [.. declaration.Methods.Where(method => method.IsTrigger)];
        _hasTriggers = _triggerMethods.Any(t => !_statesBuilder.TryGetState(t.Name, out _));
        _hasStates = statesBuilder.HasStates;

        UndefinedTrigger = classCommonBuilder.ToPrivateName("Undefined");
        TriggerEnumTypeName = classCommonBuilder.ToPrivateName("Trigger");
        LastTriggerFieldName = classCommonBuilder.ToPrivateName("LastTrigger");

        EntryTriggerName = classCommonBuilder.ToPrivateName("Entry");

        InvokeTriggerMethodName = classCommonBuilder.ToPrivateName("InvokeTrigger");
    }

    public string UndefinedTrigger { get; }

    public string TriggerEnumTypeName { get; }

    public string EntryTriggerName { get; }

    public string InvokeTriggerMethodName { get; }

    public string LastTriggerFieldName { get; }

    public override bool IsEnabled => _triggerMethods.Length > 0 || _statesBuilder.HasStates;

    public string[] GetTriggerNames() => [ .. _triggerMethods.Select(trigger => trigger.Name), EntryTriggerName ];

    public override bool AddFields()
    {
        if (_hasStates)
        {
            _document.WriteLine($"private {TriggerEnumTypeName} {LastTriggerFieldName} = {TriggerEnumTypeName}.{UndefinedTrigger};");
            return true;
        }

        return false;
    }


    public override bool AddPublicMethods()
    {
        return _hasTriggers && AddTriggerMethods();
    }

    private bool AddTriggerMethods()
    {
        var first = true;
        var triggersAdded = false;

        foreach (var trigger in _triggerMethods)
        {
            var isAlsoState = _statesBuilder.TryGetState(trigger.Name, out _);

            if (!isAlsoState && trigger.IsPartial)
            {
                triggersAdded = true;
                first = _document.WriteSeparatorLine(first);

                var parameters = "";
                if (trigger.HasParameters)
                {
                    var parameterList = new List<string>();
                    foreach (var parameter in trigger.Parameters)
                    {
                        parameterList.Add($"{(string.IsNullOrWhiteSpace(parameter.Modifiers) ? "" : $"{parameter.Modifiers} ")}{parameter.ParameterType} {parameter.Name}");
                    }

                    parameters = string.Join(", ", parameterList);

                }

                _document.WriteLine($"{trigger.Modifiers} {trigger.ReturnType} {trigger.Name}({parameters})");
                _document.WriteLineBlockOpen();
                if (_hasStates)
                {
                    _document.WriteLine($"{_classCommonBuilder.AssertIsInitializedMethodName}();");
                    _document.WriteLineNoTabs();
                    _document.WriteLine($"{InvokeTriggerMethodName}({TriggerEnumTypeName}.{trigger.Name});");
                }

                if (trigger.ReturnType != CommonTypeNames.Void)
                {
                    _document.WriteLine($"return default({trigger.ReturnType});");
                }

                _document.WriteLineBlockClose();
            }
        }

        return triggersAdded;
    }
}
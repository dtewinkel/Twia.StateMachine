using Twia.StateMachine.CodeGenerator.Declarations;

namespace Twia.StateMachine.CodeGenerator.Builders.Sync;

public class TriggerMethodsProvider : ITriggersProvider
{
    public TriggerMethodsProvider(StateMachineDeclaration declaration)
    {
        Triggers = [.. declaration.Methods.Where(method => method.IsTrigger)];
        IsEnabled = Triggers.Length > 0;
        TriggerNames = [.. Triggers.Select(trigger => trigger.Name)];
    }

    public bool TriggerExists(string triggerName) => Triggers.Any(trigger => trigger.Name == triggerName);

    public bool TryGetTrigger(string triggerName, out MethodDeclaration? triggerMethod)
    {
        triggerMethod = Triggers.FirstOrDefault(trigger => trigger.Name == triggerName);
        return triggerMethod is not null;
    }

    /// <inheritdoc />
    public bool IsEnabled { get; }

    public MethodDeclaration[] Triggers { get; }

    /// <inheritdoc />
    public string[] TriggerNames { get; }
}
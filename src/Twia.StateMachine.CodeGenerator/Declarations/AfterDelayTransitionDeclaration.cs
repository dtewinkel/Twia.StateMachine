using Microsoft.CodeAnalysis;

namespace Twia.StateMachine.CodeGenerator.Declarations;

public class AfterDelayTransitionDeclaration : TransitionDeclaration
{
    private static int _index = 1;

    public AfterDelayTransitionDeclaration(string stateName, AttributeData attributeData)
    {
        TransitionType = TransitionType.AfterDelay;
        Name = $"{stateName}After{_index++}";
        Trigger = attributeData.ConstructorArguments[0].Value?.ToString() ?? "";
        TargetState = attributeData.ConstructorArguments[1].Value?.ToString() ?? "";
        Condition = attributeData.NamedArguments.FirstOrDefault(kv => kv.Key == "Condition").Value.Value?.ToString();
        Action = attributeData.NamedArguments.FirstOrDefault(kv => kv.Key == "Action").Value.Value?.ToString();
    }
}
using Microsoft.CodeAnalysis;

namespace Twia.StateMachine.CodeGenerator;

public static class AttributeDataExtensions
{
    public static string GetFullName(this AttributeData attributeData)
    {
        return attributeData.AttributeClass is not null 
            ? $"{attributeData.AttributeClass.ContainingNamespace}.{attributeData.AttributeClass.Name}"
            : throw new InvalidOperationException("attributeData.AttributeClass must not be null");
    }
}
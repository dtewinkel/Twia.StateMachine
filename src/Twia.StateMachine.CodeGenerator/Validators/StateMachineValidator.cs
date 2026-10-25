using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Twia.StateMachine.CodeGenerator.Declarations;

namespace Twia.StateMachine.CodeGenerator.Validators;

public class StateMachineValidator : BaseValidator
{
    protected override void MethodsSignaturesAreCorrect(SourceProductionContext context, StateMachineDeclaration declaration)
    {
        base.MethodsSignaturesAreCorrect(context, declaration);
        foreach (var declarationMethod in declaration.Methods)
        { 
            if ((declarationMethod.IsState || declarationMethod.IsTrigger) && declarationMethod.ReturnType is not "void")
            {
                context.ReportDiagnostic(StateMachineGeneratorDiagnostics.MethodMustHaveVoidReturnType((MethodDeclarationSyntax)declarationMethod.Node));
            }
            if ((declarationMethod.IsState || declarationMethod.IsTrigger) && declarationMethod.HasParameters)
            {
                context.ReportDiagnostic(StateMachineGeneratorDiagnostics.MethodMustHaveNoParameters((MethodDeclarationSyntax)declarationMethod.Node));
            }
        }
    }
}

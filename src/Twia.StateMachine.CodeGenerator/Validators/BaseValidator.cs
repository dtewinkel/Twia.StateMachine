using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Twia.StateMachine.CodeGenerator.Declarations;

namespace Twia.StateMachine.CodeGenerator.Validators;

public class BaseValidator
{
    public virtual bool IsDeclarationValid(SourceProductionContext context, StateMachineDeclaration declaration)
    {
        var declarationIsPartial = DeclarationIsPartial(context, declaration);
        if (declaration.Methods.Count == 0)
        {
            return declarationIsPartial;
        }

        InitialStateIsValid(context, declaration);
        MethodsAreStateOrTrigger(context, declaration);
        MethodsSignaturesAreCorrect(context, declaration);
        TriggerNamesAreCorrect(context, declaration);
        AfterTimeSpansAreCorrect(context, declaration);
        StateNamesAreCorrect(context, declaration);
        return declarationIsPartial;
    }

    protected virtual void StateNamesAreCorrect(SourceProductionContext context, StateMachineDeclaration declaration)
    {
        var states = declaration.Methods.Where(method => method.IsState).ToList();
        var stateNames = states.Select(method => method.Name).ToArray();

        foreach (var state in states)
        {
            var transitions =
                state.Transitions.Where(transition => transition.TransitionType is TransitionType.OnTrigger or TransitionType.AfterDelay or TransitionType.Triggerless);
            foreach (var transitionDeclaration in transitions)
            {
                var targetState = transitionDeclaration.TargetState;
                if (!stateNames.Contains(targetState))
                {
                    context.ReportDiagnostic(StateMachineGeneratorDiagnostics.StateMustBeDefined((MethodDeclarationSyntax)state.Node, targetState));
                }
            }
        }
    }

    protected virtual void TriggerNamesAreCorrect(SourceProductionContext context, StateMachineDeclaration declaration)
    {
        var triggerNames = declaration.Methods
            .Where(method => method.IsTrigger)
            .Select(method => method.Name)
            .ToArray();

        var states = declaration.Methods.Where(method => method.IsState);

        foreach (var state in states)
        {
            var transitions =
                state.Transitions.Where(transition => transition.TransitionType is TransitionType.OnTrigger or TransitionType.Internal);
            foreach (var transitionDeclaration in transitions)
            {
                var trigger = transitionDeclaration.Trigger;
                if (!triggerNames.Contains(trigger))
                {
                    context.ReportDiagnostic(StateMachineGeneratorDiagnostics.TriggerMustBeDefined((MethodDeclarationSyntax)state.Node, trigger));
                }
            }
        }
    }

    protected virtual void AfterTimeSpansAreCorrect(SourceProductionContext context, StateMachineDeclaration declaration)
    {
        var states = declaration.Methods.Where(method => method.IsState);
        foreach (var state in states)
        {
            var transitions =
                state.Transitions.Where(transition => transition.TransitionType is TransitionType.AfterDelay or TransitionType.InternalAfter or TransitionType.InternalAfterEvery);
            foreach (var transitionDeclaration in transitions)
            {
                var trigger = transitionDeclaration.Trigger;

                if (!ValidatePeriod(trigger))
                {
                    context.ReportDiagnostic(StateMachineGeneratorDiagnostics.TimeSpanMustBeValid((MethodDeclarationSyntax)state.Node, trigger));
                }
            }
        }
    }

    private static bool ValidatePeriod(string trigger)
    {
        if (!TimeSpan.TryParse(trigger, out _))
        {
            try
            {
                _ = XmlConvert.ToTimeSpan(trigger);
            }
            catch (FormatException)
            {
                return false;
            }
        }
        return true;
    }

    protected virtual bool DeclarationIsPartial(SourceProductionContext context, StateMachineDeclaration declaration)
    {
        if (!declaration.IsPartial)
        {
            context.ReportDiagnostic(StateMachineGeneratorDiagnostics.ClassMustBePartialDiagnostics((ClassDeclarationSyntax)declaration.Node));
            return false;
        }

        return true;
    }

    protected virtual void MethodsAreStateOrTrigger(SourceProductionContext context, StateMachineDeclaration declaration)
    {
        foreach (var declarationMethod in declaration.Methods)
        {
            if (declarationMethod is { IsState: true, IsTrigger: true })
            {
                context.ReportDiagnostic(StateMachineGeneratorDiagnostics.MethodCannotBeTriggerAndState((MethodDeclarationSyntax)declarationMethod.Node));
            }
            if (declarationMethod is { IsState: false, IsTrigger: false, HasTransitions: true })
            {
                context.ReportDiagnostic(StateMachineGeneratorDiagnostics.MethodMustBeAState((MethodDeclarationSyntax)declarationMethod.Node));
            }
            if (declarationMethod is { IsTrigger: true, HasTransitions: true })
            {
                context.ReportDiagnostic(StateMachineGeneratorDiagnostics.TriggerCannotHaveStateAttributes((MethodDeclarationSyntax)declarationMethod.Node));
            }
        }
    }

    protected virtual void MethodsSignaturesAreCorrect(SourceProductionContext context, StateMachineDeclaration declaration)
    {
        foreach (var declarationMethod in declaration.Methods)
        {
            if ((declarationMethod.IsState || declarationMethod.IsTrigger) && declarationMethod is { IsPartial: false })
            {
                context.ReportDiagnostic(StateMachineGeneratorDiagnostics.MethodMustBePartial((MethodDeclarationSyntax)declarationMethod.Node));
            }
        }
    }

    protected virtual void InitialStateIsValid(SourceProductionContext context, StateMachineDeclaration declaration)
    {
        var initialStates = declaration.Methods.Where(method => method.IsInitial).ToArray();
        switch (initialStates.Length)
        {
            case 0:
                context.ReportDiagnostic(StateMachineGeneratorDiagnostics.NoInitialStateDiagnostics((ClassDeclarationSyntax)declaration.Node));
                break;

            case > 1:
            {
                var initialStateName = initialStates[0].Name;
                foreach (var initialState in initialStates.Skip(1))
                {
                    context.ReportDiagnostic(StateMachineGeneratorDiagnostics.MultipleInitialStatesDiagnostics((MethodDeclarationSyntax)initialState.Node, initialStateName));
                }
                break;
            }
        }
    }
}

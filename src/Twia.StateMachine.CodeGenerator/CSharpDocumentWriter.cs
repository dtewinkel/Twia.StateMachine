using System.CodeDom.Compiler;
using System.Globalization;
using System.Text;
using Twia.StateMachine.CodeGenerator.Declarations;

namespace Twia.StateMachine.CodeGenerator;

public class CSharpDocumentWriter(int capacity = 10000)
    : IndentedTextWriter(new StringWriter(new StringBuilder(capacity), CultureInfo.InvariantCulture))
{
    public void WriteLineBlockOpen()
    {
        WriteLine("{");
        Indent++;
    }

    public void WriteLineBlockClose()
    {
        Indent--;
        WriteLine("}");
    }

    public void WriteLineNoTabs()
    {
        WriteLineNoTabs("");
    }

    public bool WriteSeparatorLine(bool first)
    {
        if (!first)
        {
            WriteLineNoTabs();
        }
        return false;
    }

    public void AddEnumMembers(List<string> members, bool moreToFollow = false, int? firstValue = null, string? summaryTemplate = null)
    {
        var count = members.Count;
        var position = 1;
        foreach (var member in members)
        {
            if(summaryTemplate != null)
            {
                WriteLine($"/// <summary>{string.Format(summaryTemplate, member)}</summary>");
            }
            Write($"{member}");
            if (firstValue.HasValue && position == 1)
            {
                Write($" = {firstValue.Value}");
            }
            WriteLine(position == count && !moreToFollow ? "" : ",");
            position++;
        }
    }

    public void WriteConditionActionAndTransition(TransitionDeclaration transitionDeclaration, string? onExitCall, Action<CSharpDocumentWriter, TransitionDeclaration> addBody)
    {
        var condition = transitionDeclaration.Condition;
        var hasCondition = !string.IsNullOrWhiteSpace(condition);
        var action = transitionDeclaration.Action;
        var hasAction = !string.IsNullOrWhiteSpace(action);

        if (hasCondition)
        {
            WriteMappedLine($"if ({condition})", transitionDeclaration.ConditionSource);
            WriteLineBlockOpen();
        }

        if (!string.IsNullOrWhiteSpace(onExitCall))
        {
            WriteLine(onExitCall);
        }

        if (hasAction)
        {
            WriteMappedLine($"{action};", transitionDeclaration.ActionSource);
            WriteLineNoTabs();
        }

        addBody(this, transitionDeclaration);

        if (hasCondition)
        {
            WriteLineBlockClose();
        }
    }

    public void WriteConditionAndAction(TransitionDeclaration transitionDeclaration)
    {
        var condition = transitionDeclaration.Condition;
        var hasCondition = !string.IsNullOrWhiteSpace(condition);
        var action = transitionDeclaration.Action;

        if (hasCondition)
        {
            WriteMappedLine($"if ({condition})", transitionDeclaration.ConditionSource);
            WriteLineBlockOpen();
        }
        WriteMappedLine($"{action};", transitionDeclaration.ActionSource);
        if (hasCondition)
        {
            WriteLineBlockClose();
        }
    }

    private void WriteMappedLine(string text, SnippetSourceLocation? source)
    {
        if (source is not null)
        {
            // Unlike C# string literals, #line filenames preserve backslashes literally.
            WriteLineNoTabs($"#line {source.Line.ToString(CultureInfo.InvariantCulture)} \"{source.Path}\"");
        }

        WriteLine(text);

        if (source is not null)
        {
            WriteLineNoTabs("#line default");
        }
    }
}
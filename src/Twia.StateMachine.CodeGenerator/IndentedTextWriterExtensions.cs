using System.CodeDom.Compiler;
using Twia.StateMachine.CodeGenerator.Declarations;

namespace Twia.StateMachine.CodeGenerator;

public static class IndentedTextWriterExtensions
{
    extension(IndentedTextWriter document)
    {

        public void WriteLineBlockOpen()
        {
            document.WriteLine("{");
            document.Indent++;
        }

        public void WriteLineBlockClose()
        {
            document.Indent--;
            document.WriteLine("}");
        }

        public void WriteLineNoTabs()
        {
            document.WriteLineNoTabs("");
        }

        public bool WriteSeparatorLine(bool first)
        {
            if (!first)
            {
                document.WriteLineNoTabs();
            }
            return false;
        }

        public void AddEnumMembers(List<string> members, bool moreToFollow = false, int? firstValue = null)
        {
            var count = members.Count;
            var position = 1;
            foreach (var member in members)
            {
                document.Write($"{member}");
                if (firstValue.HasValue && position == 1)
                {
                    document.Write($" = {firstValue.Value}");
                }
                document.WriteLine(position == count && !moreToFollow ? "" : ",");
                position++;
            }
        }

        public void WriteConditionActionAndTransition(TransitionDeclaration transitionDeclaration, string? onExitCall, Action<IndentedTextWriter, TransitionDeclaration> addBody)
        {
            var condition = transitionDeclaration.Condition;
            var hasCondition = !string.IsNullOrWhiteSpace(condition);
            var action = transitionDeclaration.Action;
            var hasAction = !string.IsNullOrWhiteSpace(action);

            if (hasCondition)
            {
                document.WriteLine($"if ({condition})");
                document.WriteLineBlockOpen();
            }

            if (!string.IsNullOrWhiteSpace(onExitCall))
            {
                document.WriteLine(onExitCall);
            }

            if (hasAction)
            {
                document.WriteLine($"{action};");
                document.WriteLineNoTabs();
            }

            addBody(document, transitionDeclaration);

            if (hasCondition)
            {
                document.WriteLineBlockClose();
            }
        }

        public void WriteConditionAndAction(TransitionDeclaration transitionDeclaration)
        {
            var condition = transitionDeclaration.Condition;
            var hasCondition = !string.IsNullOrWhiteSpace(condition);
            var action = transitionDeclaration.Action;

            if (hasCondition)
            {
                document.WriteLine($"if ({condition})");
                document.WriteLineBlockOpen();
            }
            document.WriteLine($"{action};");
            if (hasCondition)
            {
                document.WriteLineBlockClose();
            }
        }
    }
}
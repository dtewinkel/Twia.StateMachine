using System.CodeDom.Compiler;

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
    }
}
using System.CodeDom.Compiler;

namespace Twia.StateMachine.CodeGenerator;

public static class IndentedTextWriterExtensions
{
    public static void WriteLineBlockOpen(this IndentedTextWriter document)
    {
        document.WriteLine("{");
        document.Indent++;
    }

    public static void WriteLineBlockClose(this IndentedTextWriter document)
    {
        document.Indent--;
        document.WriteLine("}");
    }

    public static void WriteLineNoTabs(this IndentedTextWriter document)
    {
        document.WriteLineNoTabs("");
    }

}
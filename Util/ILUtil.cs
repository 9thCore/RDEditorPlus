using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace RDEditorPlus.Util
{
    public static class ILUtil
    {
        public static void Print(this Instruction instruction)
        {
            if (instruction.Operand is ILLabel label)
            {
                Plugin.LogInfo($"{instruction.Offset}: {instruction.OpCode} {label.Target.Offset}");
            }
            else
            {
                Plugin.LogInfo($"{instruction.Offset}: {instruction.OpCode} {instruction.Operand}");
            }
        }
    }
}

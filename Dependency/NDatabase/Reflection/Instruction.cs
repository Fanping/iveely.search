using System.Reflection.Emit;
using System.Text;

namespace NDatabase.Reflection
{
    internal sealed class Instruction
    {
        internal Instruction(int offset, OpCode opcode)
        {
            Offset = offset;
            OpCode = opcode;
        }

        public Instruction Next { get; internal set; }

        public int Offset { get; private set; }

        public OpCode OpCode { get; private set; }

        public object Operand { get; internal set; }

        public Instruction Previous { get; internal set; }

        private static void AppendLabel(StringBuilder builder, Instruction instruction)
        {
            builder.Append("IL_");
            builder.Append(instruction.Offset.ToString("x4"));
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            AppendLabel(builder, this);
            builder.Append(':');
            builder.Append(' ');
            builder.Append(OpCode.Name);
            if (Operand != null)
            {
                builder.Append(' ');
                switch (OpCode.OperandType)
                {
                    case OperandType.InlineString:
                        builder.Append('"');
                        builder.Append(Operand);
                        builder.Append('"');
                        break;

                    case OperandType.InlineSwitch:
                        {
                            var operand = (Instruction[]) Operand;
                            for (var i = 0; i < operand.Length; i++)
                            {
                                if (i > 0)
                                {
                                    builder.Append(',');
                                }
                                AppendLabel(builder, operand[i]);
                            }
                            break;
                        }
                    case OperandType.ShortInlineBrTarget:
                    case OperandType.InlineBrTarget:
                        AppendLabel(builder, (Instruction) Operand);
                        break;

                    default:
                        builder.Append(Operand);
                        break;
                }
            }
            return builder.ToString();
        }
    }
}
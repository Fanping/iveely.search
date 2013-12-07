using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace NDatabase.Reflection
{
    internal abstract class ILPattern
    {
        protected static Instruction GetLastMatchingInstruction(MatchContext context)
        {
            return context.Instruction == null ? null : context.Instruction.Previous;
        }

        internal abstract void Match(MatchContext context);

        internal static MatchContext Match(MethodBase method, ILPattern pattern)
        {
            if (method == null)
                throw new ArgumentNullException("method");

            if (pattern == null)
                throw new ArgumentNullException("pattern");

            var instructions = (IList<Instruction>) MethodBodyReader.GetInstructions(method).AsReadOnly();
            if (instructions.Count == 0)
                throw new ArgumentException();

            var context = new MatchContext(instructions[0]);
            pattern.Match(context);
            return context;
        }

        internal static ILPattern OpCode(OpCode opcode)
        {
            return new OpCodePattern(opcode);
        }

        private static ILPattern Optional(ILPattern pattern)
        {
            return new OptionalPattern(pattern);
        }

        internal static ILPattern Optional(params OpCode[] opcodes)
        {
            return Optional(Sequence((from opcode in opcodes select OpCode(opcode)).ToArray<ILPattern>()));
        }

        protected static ILPattern Optional(OpCode opcode)
        {
            return Optional(OpCode(opcode));
        }

        internal static ILPattern Sequence(params ILPattern[] patterns)
        {
            return new SequencePattern(patterns);
        }

        private void TryMatch(MatchContext context)
        {
            var instruction = context.Instruction;
            Match(context);

            if (!context.Success)
                context.Reset(instruction);
        }

        #region Nested type: OpCodePattern

        private sealed class OpCodePattern : ILPattern
        {
            private readonly OpCode _opcode;

            internal OpCodePattern(OpCode opcode)
            {
                _opcode = opcode;
            }

            internal override void Match(MatchContext context)
            {
                if (context.Instruction == null)
                {
                    context.Success = false;
                }
                else
                {
                    context.Success = context.Instruction.OpCode == _opcode;
                    context.Advance();
                }
            }
        }

        #endregion

        #region Nested type: OptionalPattern

        private sealed class OptionalPattern : ILPattern
        {
            private readonly ILPattern _pattern;

            internal OptionalPattern(ILPattern optional)
            {
                _pattern = optional;
            }

            internal override void Match(MatchContext context)
            {
                _pattern.TryMatch(context);
            }
        }

        #endregion

        #region Nested type: SequencePattern

        private sealed class SequencePattern : ILPattern
        {
            private readonly ILPattern[] _patterns;

            internal SequencePattern(ILPattern[] patterns)
            {
                _patterns = patterns;
            }

            internal override void Match(MatchContext context)
            {
                foreach (var pattern in _patterns)
                {
                    pattern.Match(context);
                    if (!context.Success)
                        return;
                }
            }
        }

        #endregion
    }
}
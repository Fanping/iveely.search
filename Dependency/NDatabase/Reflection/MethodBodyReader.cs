using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace NDatabase.Reflection
{
    internal sealed class MethodBodyReader
    {
        private static readonly OpCode[] OneByteOpcodes = new OpCode[0xe1];
        private static readonly OpCode[] TwoBytesOpcodes = new OpCode[0x1f];
        private readonly MethodBody _body;
        private readonly ByteBuffer _il;
        private readonly List<Instruction> _instructions = new List<Instruction>();
        private readonly IList<LocalVariableInfo> _locals;
        private readonly MethodBase _method;
        private readonly Type[] _methodArguments;
        private readonly Module _module;
        private readonly ParameterInfo[] _parameters;
        private readonly Type[] _typeArguments;

        static MethodBodyReader()
        {
            foreach (var info in typeof (OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                var code = (OpCode) info.GetValue(null);
                if (code.OpCodeType == OpCodeType.Nternal) 
                    continue;

                if (code.Size == 1)
                    OneByteOpcodes[code.Value] = code;
                else
                    TwoBytesOpcodes[code.Value & 0xff] = code;
            }
        }

        private MethodBodyReader(MethodBase method)
        {
            _method = method;
            _body = method.GetMethodBody();
            if (_body == null)
                throw new ArgumentException("Method has no body");

            var iLAsByteArray = _body.GetILAsByteArray();
            if (iLAsByteArray == null)
                throw new ArgumentException("Can not get the body of the method");

            if (!(method is ConstructorInfo))
                _methodArguments = method.GetGenericArguments();

            if (method.DeclaringType != null)
                _typeArguments = method.DeclaringType.GetGenericArguments();

            _parameters = method.GetParameters();
            _locals = _body.LocalVariables;
            _module = method.Module;
            _il = new ByteBuffer(iLAsByteArray);
        }

        private static Instruction GetInstruction(List<Instruction> instructions, int offset)
        {
            var count = instructions.Count;
            if ((offset >= 0) && (offset <= instructions[count - 1].Offset))
            {
                var num2 = 0;
                var num3 = count - 1;
                while (num2 <= num3)
                {
                    var num4 = num2 + ((num3 - num2)/2);
                    var instruction = instructions[num4];
                    var num5 = instruction.Offset;

                    if (offset == num5)
                        return instruction;

                    if (offset < num5)
                        num3 = num4 - 1;
                    else
                        num2 = num4 + 1;
                }
            }
            return null;
        }

        public static List<Instruction> GetInstructions(MethodBase method)
        {
            var reader = new MethodBodyReader(method);
            reader.ReadInstructions();
            return reader._instructions;
        }

        private LocalVariableInfo GetLocalVariable(int index)
        {
            return _locals[index];
        }

        private ParameterInfo GetParameter(int index)
        {
            if (!_method.IsStatic)
                index--;

            return _parameters[index];
        }

        private object GetVariable(Instruction instruction, int index)
        {
            if (TargetsLocalVariable(instruction.OpCode))
                return GetLocalVariable(index);

            return GetParameter(index);
        }

        private void ReadInstructions()
        {
            Instruction instruction = null;
            while (_il.Position < _il.Buffer.Length)
            {
                var instruction2 = new Instruction(_il.Position, ReadOpCode());
                ReadOperand(instruction2);
                if (instruction != null)
                {
                    instruction2.Previous = instruction;
                    instruction.Next = instruction2;
                }
                _instructions.Add(instruction2);
                instruction = instruction2;
            }
            ResolveBranches();
        }

        private OpCode ReadOpCode()
        {
            var index = _il.ReadByte();
            return index == 0xfe ? TwoBytesOpcodes[_il.ReadByte()] : OneByteOpcodes[index];
        }

        private void ReadOperand(Instruction instruction)
        {
            switch (instruction.OpCode.OperandType)
            {
                case OperandType.InlineBrTarget:
                    instruction.Operand = _il.ReadInt32() + _il.Position;
                    return;

                case OperandType.InlineField:
                case OperandType.InlineMethod:
                case OperandType.InlineTok:
                case OperandType.InlineType:
                    instruction.Operand = _module.ResolveMember(_il.ReadInt32(), _typeArguments, _methodArguments);
                    return;

                case OperandType.InlineI:
                    instruction.Operand = _il.ReadInt32();
                    return;

                case OperandType.InlineI8:
                    instruction.Operand = _il.ReadInt64();
                    return;

                case OperandType.InlineNone:
                    return;

                case OperandType.InlineR:
                    instruction.Operand = _il.ReadDouble();
                    return;

                case OperandType.InlineSig:
                    instruction.Operand = _module.ResolveSignature(_il.ReadInt32());
                    return;

                case OperandType.InlineString:
                    instruction.Operand = _module.ResolveString(_il.ReadInt32());
                    return;

                case OperandType.InlineSwitch:
                    {
                        var num = _il.ReadInt32();
                        var num2 = _il.Position + (4*num);
                        var numArray = new int[num];
                        for (var i = 0; i < num; i++)
                        {
                            numArray[i] = _il.ReadInt32() + num2;
                        }
                        instruction.Operand = numArray;
                        return;
                    }
                case OperandType.InlineVar:
                    instruction.Operand = GetVariable(instruction, _il.ReadInt16());
                    return;

                case OperandType.ShortInlineBrTarget:
                    instruction.Operand = ((sbyte) _il.ReadByte()) + _il.Position;
                    return;

                case OperandType.ShortInlineI:
                    if (!(instruction.OpCode == OpCodes.Ldc_I4_S))
                    {
                        instruction.Operand = _il.ReadByte();
                        return;
                    }
                    instruction.Operand = (sbyte) _il.ReadByte();
                    return;

                case OperandType.ShortInlineR:
                    instruction.Operand = _il.ReadSingle();
                    return;

                case OperandType.ShortInlineVar:
                    instruction.Operand = GetVariable(instruction, _il.ReadByte());
                    return;
            }
            throw new NotSupportedException();
        }

        private void ResolveBranches()
        {
            foreach (var instruction in _instructions)
            {
                var operandType = instruction.OpCode.OperandType;
                if (operandType != OperandType.InlineBrTarget)
                {
                    if (operandType == OperandType.InlineSwitch)
                    {
                        goto Label_005A;
                    }
                    if (operandType != OperandType.ShortInlineBrTarget)
                    {
                        continue;
                    }
                }
                instruction.Operand = GetInstruction(_instructions, (int) instruction.Operand);
                continue;
                Label_005A:
                var numArray = (int[]) instruction.Operand;
                var instructionArray = new Instruction[numArray.Length];
                for (var i = 0; i < numArray.Length; i++)
                {
                    instructionArray[i] = GetInstruction(_instructions, numArray[i]);
                }
                instruction.Operand = instructionArray;
            }
        }

        private static bool TargetsLocalVariable(OpCode opcode)
        {
            return opcode.Name.Contains("loc");
        }
    }
}

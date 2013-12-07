using System.Collections.Generic;

namespace NDatabase.Reflection
{
    internal sealed class MatchContext
    {
        private readonly Dictionary<object, object> _data = new Dictionary<object, object>();

        internal MatchContext(Instruction instruction)
        {
            Reset(instruction);
        }

        public bool IsMatch { get; private set; }

        public Instruction Instruction { get; private set; }

        public bool Success
        {
            get { return IsMatch; }
            set { IsMatch = value; }
        }

        public void AddData(object key, object value)
        {
            _data.Add(key, value);
        }

        internal void Advance()
        {
            Instruction = Instruction.Next;
        }

        internal void Reset(Instruction instruction)
        {
            Instruction = instruction;
            IsMatch = true;
        }

        public bool TryGetData(object key, out object value)
        {
            return _data.TryGetValue(key, out value);
        }
    }
}
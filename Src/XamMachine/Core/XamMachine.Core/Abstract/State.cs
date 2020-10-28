using System;

namespace XamMachine.Core.Abstract
{
    public abstract class State<TEnum> where TEnum : Enum, new()
    {
        public TEnum Key { get; set; }

        protected State(TEnum @enum)
        {
            Key = @enum;
        }

        public abstract void Move(TEnum state);

        /// <summary> Add state case
        /// </summary>
        public abstract void Add(TEnum stat);

        public abstract void Release();

        public abstract void EnterState();

        public abstract void LeaveState();
    }
}
using System;
using System.Collections.Generic;
using System.Linq;

namespace XamMachine.Core.Abstract
{
    public abstract class BaseStateMachine<TEnum> : IDisposable         
        where TEnum : Enum, new()
    {

        public Dictionary<TEnum, State<TEnum>> _states = new Dictionary<TEnum, State<TEnum>>();

        public State<TEnum> _lastState;

        protected BaseStateMachine(State<TEnum>[] states)
        {
            InitStates(states);
        }

        protected BaseStateMachine(State<TEnum>[] states, TEnum initState)
        {
            InitStates(states);
            InitWith(initState);
        }

        private void InitStates(State<TEnum>[] states)
        {
            _states = states.ToDictionary(x => x.Key, y => y);
        }

        public void InitWith(TEnum state)
        {
            _lastState = _states[state];
            _lastState.EnterState();
        }

        public State<TEnum> CurrentState()
        {
            return _lastState;
        }

        public virtual void Move(TEnum state)
        {
            _lastState?.Move(state);
            _lastState = _states[state];
            _lastState.EnterState();   
        }

        public void Dispose()
        {
            foreach (var keyValuePair in _states)
            {
                keyValuePair.Value.Release();
            }

            _states = null;
        }
    }
}
using System;
using XamMachine.Core.Abstract;

namespace XamMachine.Example.Android.ViewModel.Main
{
    public class MainStateMachine<TEnum> : BaseStateMachine<TEnum> where TEnum : Enum, new()
    {
        public MainStateMachine(State<TEnum>[] states) : base(states)
        {
        }

        public MainStateMachine(State<TEnum>[] states, TEnum initState) : base(states, initState)
        {
        }
    }
}
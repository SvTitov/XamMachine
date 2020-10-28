using System;
using XamMachine.Core.Abstract;

namespace XamMachine.Example.Android.ViewModel.Main
{
    public class MainStateMachine<TEnum> : BaseStateMachine<MainViewModel, TEnum> where TEnum : Enum, new()
    {
        public MainStateMachine(State<TEnum>[] states) : base(states)
        {
        }

        public MainStateMachine(MainViewModel model, State<TEnum>[] states, TEnum initState) 
            : base(model, states, initState)
        {
        }
    }
}
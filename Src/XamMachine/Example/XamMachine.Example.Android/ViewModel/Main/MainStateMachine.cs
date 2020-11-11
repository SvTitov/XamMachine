using System;
using XamMachine.Core.Abstract;

namespace XamMachine.Example.Android.ViewModel.Main
{
    public class MainStateMachine<TEnum> : BaseStateMachine<MainViewModel, TEnum> where TEnum : Enum, new()
    {
        public MainStateMachine(MainViewModel model, TEnum initState) 
            : base(model, initState)
        {
        }
    }
}
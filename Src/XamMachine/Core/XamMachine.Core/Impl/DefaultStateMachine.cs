using System;
using System.ComponentModel;
using XamMachine.Core.Abstract;

namespace XamMachine.Core.Impl
{
    public class DefaultStateMachine<TViewModel, TEnum> : BaseStateMachine<TViewModel, TEnum>
        where TEnum : Enum, new()
        where TViewModel: class, INotifyPropertyChanged
    {
        public DefaultStateMachine(TViewModel viewModel, TEnum initState) 
            : base(viewModel, initState)
        {
        }

        public static DefaultStateMachine<TViewModel, TEnum> Produce(TViewModel context, TEnum eEnum)
        {
            return new DefaultStateMachine<TViewModel, TEnum>(context, eEnum);
        }
    }
}
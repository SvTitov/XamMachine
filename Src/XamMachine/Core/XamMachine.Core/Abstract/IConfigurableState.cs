using System;
using System.Linq.Expressions;

namespace XamMachine.Core.Abstract
{
    public interface IConfigurableState<TViewModel>
        where TViewModel: class
    {
        IConfigurableState<TViewModel> Action<TType>(Expression<Func<TViewModel, TType>> expression, TType value);

        IConfigurableState<TViewModel> Action(Expression<Action<TViewModel>> expression);

        IConfigurator<TViewModel> ActivationCondition(Expression<Func<TViewModel, bool>> expression);

        IConfigurableState<TViewModel> Next(params Enum[] eEnums);
    }

    public interface IConfigurator<TViewModel>
        where TViewModel : class
    {
        IConfigurableState<TViewModel> And(Expression<Func<TViewModel, bool>> expression);

        IConfigurableState<TViewModel> Or(Expression<Func<TViewModel, bool>> expression);
    }
}
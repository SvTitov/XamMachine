using System;
using System.Linq.Expressions;

namespace XamMachine.Core.Abstract
{
    public interface IConfigurableState<TViewModel>
        where TViewModel: class
    {
        IConfigurableState<TViewModel> WithContext(TViewModel model);

        IConfigurableState<TViewModel> For<TType>(Expression<Func<TViewModel, TType>> expression, TType value);

        void Build();
    }
}
using System;
using System.Linq.Expressions;

namespace XamMachine.Core.Abstract
{
    public interface IConfigurableState<TViewModel>
        where TViewModel: class
    {
        IConfigurableState<TViewModel> For<TType>(Expression<Func<TViewModel, TType>> expression, TType value);
    }
}
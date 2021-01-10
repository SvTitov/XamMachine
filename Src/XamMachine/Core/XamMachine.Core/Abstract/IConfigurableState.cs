﻿using System;
using System.Linq.Expressions;

namespace XamMachine.Core.Abstract
{
    public interface IConfigurableState<TViewModel>
        where TViewModel: class
    {
        IConfigurableState<TViewModel> For<TType>(Expression<Func<TViewModel, TType>> expression, TType value);

        IConfigurableState<TViewModel> For(Expression<Action<TViewModel>> expression);

        IConfigurableState<TViewModel> Next(Enum eEnum);
    }
}
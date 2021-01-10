﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;

namespace XamMachine.Core.Abstract
{
    public abstract class BaseStateMachine<TViewModel, TEnum> : IDisposable         
        where TEnum : Enum, new()
        where TViewModel : class, INotifyPropertyChanged
    {
        #region Fileds

        private TViewModel _viewModel;
        private readonly TEnum _initState;
        private bool _isSubscribed;

        private readonly Dictionary<string, List<ISourcePath<TEnum>>> _callbacksDictionary
            = new Dictionary<string, List<ISourcePath<TEnum>>>();

        private Dictionary<TEnum, ModelState<TViewModel>> _states = new Dictionary<TEnum, ModelState<TViewModel>>();

        protected ModelState<TViewModel> LastState;

        #endregion

        #region Ctor

        protected BaseStateMachine(TViewModel viewModel, TEnum initState)
        {
            _viewModel = viewModel;
            _initState = initState;
        }

        #endregion

        #region Public

        public void Build()
        {
            if (_states.TryGetValue(_initState, out LastState))
            {
                LastState.ActiveState();
            }
        }

        public ModelState<TViewModel> CurrentState()
        {
            return LastState;
        }

        public virtual void Move(TEnum state)
        {
            LastState?.Deactivate();
            LastState = _states[state];
            LastState.ActiveState();
        }

        public void Dispose()
        {
            foreach (var keyValuePair in _states)
            {
                keyValuePair.Value.Dispose();
            }

            _states.Clear();
            _states = null;

            _callbacksDictionary.Clear();
            _viewModel.PropertyChanged -= ContextOnPropertyChanged;
            _viewModel = null;
        }

        public IConfigurableState<TViewModel> ConfigureState(TEnum tEnum)
        {
            var state = new ModelState<TViewModel>(_viewModel, tEnum);
            _states.Add(tEnum, state);

            return state;
        }

        #endregion

        #region Combine

        public void ActOn<TType>(TViewModel context, Expression<Func<TViewModel, TType>> expression, Func<TType, bool> predicate, TEnum enumTrue)
        {
            SubscribePropertyChanged(context);

            var name = GetPropertyName(expression);

            var compiledExp = expression.Compile();

            var srcPath = CreateSourcePath(context, predicate, enumTrue, compiledExp);

            AddSourcePath(name, srcPath);
        }

        public void ForCombineAnd<TType>(TEnum currentEnum, params (Expression<Func<TViewModel, TType>> expression, Func<TType, bool> predicate)[] tuples)
        {
            Combine(CombineType.And, currentEnum, tuples);
        }

        public void ForCombineOr<TType>(TEnum currentEnum, params (Expression<Func<TViewModel, TType>> expression, Func<TType, bool> predicate)[] tuples)
        {
            Combine(CombineType.Or, currentEnum, tuples);
        }

        private void Combine<TType>(CombineType combineType, TEnum currentEnum,
            (Expression<Func<TViewModel, TType>> expression, Func<TType, bool> predicate)[] tuples)
        {
            Expression lastExpression = null;
            foreach (var valueTuple in tuples)
            {
                var callbackExpression = Expression.Invoke(valueTuple.expression, Expression.Constant(_viewModel));

                var trueExpression = Expression.Equal(Expression.Call(valueTuple.predicate.Method, callbackExpression),
                    Expression.Constant(true));

                lastExpression = lastExpression == null ? trueExpression : CreateCombineExpression(combineType, trueExpression, lastExpression);
            }

            var predicate = Expression.Lambda<Func<bool>>(lastExpression).Compile();

            ActOnMultiple(_viewModel, currentEnum, predicate, tuples.Select(x => x.expression).ToArray());
        }

        private BinaryExpression CreateCombineExpression(CombineType type, BinaryExpression trueExpression, Expression last)
        {
            switch (type)
            {
                case CombineType.And:
                    return Expression.AndAlso(trueExpression, last);
                case CombineType.Or:
                    return Expression.Or(trueExpression, last);
            }

            throw new ArgumentException($"{nameof(CombineType)} should be declared!");
        }

        #endregion

        #region Private

        private void ActOnMultiple<TType>(TViewModel context, TEnum enEnum, Func<bool> predicate, params Expression<Func<TViewModel, TType>>[] expression)
        {
            SubscribePropertyChanged(context);

            foreach (var expression1 in expression)
            {
                var name = GetPropertyName(expression1);
                var compiledExp = expression1.Compile();
                var srcPath = CreateSourcePath(context, predicate, enEnum, compiledExp);
                AddSourcePath(name, srcPath);
            }
        }

        private SourcePathMulti<TViewModel, TType, TEnum> CreateSourcePath<TType>(TViewModel context, Func<bool> predicate, TEnum enumTrue, Func<TViewModel, TType> compiled)
        {
            var srcPath = new SourcePathMulti<TViewModel, TType, TEnum>(context)
            {
                PropertyCache = compiled,
                Predicate = predicate,
                Case = enumTrue,
            };

            return srcPath;
        }

        private SourcePath<TViewModel, TType, TEnum> CreateSourcePath<TType>(TViewModel context, Func<TType, bool> predicate, TEnum enumTrue, Func<TViewModel, TType> compiled)
        {
            var srcPath = new SourcePath<TViewModel, TType, TEnum>(context)
            {
                PropertyCache = compiled,
                Predicate = predicate,
                Case = enumTrue,
            };

            return srcPath;
        }

        private string GetPropertyName<TType>(Expression<Func<TViewModel, TType>> expression)
        {
            var name = (expression.Body as MemberExpression)?.Member.Name;

            if (string.IsNullOrEmpty(name))
                throw new Exception("Cannot get a property name!");
            return name;
        }

        private void SubscribePropertyChanged(TViewModel context)
        {
            if (!_isSubscribed)
            {
                context.PropertyChanged += ContextOnPropertyChanged;
                _isSubscribed = true;
            }
        }

        private void AddSourcePath(string name, ISourcePath<TEnum> srcPath)
        {
            if (_callbacksDictionary.TryGetValue(name, out var value))
                value.Add(srcPath);
            else
                _callbacksDictionary.Add(name, new List<ISourcePath<TEnum>> {srcPath});
        }

        private void ContextOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var name = e.PropertyName;

            if (!_callbacksDictionary.TryGetValue(name, out var expressionSourcePaths)) 
                return;

            var currentKey = CurrentState();
            var isValid = false;
            TEnum navigateTo = default;

            var nextPath = expressionSourcePaths.FirstOrDefault(x => x.Case.Equals(currentKey.NextState));
            if (nextPath != null)
            {
                navigateTo = ProcessSourcePath(nextPath, ref isValid);
            }
            else
            {
                foreach (var sourcePath in expressionSourcePaths.Where(item => !currentKey.State.Equals(item.Case))) //exclude current state
                {
                    navigateTo = ProcessSourcePath(sourcePath, ref isValid);
                }
            }

            if (isValid)
            {
                Move(navigateTo);
            }
        }

        private static TEnum ProcessSourcePath(ISourcePath<TEnum> nextPath, ref bool isValid)
        {
            var result = nextPath.Invoke();
            //SetSearchValue(ref isValid, result);
            if (result)
                isValid = true;

            return nextPath.Case;
        }

        protected void SetSearchValue(ref bool? result, bool searchResult)
        {
            if (result.HasValue)
                result = result.Value && searchResult;
            else
                result = searchResult;
        }

        #endregion
    }
}
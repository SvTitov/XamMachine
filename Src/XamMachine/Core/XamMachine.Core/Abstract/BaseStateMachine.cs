using System;
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
            var state = new ModelState<TViewModel>(tEnum);
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

        public void CombineAnd<TType>(TViewModel context, TEnum eEnum, params (Expression<Func<TViewModel, TType>> expression, Func<TType, bool> predicate)[] tuples)
        {
            Combine(context, CombineType.And, eEnum, tuples);
        }

        public void CombineOr<TType>(TViewModel context, TEnum eEnum, params (Expression<Func<TViewModel, TType>> expression, Func<TType, bool> predicate)[] tuples)
        {
            Combine(context, CombineType.Or, eEnum, tuples);
        }

        private void Combine<TType>(TViewModel context, CombineType combineType, TEnum eEnum,
            (Expression<Func<TViewModel, TType>> expression, Func<TType, bool> predicate)[] tuples)
        {
            Expression last = null;
            foreach (var valueTuple in tuples)
            {
                var callbackExpression = Expression.Invoke(valueTuple.expression, Expression.Constant(context));

                var trueExpression = Expression.Equal(Expression.Call(valueTuple.predicate.Method, callbackExpression),
                    Expression.Constant(true));

                last = last == null ? trueExpression : CreateCombineExpression(combineType, trueExpression, last);
            }

            var predicate = Expression.Lambda<Func<bool>>(last).Compile();

            ActOnMultiple(context, eEnum, predicate, tuples.Select(x => x.expression).ToArray());
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
                CaseIfTrue = enumTrue,
            };

            return srcPath;
        }

        private SourcePath<TViewModel, TType, TEnum> CreateSourcePath<TType>(TViewModel context, Func<TType, bool> predicate, TEnum enumTrue, Func<TViewModel, TType> compiled)
        {
            var srcPath = new SourcePath<TViewModel, TType, TEnum>(context)
            {
                PropertyCache = compiled,
                Predicate = predicate,
                CaseIfTrue = enumTrue,
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

            var currentKey = CurrentState().State;
            bool? isValid = null;
            TEnum navigateTo = default;

            foreach (var sourcePath in expressionSourcePaths)
            {
                if (!currentKey.Equals(sourcePath.CaseIfTrue))
                {
                    var result = sourcePath.Invoke();
                    navigateTo = sourcePath.CaseIfTrue;
                    SetSearchValue(ref isValid, result);
                }
            }

            if (isValid.HasValue && isValid.Value)
            {
                Move(navigateTo);
            }
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
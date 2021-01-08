using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;

namespace XamMachine.Core.Abstract
{
    public abstract class BaseStateMachine<TViewModel, TEnum> : IDisposable         
        where TEnum : Enum, new()
        where TViewModel : class, INotifyPropertyChanged
    {
        private TViewModel _viewModel;
        private readonly TEnum _initState;
        private bool _isSubscribed;


        private readonly Dictionary<string, List<ISourcePath<TEnum>>> _callbacksDictionary
            = new Dictionary<string, List<ISourcePath<TEnum>>>();

        private Dictionary<TEnum, CState<TViewModel>> _states = new Dictionary<TEnum, CState<TViewModel>>();


        protected CState<TViewModel> LastState;

        protected BaseStateMachine(TViewModel viewModel, TEnum initState)
        {
            _viewModel = viewModel;
            _initState = initState;
        }

        public void Build()
        {
            if (_states.TryGetValue(_initState, out LastState))
            {
                LastState.ActiveState();
            }
        }

        public CState<TViewModel> CurrentState()
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

        public void ActOn<TType>(TViewModel context, Expression<Func<TViewModel, TType>> expression, Func<TType, bool> predicate, TEnum enumTrue)
        {
            SubscribePropertyChanged(context);

            var name = GetPropertyName(expression);

            var compiledExp = expression.Compile();

            var srcPath = CreateSourcePath(context, predicate, enumTrue, compiledExp);

            AddSourcePath(name, srcPath);
        }

        public void CombineActOn<TType>(TViewModel context, TEnum eEnum, params (Expression<Func<TViewModel, TType>> expression, Func<TType, bool> predicate)[] tuples)
        {
            Expression last = null;
            foreach (var valueTuple in tuples)
            {
                var callbackExpression = Expression.Invoke(valueTuple.expression, Expression.Constant(context));

                var trueExpression = Expression.Equal(Expression.Call(valueTuple.predicate.Method, callbackExpression), Expression.Constant(true));

                if (last == null)
                {
                    last = trueExpression;
                }
                else
                {
                    last = Expression.AndAlso(trueExpression, last);
                }
            }

            var predicate = Expression.Lambda<Func<bool>>(last).Compile();

            ActOnMultiple(context, eEnum, predicate, tuples.Select(x => x.expression).ToArray());
        }

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

        public IConfigurableState<TViewModel> ConfigureState(TEnum tEnum)
        {
            var state = new CState<TViewModel>(tEnum);
            _states.Add(tEnum, state);

            return state;
        }
    }

    public interface IConfigurableState<TViewModel>
        where TViewModel: class
    {
        IConfigurableState<TViewModel> WithContext(TViewModel model);

        IConfigurableState<TViewModel> For<TType>(Expression<Func<TViewModel, TType>> expression, TType value);

        void Build();
    }

    public class CState<TViewModel> : IConfigurableState<TViewModel>, IDisposable
        where TViewModel : class
    {
        private WeakReference _context;

        private List<Action> _callbacks = new List<Action>();

        public Enum State { get; }

        public CState(Enum @enum)
        {
            State = @enum;
        }

        protected TViewModel RetrieveContext()
        {
            return (TViewModel) _context.Target;
        }

        public IConfigurableState<TViewModel> WithContext(TViewModel model)
        {
            _context = new WeakReference(model);
            return this;
        }

        public IConfigurableState<TViewModel> For<TType>(Expression<Func<TViewModel, TType>> expression, TType value)
        {
            if (expression.Body is MemberExpression memberExpression)
            {
                if (memberExpression.Member is PropertyInfo propertyInfo)
                {
                    _callbacks.Add(() =>
                    {
                        propertyInfo.SetValue(RetrieveContext(), value, null);
                    });
                }
            }
     
            return this;
        }

        public void ActiveState()
        {
            _callbacks.ForEach(x => x.Invoke());
        }

        public void Deactivate()
        {

        }

        public void Build()
        {
            
        }

        public void Dispose()
        {
            _callbacks.Clear();
            _callbacks = null;
        }
    }
}
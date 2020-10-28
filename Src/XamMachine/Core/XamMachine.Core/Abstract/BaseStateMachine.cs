using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;

namespace XamMachine.Core.Abstract
{
    public abstract class BaseStateMachine<TViewModel, TEnum> : IDisposable         
        where TEnum : Enum, new()
        where TViewModel : class, INotifyPropertyChanged
    {
        private TViewModel _viewModel;
        private bool _isSubscribed;


        public Dictionary<TEnum, State<TEnum>> _states = new Dictionary<TEnum, State<TEnum>>();

        private readonly Dictionary<string, List<ISourcePath<TEnum>>> _testDictionary
            = new Dictionary<string, List<ISourcePath<TEnum>>>();

        public State<TEnum> _lastState;

        protected BaseStateMachine(State<TEnum>[] states)
        {
            InitStates(states);
        }

        protected BaseStateMachine(TViewModel viewModel, State<TEnum>[] states, TEnum initState)
        {
            _viewModel = viewModel;

            InitStates(states);
            InitWith(initState);
        }

        private void InitStates(State<TEnum>[] states)
        {
            _states = states.ToDictionary(x => x.Key, y => y);
        }

        public void InitWith(TEnum state)
        {
            _lastState = _states[state];
            _lastState.EnterState();
        }

        public State<TEnum> CurrentState()
        {
            return _lastState;
        }

        public virtual void Move(TEnum state)
        {
            _lastState?.LeaveState();
            _lastState = _states[state];
            _lastState.EnterState();   
        }

        public void Dispose()
        {
            foreach (var keyValuePair in _states)
            {
                keyValuePair.Value.Release();
            }

            _states.Clear();
            _states = null;

            _testDictionary.Clear();
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
                _isSubscribed = false;
            }
        }

        private void AddSourcePath<TType>(string name, SourcePath<TViewModel, TType, TEnum> srcPath)
        {
            if (_testDictionary.TryGetValue(name, out var value))
                value.Add(srcPath);
            else
                _testDictionary.Add(name, new List<ISourcePath<TEnum>> {srcPath});
        }

        private void ContextOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var name = e.PropertyName;

            if (!_testDictionary.TryGetValue(name, out var results)) 
                return;

            foreach (var sourcePath in results)
            {
                var currentKey = this.CurrentState().Key;
                if (!currentKey.Equals(sourcePath.CaseIfTrue))
                {
                    if (sourcePath.Invoke())
                    {
                        Move(sourcePath.CaseIfTrue);
                    }
                }
            }
        }
    }
}
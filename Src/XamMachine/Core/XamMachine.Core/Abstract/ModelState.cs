using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace XamMachine.Core.Abstract
{
    public class ModelState<TViewModel> : IConfigurableState<TViewModel>, IDisposable
        where TViewModel : class
    {
        private WeakReference _context;

        private List<Action> _callbacks = new List<Action>();

        public Enum State { get; }

        public Enum NextState { get; protected set; }

        public ModelState(TViewModel model,Enum @enum)
        {
            State = @enum;
            _context = new WeakReference(model);
        }

        protected TViewModel RetrieveContext()
        {
            return (TViewModel) _context.Target;
        }

        public IConfigurableState<TViewModel> Action<TType>(Expression<Func<TViewModel, TType>> expression, TType value)
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

        public IConfigurableState<TViewModel> Next(Enum eEnum)
        {
            NextState = eEnum;
            return this;
        }

        public IConfigurableState<TViewModel> Action(Expression<Action<TViewModel>> expression)
        {
            var @delegate = (Func<Action<TViewModel>>)Expression.Lambda(expression).Compile();
            var callback = @delegate();

            _callbacks.Add(() => { callback(RetrieveContext()); });

            return this;
        }

        public void ActiveState()
        {
            _callbacks.ForEach(x => x.Invoke());
        }

        public void Deactivate()
        {

        }

        public void Dispose()
        {
            _callbacks.Clear();
            _callbacks = null;
        }
    }
}
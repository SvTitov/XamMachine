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

        public ModelState(Enum @enum)
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
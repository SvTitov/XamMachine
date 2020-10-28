using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace XamMachine.Core.Abstract
{
    public class SourcePath<TContext, TProperty, TEnum> : ISourcePath<TEnum> 
        where TContext : class
        where TEnum : Enum
    {
        private readonly WeakReference _contextReference;

        public SourcePath(object context)
        {
            _contextReference = new WeakReference(context);
        }

        public Func<TContext, TProperty> PropertyCache { get; set; }

        public Func<TProperty, bool> Predicate { get; set; }

        public TEnum CaseIfTrue { get; set; }

        public TContext Context
        {
            get
            {
                if (_contextReference.IsAlive)
                    return (TContext) _contextReference.Target;

                return null;
            }
        }

        public bool Invoke()
        {
            var value = PropertyCache.Invoke(Context);
            return Predicate.Invoke(value);
        }
    }

    public interface ISourcePath<TEnum>
        where TEnum : Enum
    {
        bool Invoke();
        TEnum CaseIfTrue { get; set; }
    }
}
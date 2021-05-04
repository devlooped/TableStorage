using System;
using System.Threading.Tasks;

namespace Devlooped
{
    // See https://devblogs.microsoft.com/pfxteam/asynclazyt/
    class AsyncLazy<T> : Lazy<Task<T>>
    {
        public AsyncLazy(Func<T> valueFactory) : base(() => Task.Run(valueFactory))
        { }

        public AsyncLazy(Func<Task<T>> asyncValueFactory) : base(() => Task.Run(async () => await asyncValueFactory()))
        { }
    }
}

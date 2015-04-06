﻿/*
The MIT License (MIT)

Copyright (c) 2014 Maksim Volkau

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

namespace DryIoc
{
    using System;
    using System.Runtime.Remoting.Messaging;
    using System.Diagnostics.CodeAnalysis;

    partial class Container
    {
        [SuppressMessage("ReSharper", "RedundantAssignment")]
        static partial void GetDefaultScopeContext(ref IScopeContext resultContext)
        {
            resultContext = new AsyncExecutionFlowScopeContext();
        }
    }

    /// <summary>Stores scopes propagating through async-await boundaries.</summary>
    public sealed class AsyncExecutionFlowScopeContext : IScopeContext
    {
        /// <summary>Statically known name of root scope in this context.</summary>
        public static readonly object ROOT_SCOPE_NAME = typeof(AsyncExecutionFlowScopeContext);

        /// <summary>Name associated with context root scope - so the reuse may find scope context.</summary>
        public object RootScopeName { get { return ROOT_SCOPE_NAME; } }

        /// <summary>Returns current scope or null if no ambient scope available at the moment.</summary>
        /// <returns>Current scope or null.</returns>
        public IScope GetCurrentOrDefault()
        {
            var scope = (Copyable<IScope>)CallContext.LogicalGetData(_key);
            return scope == null ? null : scope.Value;
        }

        /// <summary>Changes current scope using provided delegate. Delegate receives current scope as input and  should return new current scope.</summary>
        /// <param name="getNewCurrentScope">Delegate to change the scope.</param>
        /// <remarks>Important: <paramref name="getNewCurrentScope"/> may be called multiple times in concurrent environment.
        /// Make it predictable by removing any side effects.</remarks>
        /// <returns>New current scope. So it is convenient to use method in "using (var newScope = ctx.SetCurrent(...))".</returns>
        public IScope SetCurrent(Func<IScope, IScope> getNewCurrentScope)
        {
            var oldScope = GetCurrentOrDefault();
            var newScope = getNewCurrentScope.ThrowIfNull()(oldScope);
            CallContext.LogicalSetData(_key, new Copyable<IScope>(newScope));
            return newScope;
        }

        #region Implementation

        private static readonly string _key = typeof(AsyncExecutionFlowScopeContext).Name;

        [Serializable]
        private sealed class Copyable<T> : MarshalByRefObject
        {
            public readonly T Value;

            public Copyable(T value)
            {
                Value = value;
            }
        }

        #endregion
    }
}

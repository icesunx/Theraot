﻿using System;

using Theraot.Collections;
using Theraot.Threading.Needles;

namespace Theraot.Threading
{
    [global::System.Diagnostics.DebuggerNonUserCode]
    public sealed class ReentryGuard
    {
        private NotNull<NoTrackingThreadLocal<Tuple<ExtendedQueue<Action>, Guard>>> _workQueue;

        public ReentryGuard()
        {
            _workQueue = new NotNull<NoTrackingThreadLocal<Tuple<ExtendedQueue<Action>, Guard>>>
                (
                    new NoTrackingThreadLocal<Tuple<ExtendedQueue<Action>, Guard>>
                    (
                        () => new Tuple<ExtendedQueue<Action>, Guard>(new ExtendedQueue<Action>(), new Guard())
                    )
                );
        }

        public IPromise Execute(Action action)
        {
            var local = _workQueue.Value.Value;
            var result = AddExecution(action, local);
            IDisposable engagement;
            if (local.Item2.Enter(out engagement))
            {
                using (engagement)
                {
                    ExecutePending(local);
                }
            }
            return result;
        }

        public IPromise<T> Execute<T>(Func<T> action)
        {
            var local = _workQueue.Value.Value;
            var result = AddExecution<T>(action, local);
            IDisposable engagement;
            if (local.Item2.Enter(out engagement))
            {
                using (engagement)
                {
                    ExecutePending(local);
                }
            }
            return result;
        }

        private static IPromise AddExecution(Action action, Tuple<ExtendedQueue<Action>, Guard> local)
        {
            IPromised promised;
            var result = new PromiseNeedle(out promised, false);
            local.Item1.Add
            (
                () =>
                {
                    try
                    {
                        action.Invoke();
                        promised.OnCompleted();
                    }
                    catch (Exception exception)
                    {
                        promised.OnError(exception);
                    }
                }
            );
            return result;
        }

        private static IPromise<T> AddExecution<T>(Func<T> action, Tuple<ExtendedQueue<Action>, Guard> local)
        {
            IPromised<T> promised;
            var result = new PromiseNeedle<T>(out promised, false);
            local.Item1.Add
            (
                () =>
                {
                    try
                    {
                        promised.OnNext(action.Invoke());
                    }
                    catch (Exception exception)
                    {
                        promised.OnError(exception);
                    }
                }
            );
            return result;
        }

        private static Action ExecutePending(Tuple<ExtendedQueue<Action>, Guard> local)
        {
            Action action;
            while (local.Item1.TryTake(out action))
            {
                action.Invoke();
            }
            return action;
        }
    }
}
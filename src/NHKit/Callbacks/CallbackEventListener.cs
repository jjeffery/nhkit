using NHibernate.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NHKit.Callbacks
{
    internal class CallbackEventListener : IPostInsertEventListener, IPostUpdateEventListener, IPostDeleteEventListener
    {
        private readonly IDictionary<Type, CallbackHandler[]> _callbackHandlers = new Dictionary<Type, CallbackHandler[]>();
        private readonly CallbackTransactionManager _transactionManager = new CallbackTransactionManager();

        public CallbackEventListener(IEnumerable<CallbackHandler> callbackHandlers)
        {
            foreach (var group in callbackHandlers.GroupBy(c => c.EntityType)) {
                var entityType = group.Key;
                var classes = group.ToArray();
                _callbackHandlers.Add(entityType, classes);
            }
        }

        public Task OnPostInsertAsync(PostInsertEvent @event, CancellationToken cancellationToken)
        {
            OnPostInsert(@event);
            return Task.CompletedTask;
        }

        public void OnPostInsert(PostInsertEvent @event)
        {
            var callbackHandlers = GetCallbackHandlers(@event);
            if (callbackHandlers == null) {
                return;
            }
            var processor = _transactionManager.Get(@event.Session);
            foreach (var callbackHandler in callbackHandlers) {
                processor.Add(new InsertWorkUnit(@event, callbackHandler));
            }
        }

        public Task OnPostUpdateAsync(PostUpdateEvent @event, CancellationToken cancellationToken)
        {
            OnPostUpdate(@event);
            return Task.CompletedTask;
        }

        public void OnPostUpdate(PostUpdateEvent @event)
        {
            var callbackHandlers = GetCallbackHandlers(@event);
            if (callbackHandlers == null) {
                return;
            }
            var processor = _transactionManager.Get(@event.Session);
            foreach (var callbackHandler in callbackHandlers) {
                processor.Add(new UpdateWorkUnit(@event, callbackHandler));
            }
        }

        public Task OnPostDeleteAsync(PostDeleteEvent @event, CancellationToken cancellationToken)
        {
            OnPostDelete(@event);
            return Task.CompletedTask;
        }

        public void OnPostDelete(PostDeleteEvent @event)
        {
            var callbackHandlers = GetCallbackHandlers(@event);
            if (callbackHandlers == null) {
                return;
            }
            var processor = _transactionManager.Get(@event.Session);
            foreach (var callbackHandler in callbackHandlers) {
                processor.Add(new DeleteWorkUnit(@event, callbackHandler));
            }
        }

        private CallbackHandler[] GetCallbackHandlers(AbstractPostDatabaseOperationEvent @event)
        {
            if (@event.Persister.MappedClass != null) {
                if (_callbackHandlers.TryGetValue(@event.Persister.MappedClass, out var callbackHandlers)) {
                    return callbackHandlers;
                }
            }

            return null;
        }
    }
}

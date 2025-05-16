using NHibernate;
using NHibernate.Action;
using NHibernate.Event;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NHKit.Callbacks
{
    internal class CallbackTransactionManager
    {
        private readonly ConcurrentDictionary<ITransaction, CallbackTransactionProcessor> _processors =
            new ConcurrentDictionary<ITransaction, CallbackTransactionProcessor>();

        public CallbackTransactionProcessor Get(IEventSource session)
        {
            var transaction = session.GetCurrentTransaction();
            if (transaction == null) {
                throw new InvalidOperationException("callback: transaction is required");
            }

            return _processors.GetOrAdd(transaction, tx => {
                var processor = new CallbackTransactionProcessor();

                // Register with the session so that the transaction processor is invoked
                // just prior to the transaction commit, and that it is cleaned up after the transaction
                // has completed.
                var transactionCompletionProcess = new TransactionCompletionProcess(_processors, tx);
                session.ActionQueue.RegisterProcess((IBeforeTransactionCompletionProcess)transactionCompletionProcess);
                session.ActionQueue.RegisterProcess((IAfterTransactionCompletionProcess)transactionCompletionProcess);

                return processor;
            });
        }

        private class TransactionCompletionProcess : IBeforeTransactionCompletionProcess,
            IAfterTransactionCompletionProcess
        {
            private readonly IDictionary<ITransaction, CallbackTransactionProcessor> _processors;
            private readonly ITransaction _transaction;

            public TransactionCompletionProcess(
                IDictionary<ITransaction, CallbackTransactionProcessor> processors,
                ITransaction transaction)
            {
                _processors = processors;
                _transaction = transaction;
            }

            public void ExecuteBeforeTransactionCompletion()
            {
                if (_processors.TryGetValue(_transaction, out var processor)) {
                    processor.DoBeforeTransactionCompletion();
                }
            }

            public Task ExecuteBeforeTransactionCompletionAsync(CancellationToken cancellationToken)
            {
                if (_processors.TryGetValue(_transaction, out var processor)) {
                    return processor.DoBeforeTransactionCompletionAsync(cancellationToken);
                }

                return Task.CompletedTask;
            }

            public void ExecuteAfterTransactionCompletion(bool success)
            {
                _processors.Remove(_transaction);
            }

            public Task ExecuteAfterTransactionCompletionAsync(bool success, CancellationToken cancellationToken)
            {
                ExecuteAfterTransactionCompletion(success);
                return Task.CompletedTask;
            }
        }
    }
}

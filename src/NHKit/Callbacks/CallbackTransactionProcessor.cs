using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace NHKit.Callbacks
{
    internal class CallbackTransactionProcessor
    {
        private readonly ConcurrentDictionary<Tuple<string, object, CallbackHandler>, WorkUnit> _workUnits =
            new ConcurrentDictionary<Tuple<string, object, CallbackHandler>, WorkUnit>();

        public void Add(WorkUnit workUnit)
        {
            WorkUnit AddValue(Tuple<string, object, CallbackHandler> key) => workUnit;
            WorkUnit UpdateValue(Tuple<string, object, CallbackHandler> key, WorkUnit previousWorkUnit) => previousWorkUnit.Merge(workUnit);
            _workUnits.AddOrUpdate(workUnit.Key, AddValue, UpdateValue);
        }

        public void DoBeforeTransactionCompletion()
        {
            foreach (var kv in _workUnits) {
                var workUnit = kv.Value;
                workUnit.DoWork();
            }
        }

        public async Task DoBeforeTransactionCompletionAsync(CancellationToken cancel)
        {
            foreach (var kv in _workUnits) {
                var workUnit = kv.Value;
                await workUnit.DoWorkAsync(cancel);
            }
        }
    }
}

using NHibernate.Event;
using NHKit.Internal;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NHKit.Callbacks
{
    internal abstract class WorkUnit
    {
        public readonly Tuple<string, object, CallbackHandler> Key;
        public readonly IEventSource Session;
        public readonly object Entity;
        public readonly object Id;
        public readonly CallbackHandler CallbackHandler;

        protected WorkUnit(AbstractPostDatabaseOperationEvent @event, CallbackHandler callbackHandler)
        {
            Session = @event.Session;
            Entity = @event.Entity;
            Id = @event.Id;
            Key = Tuple.Create(@event.Persister.EntityName, Id, callbackHandler);
            CallbackHandler = callbackHandler;
        }

        public WorkUnit Merge(WorkUnit second)
        {
            if (second is InsertWorkUnit ins) {
                return MergeInsert(ins);
            }

            if (second is UpdateWorkUnit upd) {
                return MergeUpdate(upd);
            }

            if (second is DeleteWorkUnit del) {
                return MergeDelete(del);
            }

            throw new ArgumentException("unknown work unit type");
        }

        public abstract void DoWork();

        public virtual Task DoWorkAsync(CancellationToken cancel)
        {
            DoWork();
            return Task.CompletedTask;
        }

        protected abstract WorkUnit MergeInsert(InsertWorkUnit second);
        protected abstract WorkUnit MergeUpdate(UpdateWorkUnit second);
        protected abstract WorkUnit MergeDelete(DeleteWorkUnit second);
    }

    internal class InsertWorkUnit : WorkUnit
    {
        public object[] State { get; private set; }

        public InsertWorkUnit(PostInsertEvent @event, CallbackHandler callbackHandler) : base(@event, callbackHandler)
        {
            State = Tools.CloneArray(@event.State);
        }

        public override void DoWork()
        {
            if (CallbackHandler.InsertedCallbackMethod != null) {
                var entityInfo = CallbackHandler.CreateInsertedEntityInfo(Session, Entity);
                if (CallbackHandler.HandlerIsEntity) {
                    CallbackHandler.InsertedCallbackMethod.Invoke(Entity, new[] { entityInfo });
                }
                // TODO: instantiate handler from dependency injection container
            }
        }

        protected override WorkUnit MergeInsert(InsertWorkUnit second)
        {
            return second;
        }

        protected override WorkUnit MergeUpdate(UpdateWorkUnit second)
        {
            var clone = (InsertWorkUnit)MemberwiseClone();
            clone.State = Tools.CloneArray(second.State);
            return clone;
        }

        protected override WorkUnit MergeDelete(DeleteWorkUnit second)
        {
            return second;
        }
    }

    internal class UpdateWorkUnit : WorkUnit
    {
        public object[] OldState { get; private set; }
        public object[] State { get; private set; }

        public UpdateWorkUnit(PostUpdateEvent @event, CallbackHandler callbackHandler) : base(@event, callbackHandler)
        {
            OldState = Tools.CloneArray(@event.OldState);
            State = Tools.CloneArray(@event.State);
        }

        public override void DoWork()
        {
            if (CallbackHandler.UpdatedCallbackMethod != null) {
                var entityInfo = CallbackHandler.CreateUpdatedEntityInfo(Session, Entity, OldState, State);
                if (CallbackHandler.HandlerIsEntity) {
                    CallbackHandler.UpdatedCallbackMethod.Invoke(Entity, new [] { entityInfo });
                }
                // TODO: instantiate handler from dependency injection container
            }
        }

        protected override WorkUnit MergeInsert(InsertWorkUnit second)
        {
            // should not happen
            return second;
        }

        protected override WorkUnit MergeUpdate(UpdateWorkUnit second)
        {
            var clone = (UpdateWorkUnit)MemberwiseClone();
            clone.State = Tools.CloneArray(second.State);
            return clone;
        }

        protected override WorkUnit MergeDelete(DeleteWorkUnit second)
        {
            return second;
        }

        public UpdateWorkUnit WithOldState(object[] oldState)
        {
            var clone = (UpdateWorkUnit)MemberwiseClone();
            clone.OldState = oldState;
            return clone;
        }
    }

    internal class DeleteWorkUnit : WorkUnit
    {
        public object[] DeletedState { get; private set; }

        public DeleteWorkUnit(PostDeleteEvent @event, CallbackHandler callbackHandler) : base(@event, callbackHandler)
        {
            DeletedState = Tools.CloneArray(@event.DeletedState);
        }

        public override void DoWork()
        {
            // Does nothing at the moment
        }

        protected override WorkUnit MergeInsert(InsertWorkUnit second)
        {
            // Could happen if inserted, deleted and then re-inserted
            return second;
        }

        protected override WorkUnit MergeUpdate(UpdateWorkUnit second)
        {
            return second.WithOldState(DeletedState);
        }

        protected override WorkUnit MergeDelete(DeleteWorkUnit second)
        {
            return second;
        }
    }
}

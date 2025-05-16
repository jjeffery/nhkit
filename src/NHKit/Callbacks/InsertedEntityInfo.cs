using NHibernate;

namespace NHKit.Callbacks
{
    internal class InsertedEntityInfo<TEntity> : IInsertedEntityInfo<TEntity>
    {
        public ISession Session { get; }
        public TEntity Entity { get; }

        public InsertedEntityInfo(ISession session, TEntity entity)
        {
            Session = session;
            Entity = entity;
        }
    }
}

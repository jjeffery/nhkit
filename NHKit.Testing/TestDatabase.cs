using JetBrains.Annotations;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Event;
using NHibernate.Persister.Entity;
using System.Data;
using System.Data.Common;
using System.Runtime.CompilerServices;

namespace NHKit.Testing;

public abstract class TestDatabase : IDisposable
{
    public string ConnectionString { get; }
    public ISessionFactory SessionFactory { get; }

    private readonly HashSet<string> _modifiedTableNames = new();
    private static readonly ConditionalWeakTable<ISession, TestDatabase> Sessions = new();
    private readonly Dictionary<Type, object> _fixtures = new Dictionary<Type, object>();

    protected TestDatabase()
    {
        // ReSharper disable VirtualMemberCallInConstructor
        var connectionString = GetConnectionString();
        if (!IsTestDatabase(connectionString)) {
            throw new InvalidOperationException("Not a test database");
        }

        var nhConfiguration = CreateConfiguration(connectionString);
        AddListeners(nhConfiguration);
        // ReSharper restore VirtualMemberCallInConstructor
        
        ConnectionString = connectionString;
        SessionFactory = nhConfiguration.BuildSessionFactory();
        TruncateTables();
    }


    public virtual void Dispose()
    {
        SessionFactory.Dispose();
    }

    public ISession OpenSession()
    {
        var session = SessionFactory.OpenSession();
        Sessions.AddOrUpdate(session, this);
        return session;
    }

    public static TestDatabase FromSession(ISession session)
    {
        if (Sessions.TryGetValue(session, out var testDatabase)) {
            return testDatabase;
        }

        throw new ArgumentException("Session is not associated with a test database", nameof(session));
    }
    
    public TFixture SetFixture<TFixture>([NotNull] TFixture fixture) where TFixture : class
    {
        _fixtures[typeof(TFixture)] = fixture;
        return fixture;
    }

    [CanBeNull]
    public TFixture GetFixture<TFixture>() where TFixture : class
    {
        var fixture = _fixtures.GetValueOrDefault(typeof(TFixture)) as TFixture;
        return fixture;
    }

    public void Transaction(Action<ISession> action)
    {
        using var session = OpenSession();
        using var tx = session.BeginTransaction();
        action(session);
        tx.Commit();
    }

    public TResult Transaction<TResult>(Func<ISession, TResult> func)
    {
        using var session = OpenSession();
        using var tx = session.BeginTransaction();
        var t = func(session);
        tx.Commit();
        return t;
    }

    public async Task TransactionAsync(Func<ISession, Task> action)
    {
        using var session = OpenSession();
        using var tx = session.BeginTransaction();
        await action(session);
        await tx.CommitAsync();
    }

    public async Task<T> TransactionAsync<T>(Func<ISession, Task<T>> func)
    {
        using var session = OpenSession();
        using var tx = session.BeginTransaction();
        var t = await func(session);
        await tx.CommitAsync();
        return t;
    }

    public void RollbackTransaction(Action<ISession> action)
    {
        using var session = OpenSession();
        using var tx = session.BeginTransaction();
        action(session);
        tx.Rollback();
    }

    public TResult RollbackTransaction<TResult>(Func<ISession, TResult> func)
    {
        using var session = OpenSession();
        using var tx = session.BeginTransaction();
        var result = func(session);
        tx.Rollback();
        return result;
    }

    public async Task RollbackTransactionAsync(Func<ISession, Task> action)
    {
        using var session = OpenSession();
        using var tx = session.BeginTransaction();
        await action(session);
        await tx.RollbackAsync();
    }

    public async Task<T> RollbackTransactionAsync<T>(Func<ISession, Task<T>> func)
    {
        using var session = OpenSession();
        using var tx = session.BeginTransaction();
        var t = await func(session);
        await tx.RollbackAsync();
        return t;
    }

    protected abstract string GetConnectionString();

    protected abstract bool IsTestDatabase(string connectionString);

    protected abstract Configuration CreateConfiguration(string connectionString);

    protected virtual void AddListeners(Configuration nhConfiguration)
    {
        var listeners = new object[] { new Listener(this) };
        nhConfiguration.AppendListeners(ListenerType.PostInsert, listeners);
    }

    protected virtual void OnPostInsert(PostInsertEvent @event)
    {
        if (@event.Persister is ILockable lockable) {
            var tableName = lockable.RootTableName;
            TableModified(tableName);
        }
    }

    public void TableModified(string tableName)
    {
        if (_modifiedTableNames.Count == 0) {
            using (var session = SessionFactory.OpenStatelessSession()) {
                CreateModifiedTablesTable(session.Connection);
            }
        }

        if (!_modifiedTableNames.Contains(tableName)) {
            using (var session = SessionFactory.OpenStatelessSession()) {
                InsertModifiedTableName(session.Connection, tableName);
            }

            _modifiedTableNames.Add(tableName);
        }
    }

    public void TruncateTables()
    {
        using (var session = SessionFactory.OpenStatelessSession()) {
            var tableNames = ListModifiedTableNames(session.Connection);
            foreach (var tableName in tableNames) {
                TruncateTable(session.Connection, tableName);
            }

            TruncateModifiedTablesTable(session.Connection);
        }

        _modifiedTableNames.Clear();
    }

    protected virtual void CreateModifiedTablesTable(DbConnection connection)
    {
        var command = connection.CreateCommand();
        command.CommandText = "create table if not exists testing_modified_table_names(table_name varchar(255))";
        command.ExecuteNonQuery();
    }

    protected virtual void TruncateModifiedTablesTable(DbConnection connection)
    {
        var command = connection.CreateCommand();
        command.CommandText = "delete from testing_modified_table_names";
        command.ExecuteNonQuery();
    }

    protected virtual void TruncateTable(DbConnection connection, string tableName)
    {
        var command = connection.CreateCommand();
        command.CommandText = $"delete from {tableName}";
        command.ExecuteNonQuery();
    }

    protected virtual void InsertModifiedTableName(DbConnection connection, string tableName)
    {
        if (string.IsNullOrWhiteSpace(tableName)) {
            return;
        }

        var command = connection.CreateCommand();
        command.CommandText = "insert into test_modified_table_names(table_name) values(?)";
        var parameter = command.CreateParameter();
        parameter.DbType = DbType.String;
        parameter.Value = tableName;
        command.Parameters.Add(parameter);
        command.ExecuteNonQuery();
    }

    protected virtual List<string> ListModifiedTableNames(DbConnection connection)
    {
        var tableNames = new List<string>();
        var command = connection.CreateCommand();
        command.CommandText = "select distinct table_name from test_modified_table_names";
        using var reader = command.ExecuteReader();
        while (reader.Read()) {
            if (reader.IsDBNull(0)) {
                continue;
            }

            var tableName = reader.GetString(0);
            tableNames.Add(tableName);
        }

        return tableNames;
    }

    private class Listener : IPostInsertEventListener
    {
        private readonly TestDatabase _testDatabase;

        public Listener(TestDatabase testDatabase)
        {
            _testDatabase = testDatabase;
        }

        public Task OnPostInsertAsync(PostInsertEvent @event, CancellationToken cancellationToken)
        {
            OnPostInsert(@event);
            return Task.CompletedTask;
        }

        public void OnPostInsert(PostInsertEvent @event)
        {
            _testDatabase.OnPostInsert(@event);
        }
    }
}

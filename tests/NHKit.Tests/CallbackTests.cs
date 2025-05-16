using NHibernate.Cfg;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using NHKit.Callbacks;
using Shouldly;
using Xunit;

namespace NHKit.Tests;

public class CallbackTests
{
    [Fact]
    public void It_calls_OnPostInsert_when_inserted()
    {
        var configuration = new ConfigurationBuilder()
            .AddMappings(typeof(PersonMap), typeof(LogRecordMap))
            .Build()
            .EnableEntityCallbacks()
            .BuildSessionFactory();

        using (var session = configuration.OpenSession()) {
            using var tx = session.BeginTransaction();
            var person = new Person {
                GivenName = "John",
                FamilyName = "Doe"
            };
            session.Save(person);
            tx.Commit();
        }

        using (var session = configuration.OpenSession()) {
            var logRecords = session.QueryOver<LogRecord>().List();
            logRecords.Count.ShouldBe(1);
            logRecords[0].Message.ShouldBe("Person John Doe created");
        }
    }

    [Fact]
    public void It_calls_OnPostInsert_once_when_inserted_and_updated()
    {
        var configuration = new ConfigurationBuilder()
            .AddMappings(typeof(PersonMap), typeof(LogRecordMap))
            .Build()
            .EnableEntityCallbacks()
            .BuildSessionFactory();

        using (var session = configuration.OpenSession()) {
            using var tx = session.BeginTransaction();
            var person = new Person {
                GivenName = "John",
                FamilyName = "Doe"
            };
            session.Save(person);
            session.Flush();

            person.GivenName = "Jane";
            session.Update(person);
            tx.Commit();
        }

        using (var session = configuration.OpenSession()) {
            var logRecords = session.QueryOver<LogRecord>().List();
            logRecords.Count.ShouldBe(1);
            logRecords[0].Message.ShouldBe("Person Jane Doe created");
        }
    }

    [Fact]
    public void It_does_not_call_OnPostInsert_when_inserted_and_deleted()
    {
        var configuration = new ConfigurationBuilder()
            .AddMappings(typeof(PersonMap), typeof(LogRecordMap))
            .Build()
            .EnableEntityCallbacks()
            .BuildSessionFactory();

        using (var session = configuration.OpenSession()) {
            using var tx = session.BeginTransaction();
            var person = new Person {
                GivenName = "John",
                FamilyName = "Doe"
            };
            session.Save(person);
            session.Flush();

            session.Delete(person);
            tx.Commit();
        }

        using (var session = configuration.OpenSession()) {
            var logRecords = session.QueryOver<LogRecord>().List();
            logRecords.Count.ShouldBe(0);
        }
    }

    [Fact]
    public void It_calls_OnPostUpdate_when_updated()
    {
        var configuration = new ConfigurationBuilder()
            .AddMappings(typeof(PersonMap), typeof(LogRecordMap))
            .Build()
            .EnableEntityCallbacks()
            .BuildSessionFactory();

        using (var session = configuration.OpenSession()) {
            using var tx = session.BeginTransaction();
            var person = new Person {
                GivenName = "John",
                FamilyName = "Doe"
            };
            session.Save(person);
            tx.Commit();
        }

        using (var session = configuration.OpenSession()) {
            using var tx = session.BeginTransaction();
            var person = session.QueryOver<Person>().Take(1).SingleOrDefault();
            person.GivenName.ShouldBe("John");
            person.GivenName = "Jane";
            tx.Commit();
        }

        using (var session = configuration.OpenSession()) {
            var logRecords = session.QueryOver<LogRecord>().List();
            logRecords.Count.ShouldBe(2);
            logRecords[0].Message.ShouldBe("Person John Doe created");
            logRecords[1].Message.ShouldBe("Given name changed from John to Jane");
        }
    }

    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Local
    private class Person : IHandleEntityInserted<Person>, IHandleEntityUpdated<Person>
    {
        public virtual int Id { get; set; }
        public virtual string GivenName { get; set; }
        public virtual string FamilyName { get; set; }


        public virtual void OnEntityInserted(IInsertedEntityInfo<Person> entity)
        {
            var logRecord = new LogRecord { Message = $"Person {GivenName} {FamilyName} created" };
            entity.Session.Save(logRecord);
        }

        public virtual void OnEntityUpdated(IUpdatedEntityInfo<Person> entity)
        {
            var session = entity.Session;
            if (entity.HasPropertyChanged(p => p.GivenName)) {
                var logRecord = new LogRecord { Message = $"Given name changed from {entity.GetOldValue(p => p.GivenName)} to {entity.GetCurrentValue(p => p.GivenName)}" };
                session.Save(logRecord);
            }
            if (entity.HasPropertyChanged(p => p.FamilyName)) {
                var logRecord = new LogRecord { Message = $"Family name changed from {entity.GetOldValue(p => p.FamilyName)} to {entity.GetCurrentValue(p => p.FamilyName)}" };
                session.Save(logRecord);
            }
        }
    }

    private class PersonMap : ClassMapping<Person>
    {
        public PersonMap()
        {
            Table("people");
            Id(x => x.Id, m => {
                m.Generator(Generators.Identity);
            });
            Property(x => x.GivenName);
            Property(x => x.FamilyName);
        }
    }

    private class LogRecord
    {
        public virtual int Id { get; set; }
        public virtual string Message { get; set; }
    }

    private class LogRecordMap : ClassMapping<LogRecord>
    {
        public LogRecordMap()
        {
            Table("log_records");
            Id(x => x.Id, m => {
                m.Generator(Generators.Identity);
            });
            Property(x => x.Message);
        }
    }
}

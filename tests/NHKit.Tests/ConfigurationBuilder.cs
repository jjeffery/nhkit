using NHibernate.Cfg;
using NHibernate.Connection;
using NHibernate.Dialect;
using NHibernate.Driver;
using NHibernate.Mapping.ByCode;
using NHibernate.Tool.hbm2ddl;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace NHKit.Tests;

public class ConfigurationBuilder
{
    private static int _number;

    public string FilePath { get; }
    private readonly ModelMapper _modelMapper = new();

    public ConfigurationBuilder(string filePath = null)
    {
        if (string.IsNullOrWhiteSpace(filePath)) {
            var number = Interlocked.Increment(ref _number);
            filePath = Path.Join(Path.GetTempPath(), $"nhkit-test-{number}.db");
        }
        FilePath = filePath;
    }

    public ConfigurationBuilder AddMappings(params IEnumerable<Type> types)
    {
        foreach (var type in types) {
            _modelMapper.AddMapping(type);
        }

        return this;
    }

    public Configuration Build()
    {
        var cfg = new Configuration();
        cfg.DataBaseIntegration(db => {
            db.ConnectionString = $"Data Source={FilePath}";
            db.Dialect<SQLiteDialect>();
            db.Driver<SQLite20Driver>();
            db.ConnectionProvider<DriverConnectionProvider>();
        });
        cfg.AddMapping(_modelMapper.CompileMappingForAllExplicitlyAddedEntities());

        var schemaExport = new SchemaExport(cfg);
        schemaExport.Create(true, true);
        return cfg;
    }
}

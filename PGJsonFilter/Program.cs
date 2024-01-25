using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Npgsql;
using JsonConverter = Newtonsoft.Json.JsonConverter;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

Console.WriteLine("Hello, World!");

const string conStr = "server=localhost:5432;database=testpg;user id=postgres;password=123456;";
var optionsBuilder = new DbContextOptionsBuilder<PGDbContext>();
optionsBuilder.UseNpgsql(PGDbContext.CreateDataSource(conStr));
var context = new PGDbContext(optionsBuilder.Options);
//context.Database.EnsureDeleted();
context.Database.EnsureCreated();
context.Database.Migrate();

var old = context.TestTable.ToList();
//context.TestTable.RemoveRange(old);

context.TestTable.AddRange(
    new TestTable
    {
        Name = "test",
        JsonData = new TestTableJson
        {
            Code = "x",
            Data =
            [
                new JsonData { Name = "x", Value = "1", TestIds = [1, 2, 3] },
                new JsonData { Name = "y", Value = "2", TestIds = [1, 2, 3] },
                new JsonData { Name = "z", Value = "3" }
            ]
        }
    },
    new TestTable
    {
        Name = "test",
        JsonData = new TestTableJson
        {
            Code = "x",
        }
    }
);
context.SaveChanges();

//this query not working without all data fetch
var testQuery = context.TestTable
    .Any(x => x.JsonData.Data
        .Any(y => y.Name == "x"));
Console.WriteLine("Done!");
//Debugger.Break();

public class PGDbContext : DbContext
{
    public DbSet<TestTable> TestTable { get; set; }

    public PGDbContext(DbContextOptions<PGDbContext> options) :
        base(options)
    {
    }

    public static NpgsqlDataSource CreateDataSource(string connectionString = null)
    {
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        //dataSourceBuilder.EnableDynamicJson();
        // dataSourceBuilder.ConfigureJsonOptions(new JsonSerializerOptions
        // {
        //     Converters = { new EnumToStringConverter<RowStatus>() }
        // });
        dataSourceBuilder.MapEnum<RowStatus>();
        return dataSourceBuilder.Build();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresEnum<RowStatus>();
        modelBuilder.Entity<TestTable>()
            .OwnsOne(t => t.JsonData, x =>
            {
                x.ToJson();
                x.Property(t => t.RowStatus).HasConversion(new EnumToStringConverter<RowStatus>());
                x.OwnsMany(t => t.Data).Property(t => t.RowStatus).HasConversion(new EnumToStringConverter<RowStatus>());
            });
        base.OnModelCreating(modelBuilder);
    }
}

public enum RowStatus
{
    active = 1,
    passive = 2,
    deleted = 3,
}

public class TestTable
{
    public int Id { get; set; }
    public string Name { get; set; }

    public TestTableJson JsonData { get; set; }

    public RowStatus RowStatus { get; set; } = RowStatus.active;
}

public class TestTableJson
{
    public string Code { get; set; }

    public RowStatus RowStatus { get; set; } = RowStatus.active;
    public List<JsonData> Data { get; set; }
}

public class JsonData
{
    public string Name { get; set; }
    public string Value { get; set; }
    public DateTime Date1 { get; set; } = DateTime.UtcNow;
    public DateTime? Date2 { get; set; }

    public List<int> TestIds { get; set; }
    public RowStatus RowStatus { get; set; }
}
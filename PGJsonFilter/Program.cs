using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Npgsql;

Console.WriteLine("Hello, World!");

const string conStr = "server=localhost:5432;database=testpg;user id=postgres;password=123456;";
var optionsBuilder = new DbContextOptionsBuilder<PGDbContext>();
optionsBuilder.UseNpgsql(PGDbContext.CreateDataSource(conStr));
var context = new PGDbContext(optionsBuilder.Options);
context.Database.EnsureCreated();
context.Database.Migrate();

if (!context.TestTable.Any())
{
    context.TestTable.Add(
        new TestTable
        {
            Name = "test",
            JsonData = new TestTableJson
            {
                Code = "x",
                Data =
                [
                    new JsonData { Name = "x", Value = "1" },
                    new JsonData { Name = "y", Value = "2" },
                    new JsonData { Name = "z", Value = "3" }
                ]
            }
        }
    );
    context.SaveChanges();
}

//this query not working without all data fetch
var testQuery = context.TestTable
    .Any(x => x.JsonData.Data
        .Any(y => y.Name == "x"));

Console.WriteLine("Done!");

public class PGDbContext : DbContext
{
    public DbSet<TestTable> TestTable { get; set; }

    public PGDbContext(DbContextOptions<PGDbContext> options) : base(options)
    {
    }

    public static NpgsqlDataSource CreateDataSource(string connectionString = null)
    {
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        // dataSourceBuilder.EnableDynamicJson();
        // dataSourceBuilder.UseJsonNet();
        return dataSourceBuilder.Build();
    }
}


public class TestTable
{
    public int Id { get; set; }
    public string Name { get; set; }

    [Column(TypeName = "jsonb")]
    public TestTableJson JsonData { get; set; }
}

public class TestTableJson
{
    public string Code { get; set; }
    public List<JsonData> Data { get; set; }
}

public class JsonData
{
    public string Name { get; set; }
    public string Value { get; set; }
}
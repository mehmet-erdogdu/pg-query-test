using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Testcontainers.MsSql;

Console.WriteLine("Hello, World!");

var msSqlContainer = new MsSqlBuilder().Build();
await msSqlContainer.StartAsync();
var conStr = msSqlContainer.GetConnectionString();

var optionsBuilder = new DbContextOptionsBuilder<PGDbContext>();
optionsBuilder.UseSqlServer(conStr);
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

Debugger.Break();

public class PGDbContext : DbContext
{
    public DbSet<TestTable> TestTable { get; set; }

    public PGDbContext(DbContextOptions<PGDbContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.LogTo(Console.WriteLine);
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TestTable>()
            .OwnsOne(category => category.JsonData, b =>
            {
                b.OwnsMany(c => c.Data);
                b.ToJson();
            });
        base.OnModelCreating(modelBuilder);
    }
}


public class TestTable
{
    public int Id { get; set; }
    public string Name { get; set; }
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
# DbContext

DbContext is a fundamental class in Entity Framework (EF) Core that acts as a bridge between your domain or entity classes and the database. It manages the entity objects during runtime, which includes tasks such as querying the database, saving data, and managing changes. Here's an explanation and a detailed example:

Explanation of DbContext
Configuration: DbContext is responsible for configuring the database connection and the model. This includes specifying the database provider (e.g., SQL Server, SQLite, In-Memory) and the connection string.

DbSet Properties: DbContext contains DbSet<TEntity> properties for each entity type in the model. These properties represent the tables in the database and provide access to querying and saving instances of the entity type.

Change Tracking: DbContext tracks changes to entities so it can detect changes that need to be persisted to the database. When you call SaveChanges, it creates the necessary SQL statements to update the database.

Querying: DbContext allows you to query the database using LINQ. It translates LINQ queries into SQL queries that are executed against the database.

Saving Data: DbContext provides methods like SaveChanges to persist changes made to the entities to the database.

Alternatives

If you don't want to use `DbContext` in Entity Framework Core, there are several alternatives for interacting with databases in .NET applications. Here are a few popular alternatives, each with its own use cases and advantages:

### 1. ADO.NET
ADO.NET is a set of classes that expose data access services for .NET Framework programmers. It provides a more granular level of control over database operations compared to ORMs like EF Core.

#### Example using ADO.NET
```csharp
using System;
using System.Data.SqlClient;

class Program
{
    static void Main()
    {
        string connectionString = "YourConnectionStringHere";
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            connection.Open();

            string query = "SELECT * FROM Products";
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Console.WriteLine($"{reader["Id"]}, {reader["Name"]}, {reader["Price"]}");
                    }
                }
            }
        }
    }
}
```

### 2. Dapper
Dapper is a micro ORM that provides a simple way to map SQL queries to objects. It is less feature-rich than EF Core but offers excellent performance and flexibility.

#### Example using Dapper
```csharp
using System;
using System.Data.SqlClient;
using Dapper;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        string connectionString = "YourConnectionStringHere";
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            string query = "SELECT * FROM Products";
            IEnumerable<Product> products = connection.Query<Product>(query);

            foreach (var product in products)
            {
                Console.WriteLine($"{product.Id}, {product.Name}, {product.Price}");
            }
        }
    }
}

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
}
```

### 3. NHibernate
NHibernate is a mature ORM for .NET that offers extensive mapping capabilities and a rich feature set. It is more complex than Dapper but provides a lot of flexibility and functionality.

#### Example using NHibernate
```csharp
// Add the necessary NHibernate packages and configure NHibernate for your application.
```

### 4. Raw SQL with Custom Abstractions
You can write your own data access layer using raw SQL and custom abstractions to interact with the database. This gives you complete control over the database operations.

#### Example using Raw SQL with Custom Abstractions
```csharp
using System;
using System.Data.SqlClient;

public interface IProductRepository
{
    IEnumerable<Product> GetAll();
    Product GetById(int id);
    void Add(Product product);
    void Update(Product product);
    void Delete(int id);
}

public class ProductRepository : IProductRepository
{
    private readonly string _connectionString;

    public ProductRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IEnumerable<Product> GetAll()
    {
        var products = new List<Product>();
        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            var query = "SELECT * FROM Products";
            using (var command = new SqlCommand(query, connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        products.Add(new Product
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Price = reader.GetDecimal(2)
                        });
                    }
                }
            }
        }
        return products;
    }

    public Product GetById(int id)
    {
        // Implement similar to GetAll, but with a WHERE clause
        // ...
    }

    public void Add(Product product)
    {
        // Implement INSERT logic
        // ...
    }

    public void Update(Product product)
    {
        // Implement UPDATE logic
        // ...
    }

    public void Delete(int id)
    {
        // Implement DELETE logic
        // ...
    }
}

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
}
```

### 5. Other ORMs
There are other ORMs available, such as LinqConnect and ServiceStack.OrmLite, which might suit specific needs or preferences.

### Conclusion
While Entity Framework Core's `DbContext` provides a powerful and convenient way to interact with databases, there are numerous alternatives available, ranging from low-level ADO.NET to micro ORMs like Dapper, or more feature-rich ORMs like NHibernate. The choice of alternative depends on your specific requirements, such as the need for performance, flexibility, simplicity, or the level of control over database interactions.

When working with .NET Minimal APIs, the choice of data access technology largely depends on your specific needs regarding performance, ease of use, and flexibility. Here are some considerations for choosing among ADO.NET, Dapper, Entity Framework Core, and other options:

### 1. **Entity Framework Core (EF Core)**

**Pros:**
- **Easy to Use:** EF Core is easy to set up and use with a high-level abstraction over database operations.
- **Rich Features:** It offers advanced features like change tracking, lazy loading, and LINQ queries.
- **Migrations:** EF Core supports database migrations to manage schema changes over time.

**Cons:**
- **Performance:** It can be slower compared to Dapper and raw ADO.NET due to its higher-level abstraction and additional features.
- **Learning Curve:** There can be a learning curve to fully understand and utilize its features.

**When to Use:**
- When you prefer ease of use and productivity over raw performance.
- When you need advanced ORM features and built-in migration support.

### 2. **Dapper**

**Pros:**
- **Performance:** Dapper is very fast and efficient because it uses raw SQL under the hood.
- **Flexibility:** You have full control over the SQL queries, which can lead to optimized performance.
- **Lightweight:** It’s a lightweight library without the overhead of a full ORM.

**Cons:**
- **Manual Mapping:** You need to manually write SQL queries and map results to your entities.
- **Limited Features:** Lacks advanced ORM features like change tracking and migrations.

**When to Use:**
- When performance is a critical factor.
- When you want more control over SQL queries.
- When you don’t need advanced ORM features.

### 3. **ADO.NET**

**Pros:**
- **Full Control:** ADO.NET gives you the most control over database interactions.
- **Performance:** It can be very performant because you’re working closer to the metal.

**Cons:**
- **Boilerplate Code:** You have to write a lot of boilerplate code to handle connections, commands, readers, etc.
- **Complexity:** It’s more complex and time-consuming compared to using an ORM.

**When to Use:**
- When you need maximum performance and control.
- When you are comfortable handling database interactions at a low level.
- When you want to avoid the overhead of an ORM.

### 4. **Other ORMs (e.g., NHibernate)**

**Pros:**
- **Feature-rich:** NHibernate and other ORMs offer a rich set of features and flexibility.

**Cons:**
- **Complexity:** They can be more complex to configure and use compared to EF Core and Dapper.
- **Learning Curve:** They often come with a steeper learning curve.

**When to Use:**
- When you need specific features offered by the ORM.
- When you have existing expertise with the ORM.

### Example with .NET Minimal API and EF Core

Here’s how you can set up a .NET Minimal API with EF Core:

#### 1. Create a new Minimal API project
```sh
dotnet new web -n MinimalApiApp
cd MinimalApiApp
```

#### 2. Add EF Core and In-Memory Database Provider
```sh
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.InMemory
```

#### 3. Define the `Product` entity
```csharp
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
}
```

#### 4. Create the `AppDbContext`
```csharp
public class AppDbContext : DbContext
{
    public DbSet<Product> Products { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
        });
    }
}
```

#### 5. Configure the Minimal API
```csharp
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("InMemoryDb"));

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGet("/products", async (AppDbContext db) =>
{
    return await db.Products.ToListAsync();
});

app.MapGet("/products/{id}", async (int id, AppDbContext db) =>
{
    return await db.Products.FindAsync(id)
        is Product product
            ? Results.Ok(product)
            : Results.NotFound();
});

app.MapPost("/products", async (Product product, AppDbContext db) =>
{
    db.Products.Add(product);
    await db.SaveChangesAsync();

    return Results.Created($"/products/{product.Id}", product);
});

app.MapPut("/products/{id}", async (int id, Product inputProduct, AppDbContext db) =>
{
    var product = await db.Products.FindAsync(id);

    if (product is null) return Results.NotFound();

    product.Name = inputProduct.Name;
    product.Price = inputProduct.Price;

    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.MapDelete("/products/{id}", async (int id, AppDbContext db) =>
{
    var product = await db.Products.FindAsync(id);

    if (product is null) return Results.NotFound();

    db.Products.Remove(product);
    await db.SaveChangesAsync();

    return Results.Ok(product);
});

app.Run();
```

### Conclusion

For a .NET Minimal API, the choice of data access technology will depend on your requirements:

- Use **EF Core** if you want an easy-to-use ORM with advanced features and support for migrations.
- Use **Dapper** if you need high performance and fine-grained control over SQL queries.
- Use **ADO.NET** if you need the utmost control and performance and are comfortable with lower-level database operations.
- Consider other ORMs like **NHibernate** if you need specific features or have existing expertise.

Each option has its trade-offs, so choose the one that best fits your application's needs and your team's expertise.

| Feature               | ADO.NET                         | Dapper                          | EF Core                         |
|-----------------------|---------------------------------|---------------------------------|---------------------------------|
| **Performance**       | High                            | Very High                       | Moderate                        |
| **Control**           | Full                            | High                            | Moderate                        |
| **Ease of Use**       | Low                             | High                            | High                            |
| **Boilerplate Code**  | High                            | Low                             | Low                             |
| **Feature Set**       | Basic                           | Basic                           | Advanced                        |
| **Learning Curve**    | Moderate to High                | Low to Moderate                 | Moderate to High                |
| **Querying**          | SQL                             | SQL                             | LINQ                            |
| **Change Tracking**   | No                              | No                              | Yes                             |
| **Migrations**        | No                              | No                              | Yes                             |
| **Ideal Use Cases**   | High control and performance    | Performance with simplicity     | Productivity and advanced features |


Conclusion
The choice of data access technology should be based on your specific needs:

Use ADO.NET if you need maximum control and performance and are comfortable with more complex and verbose code.
Use Dapper if you want high performance with simplicity and are comfortable writing SQL queries.
Use EF Core if you prefer high-level abstractions, productivity, and advanced ORM features like change tracking and migrations.

# Entity Framework (EF) Core

Entity Framework (EF) Core is designed to be a flexible and extensible ORM (Object-Relational Mapper) and supports multiple database providers. While it works with many different types of databases, it doesn't support every possible database out-of-the-box. However, it supports a wide range of popular databases through official and third-party providers.

### Commonly Supported Databases

1. **SQL Server:** EF Core has excellent support for Microsoft SQL Server. It is one of the most commonly used providers and offers robust features and performance.

    ```csharp
    optionsBuilder.UseSqlServer("YourConnectionStringHere");
    ```

2. **SQLite:** A lightweight, file-based database that's great for development, testing, and small applications.

    ```csharp
    optionsBuilder.UseSqlite("Data Source=yourdatabase.db");
    ```

3. **In-Memory Database:** Useful for testing purposes as it stores data in memory and is not persisted to disk.

    ```csharp
    optionsBuilder.UseInMemoryDatabase("InMemoryDb");
    ```

4. **MySQL:** Supported via the Pomelo.EntityFrameworkCore.MySql provider.

    ```csharp
    optionsBuilder.UseMySql("YourConnectionStringHere", new MySqlServerVersion(new Version(8, 0, 21)));
    ```

5. **PostgreSQL:** Supported via the Npgsql.EntityFrameworkCore.PostgreSQL provider.

    ```csharp
    optionsBuilder.UseNpgsql("Host=my_host;Database=my_db;Username=my_user;Password=my_pw");
    ```

6. **Oracle:** Supported via the Oracle.EntityFrameworkCore provider.

    ```csharp
    optionsBuilder.UseOracle("User Id=your_user;Password=your_password;Data Source=your_data_source");
    ```

7. **Cosmos DB:** Microsoft's Azure Cosmos DB, a globally distributed, multi-model database service.

    ```csharp
    optionsBuilder.UseCosmos("YourAccountEndpoint", "YourAccountKey", "YourDatabaseName");
    ```

### Example Configuration

Here's an example of configuring a `DbContext` to use a SQL Server database:

```csharp
public class AppDbContext : DbContext
{
    public DbSet<Product> Products { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer("YourConnectionStringHere");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
        });
    }
}
```

### How to Add Providers

To add a provider, you generally need to install the corresponding NuGet package. For example:

- For SQL Server: `Microsoft.EntityFrameworkCore.SqlServer`
- For SQLite: `Microsoft.EntityFrameworkCore.Sqlite`
- For In-Memory: `Microsoft.EntityFrameworkCore.InMemory`
- For MySQL: `Pomelo.EntityFrameworkCore.MySql`
- For PostgreSQL: `Npgsql.EntityFrameworkCore.PostgreSQL`
- For Oracle: `Oracle.EntityFrameworkCore`
- For Cosmos DB: `Microsoft.EntityFrameworkCore.Cosmos`

### Installing a Provider

You can install the provider using the .NET CLI:

```sh
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
```

### Conclusion

While EF Core supports many popular databases out-of-the-box, it doesn't support every possible database. However, it is extensible, and third-party providers can add support for additional databases. Always check the specific provider's documentation for compatibility and setup instructions.
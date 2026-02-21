using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;


var builder = WebApplication.CreateBuilder(args);

// Add Redis Service
var redisUrl = builder.Configuration["RedisUrl"];
if (!string.IsNullOrEmpty(redisUrl))
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisUrl;
    });
}
else
{
    // THIS SAVES THE DAY IN AZURE:
    builder.Services.AddDistributedMemoryCache();
}

//var redisUrl = builder.Configuration["RedisUrl"];
//if (!string.IsNullOrEmpty(redisUrl))
//{
//    builder.Services.AddStackExchangeRedisCache(options =>
//    {
//        options.Configuration = redisUrl;
//    });
//}

// 1. Get the Connection String (The address of the Notebook)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 2. Add the Database Context (The Brain)
// We use a trick: If no connection string is found (like on local laptop without docker), use temporary memory.
if (string.IsNullOrEmpty(connectionString))
{
    builder.Services.AddDbContext<ItemDbContext>(opt => opt.UseInMemoryDatabase("TempDb"));
}
else
{
    builder.Services.AddDbContext<ItemDbContext>(opt => opt.UseSqlServer(connectionString));
}

var app = builder.Build();

// 3. Create the Database automatically when startup (Migration)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ItemDbContext>();
    db.Database.EnsureCreated(); // Creates the Table if it doesn't exist

    // Seed some data if empty
    if (!db.Items.Any())
    {
        db.Items.Add(new Item { Name = "Super Burger", Price = 5.99 });
        db.Items.Add(new Item { Name = "Cheese Fries", Price = 2.50 });
        db.SaveChanges();
    }
}

// 4. ENDPOINT: Get all items from DB
app.MapGet("/items", async (ItemDbContext db) => await db.Items.ToListAsync());

// 5. ENDPOINT: Get one item
app.MapGet("/items/{id}", async (int id, ItemDbContext db, Microsoft.Extensions.Caching.Distributed.IDistributedCache cache) =>
{
    string cacheKey = $"item_{id}";

    // 1. Check Sticky Note (Redis) first!
    var cachedItem = await cache.GetStringAsync(cacheKey);
    if (!string.IsNullOrEmpty(cachedItem))
    {
        Console.WriteLine("Found in Cache! (Fast)");
        return Results.Ok(System.Text.Json.JsonSerializer.Deserialize<Item>(cachedItem));
    }

    // 2. If not found, go to Notebook (SQL)
    Console.WriteLine("Not in Cache. Checking Database... (Slow)");
    var item = await db.Items.FindAsync(id);

    if (item is null) return Results.NotFound();

    // 3. Save to Sticky Note for next time (expires in 1 minute)
    var json = System.Text.Json.JsonSerializer.Serialize(item);
    await cache.SetStringAsync(cacheKey, json, new Microsoft.Extensions.Caching.Distributed.DistributedCacheEntryOptions
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1)
    });

    return Results.Ok(item);
});



//// --without Redis
//app.MapGet("/items/{id}", async (int id, ItemDbContext db) =>
//{
//    var item = await db.Items.FindAsync(id);
//    return item is not null ? Results.Ok(item) : Results.NotFound();
//});

// 6. ENDPOINT: Add a new item (Now we can save data!)
app.MapPost("/items", async (Item item, ItemDbContext db) =>
{
    db.Items.Add(item);
    await db.SaveChangesAsync();
    return Results.Created($"/items/{item.Id}", item);
});

app.Run();

// --- The Data Models ---
public class Item
{
    public int Id { get; set; }
    public string Name { get; set; }
    public double Price { get; set; }
}

// The Bridge between Code and SQL
public class ItemDbContext : DbContext
{
    public ItemDbContext(DbContextOptions<ItemDbContext> options) : base(options) { }
    public DbSet<Item> Items { get; set; }
}












//var builder = WebApplication.CreateBuilder(args);

//// Add services to the container.
//// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//builder.Services.AddOpenApi();

//var app = builder.Build();

//// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.MapOpenApi();
//}

//app.UseHttpsRedirection();

//var summaries = new[]
//{
//    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
//};

//app.MapGet("/weatherforecast", () =>
//{
//    var forecast =  Enumerable.Range(1, 5).Select(index =>
//        new WeatherForecast
//        (
//            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
//            Random.Shared.Next(-20, 55),
//            summaries[Random.Shared.Next(summaries.Length)]
//        ))
//        .ToArray();
//    return forecast;
//})
//.WithName("GetWeatherForecast");


//// This is our Database (Just a simple list for now)
//var items = new List<Item>
//{
//    new Item(1, "Burger"),
//    new Item(2, "Fries"),
//    new Item(3, "Soda")
//};

//// 1. ENDPOINT: A way to ask "What food do you have?"
//app.MapGet("/items", () => items);

//// 2. ENDPOINT: Get a specific item by ID
//app.MapGet("/items/{id}", (int id) =>
//{
//    var item = items.FirstOrDefault(i => i.Id == id);
//    return item is not null ? Results.Ok(item) : Results.NotFound();
//});

//// Make this robot listen on Port 5001
////app.Run("https://localhost:5001");

//app.Run();
//// Our simple Food Item definition
//record Item(int Id, string Name);


//record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
//{
//    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
//}



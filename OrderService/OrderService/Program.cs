using Microsoft.AspNetCore.Mvc;

// Enterprise Chaos Test: Verifying CI/CD Path Filtering

var builder = WebApplication.CreateBuilder(args);


// 1. Add the "Phone Factory" to the shop
builder.Services.AddHttpClient();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();



app.MapGet("/order", async (int id, IHttpClientFactory clientFactory, IConfiguration config) =>
{
    var client = clientFactory.CreateClient();

    // 1. Get the address from settings, OR use localhost as a backup
    // This variable "ITEM_SERVICE_URL" comes from docker-compose.yml
    var itemServiceUrl = config["ITEM_SERVICE_URL"] ?? "https://localhost:5001";

    // 2. Build the full URL
    // Note: If running in Docker, this url becomes http://item-api:8080/items/{id}
    var url = $"{itemServiceUrl}/items/{id}";

    Console.WriteLine($"Calling Inventory at: {url}"); // Debug log

    try
    {
        var response = await client.GetAsync(url);
        // ... rest of your code is the same ...
        if (!response.IsSuccessStatusCode) return Results.Problem("Item not found");
        var item = await response.Content.ReadFromJsonAsync<Item>();
        return Results.Ok($"Order placed for: {item.Name}");
    }
    catch (Exception ex)
    {
        return Results.Problem($"Inventory is down: {ex.Message}");
    }

});



//// 2. ENDPOINT: Customer places an order
//// Notice the change: We ask for 'IHttpClientFactory' instead of just 'HttpClient'
//app.MapGet("/order", async (int id, IHttpClientFactory clientFactory) =>
//{
//    var client = clientFactory.CreateClient();

//    // --- START OF SAFETY NET ---
//    try
//    {
//        // 1. Try to call the Inventory Guy
//        var response = await client.GetAsync($"https://localhost:5001/items/{id}");

//        if (!response.IsSuccessStatusCode)
//        {
//            return Results.Problem("Oops! Inventory Guy exists, but he doesn't have that item.");
//        }

//        var item = await response.Content.ReadFromJsonAsync<Item>();
//        return Results.Ok($"Order placed for: {item.Name}! Yummy.");
//    }
//    catch (HttpRequestException ex)
//    {
//        // 2. CATCH THE FALL! 
//        // This runs if the Inventory Guy is completely dead or unreachable.

//        // Log the error for the programmer (optional)
//        Console.WriteLine($"Alert: Inventory Service is down: {ex.Message}");

//        // Return a nice message to the user
//        return Results.Problem("Sorry, our Inventory System is currently offline. We cannot place orders right now.");
//    }
//    // --- END OF SAFETY NET ---
//});

//// ENDPOINT: Customer places an order
//app.MapPost("/order", async (int id, HttpClient client) =>
//{
//    // 1. The Cashier (OrderService) calls the Inventory Guy (ItemService)
//    // He asks: "Hey, does item #{id} exist?"
//    var response = await client.GetAsync($"http://localhost:5001/items/{id}");

//    if (!response.IsSuccessStatusCode)
//    {
//        return Results.Problem("Oops! The Inventory Guy didn't answer or doesn't have that item.");
//    }

//    // 2. Read the answer
//    var item = await response.Content.ReadFromJsonAsync<Item>();

//    // 3. Confirm the order
//    return Results.Ok($"Order placed for: {item.Name}! Yummy.");
//});

//// Make this robot listen on Port 5002
//app.Run("https://localhost:5002");
app.Run();

// We need to know what an 'Item' looks like here too
record Item(int Id, string Name);




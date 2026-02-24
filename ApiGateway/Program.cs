using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1. Define the super secret key (In production, this comes from Azure Key Vault!)
var secretKey = "ThisIsMySuperSecretKeyForFoodCourtWhichNeedsToBeAtLeast32BytesLong!";
var key = Encoding.ASCII.GetBytes(secretKey);

// 2. Train the Bouncer (Configure JWT Authentication)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false, // We will skip issuer/audience validation for this test
            ValidateAudience = false,
            ValidateLifetime = true
        };
    });

// 3. Tell .NET to read our routing map
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
builder.Services.AddOcelot(builder.Configuration);

var app = builder.Build();

app.MapGet("/", () => "Welcome to the SECURE Food Court API Gateway!");

// 4. Turn on the Security Checkpoint BEFORE the routing engine
app.UseAuthentication();
app.UseAuthorization();

await app.UseOcelot();

app.Run();




//using Ocelot.DependencyInjection;
//using Ocelot.Middleware;

//var builder = WebApplication.CreateBuilder(args);

//// Tell .NET to read our routing map
//builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

//// Add the Ocelot tool to the toolbox
//builder.Services.AddOcelot(builder.Configuration);

//var app = builder.Build();

//// A simple health check so we know the Front Desk is awake
//app.MapGet("/", () => "Welcome to the Food Court API Gateway!");

//// Turn on the routing engine
//await app.UseOcelot();

//app.Run();
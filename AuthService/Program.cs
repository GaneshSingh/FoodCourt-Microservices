using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Food Court Security Office is Open!");

// The Login Endpoint
app.MapPost("/login", (UserLogin user) =>
{
    // In a real system, you would query your Azure SQL Database here to check the password hash.
    // For our architecture test, we will hardcode a valid admin user.
    if (user.Username == "ganesh" && user.Password == "architect")
    {
        // THIS MUST MATCH THE API GATEWAY EXACTLY
        var secretKey = "ThisIsMySuperSecretKeyForFoodCourtWhichNeedsToBeAtLeast32BytesLong!";
        var key = Encoding.ASCII.GetBytes(secretKey);

        // Create the ID Card (Claims)
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim("Role", "Admin")
            }),
            Expires = DateTime.UtcNow.AddHours(1), // Token valid for 1 hour
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        // Mint the Token
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        // Hand the token back to the user
        return Results.Ok(new { Token = tokenHandler.WriteToken(token) });
    }

    // If bad password, throw them out
    return Results.Unauthorized();
});

app.Run();

// A simple record to hold the incoming JSON data
public record UserLogin(string Username, string Password);




//var builder = WebApplication.CreateBuilder(args);
//var app = builder.Build();

//app.MapGet("/", () => "Hello World!");

//app.Run();

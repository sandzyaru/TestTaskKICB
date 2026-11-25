using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Npgsql;
using Swashbuckle.AspNetCore.SwaggerGen;
using TestTaskKICB.Application.Repositories;
using TestTaskKICB.Application.Services;
using TestTaskKICB.Domain.Models;
using TestTaskKICB.Infrastructure.DB;
using TestTaskKICB.Infrastructure.Repositories;
using TestTaskKICB.Presentation.Api.V1;

var builder = WebApplication.CreateBuilder(args);
var connectionString = GetBankConnectionString(builder.Configuration);

builder.Services.AddDbContext<BankDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<CustomerService>();
builder.Services.AddScoped<AccountService>();
builder.Services.ConfigureHttpJsonOptions(options =>
{
    var json = options.SerializerOptions;
    json.Converters.Add(new JsonStringEnumConverter<Currency>(allowIntegerValues: false));
    json.Converters.Add(new JsonStringEnumConverter<CardType>(
        JsonNamingPolicy.CamelCase,
        allowIntegerValues: false
    ));
    json.Converters.Add(new JsonStringEnumConverter<PhoneType>(
        JsonNamingPolicy.CamelCase,
        allowIntegerValues: false
    ));
});
builder.Services.Configure<RouteHandlerOptions>(options => options.ThrowOnBadRequest = false);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSingleton<ISerializerDataContractResolver>(services =>
{
    var json = services.GetRequiredService<IOptions<JsonOptions>>().Value.SerializerOptions;
    return new JsonSerializerDataContractResolver(json);
});
builder.Services.AddSwaggerGen(options =>
{
    options.MapType<DateOnly>(() => new OpenApiSchema { Type = "string", Format = "date" });
    options.MapType<JsonElement>(() => new OpenApiSchema { Type = "number", Format = "double" });
});
builder.Services.AddProblemDetails();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BankDbContext>();
    db.Database.Migrate();
}

app.UseExceptionHandler(new ExceptionHandlerOptions
{
    ExceptionHandler = context => ApiErrors.Write(context, DatabaseErrors.Read),
    AllowStatusCode404Response = true
});
app.UseStatusCodePages();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/health/ready", Ready);
app.MapCustomersEndpoints();
app.MapPhonesEndpoints();
app.MapAccountsEndpoints();
app.MapCardsEndpoints();
app.Run();

static IResult Ready(BankDbContext db)
{
    if (db.Database.CanConnect())
        return Results.Ok(new { status = "ready" });
    return Results.Problem(statusCode: StatusCodes.Status503ServiceUnavailable);
}

static string? GetBankConnectionString(IConfiguration configuration)
{
    var connectionString = configuration.GetConnectionString("Bank");
    if (connectionString != null)
        return connectionString;

    var database = configuration.GetSection("Bank");
    var host = database["Host"];
    if (host == null)
        return null;

    var connectionSettings = new NpgsqlConnectionStringBuilder
    {
        Host = host,
        Database = database["Database"],
        Username = database["Username"],
        Password = database["Password"]
    };
    return connectionSettings.ConnectionString;
}

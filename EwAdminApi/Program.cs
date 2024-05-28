using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using EwAdminApi.Middlewares;
using EwAdminApi.Repositories;
using EwAdminApi.Services;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Define the name of the XML documentation file based on the executing assembly's name
var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";

// Combine the base directory path with the XML file name to get the full path to the XML documentation file
var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

// Add services to the container.
builder.Services.AddSingleton<IConnectionService, ConnectionService>();
builder.Services.AddScoped<PosShopRepository>();
builder.Services.AddScoped<PosShopWorkdayDetailRepository>();
builder.Services.AddScoped<PosShopWorkdayPeriodDetailRepository>();
builder.Services.AddScoped<PosTxSalesRepository>();
builder.Services.AddScoped<PosPaymentMethodRepository>();
builder.Services.AddScoped<PosItemCategoryRepository>();
builder.Services.AddScoped<WebAdminRegionMasterRepository>();
builder.Services.AddScoped<WebAdminCompanyMasterRepository>();
builder.Services.AddScoped<WebAdminBrandMasterRepository>();
builder.Services.AddScoped<PosTableMasterRepository>();

// builder.Services.TryAddMondayClient(options =>
// {
//     options.EndPoint = new System.Uri(builder.Configuration.GetSection("Monday:Endpoint").Value ?? string.Empty);
//     options.Token = builder.Configuration.GetSection("Monday:Token").Value ?? string.Empty;
// });

builder.Services.AddHttpClient("MondayClient", client =>
{
    client.BaseAddress = new Uri(builder.Configuration.GetSection("Monday:Endpoint").Value ?? string.Empty);
    // let user pass the API key in each API calls
    //client.DefaultRequestHeaders.Add("Authorization", "Bearer your_monday_api_key_here");
});

builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor(); 

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "EwAdminApi", 
        Version = "v1",
        Description = "These APIs are used for internal admin purpose."
    });
    
    // c.EnableAnnotations();
    c.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

// cross-origin resource sharing with specific origins (https://localhost:5001)
app.UseCors(c =>
{
    c.WithOrigins("https://localhost:5001")
        .AllowAnyHeader()
        .AllowAnyMethod();
});


app.UseMiddleware<MondayAuthorizationMiddleware>();

app.UseStatusCodePages(context =>
{
    var request = context.HttpContext.Request;
    if (request.Path.StartsWithSegments("/api") 
        && context.HttpContext.Response.StatusCode is 400 or 403 or 404 or 500 
        && !context.HttpContext.Response.HasStarted)
    {
        context.HttpContext.Response.ContentType = "application/json";
        var result = JsonSerializer.Serialize(new
        {
            type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
            title = "Unhandled exception occurred.",
            status = context.HttpContext.Response.StatusCode,
            errors = context.HttpContext.Features.Get<IExceptionHandlerFeature>()?.Error.Message,
            traceId = Activity.Current?.Id ?? context.HttpContext.TraceIdentifier
        });
        return context.HttpContext.Response.WriteAsync(result);
    }
    return Task.CompletedTask;
});

app.MapControllers();

app.Run();
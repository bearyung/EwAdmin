using System.Data;
using EwAdminApi.Middlewares;
using EwAdminApi.Repositories;
using EwAdminApi.Services;
using Microsoft.Data.SqlClient;
using MondaySharp.NET.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSingleton<IConnectionService, ConnectionService>();
builder.Services.AddScoped<WebAdminCompanyMasterRepository>();
builder.Services.AddScoped<PosShopRepository>();
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

app.UseMiddleware<MondayAuthorizationMiddleware>();

app.MapControllers();

app.Run();
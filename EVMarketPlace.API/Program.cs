using EVMarketPlace.Repositories.Repository;
using EVMarketPlace.Services.Implements;
using EVMarketPlace.Services.Interfaces;

using Microsoft.AspNetCore.Identity.UI.Services;


using Microsoft.EntityFrameworkCore;
using EVMarketPlace.Repositories.Options;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMemoryCache();
builder.Services.AddScoped<IEmailSender,EmailSender>();
builder.Services.AddScoped<IOtpService, OtpService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<UserRepository>();




// Configure PaginationOptions
builder.Services.Configure<PaginationOptions>(
    builder.Configuration.GetSection("Pagination"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

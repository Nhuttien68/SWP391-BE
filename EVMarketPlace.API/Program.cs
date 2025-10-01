using EVMarketPlace.Repositories.Repository;
using EVMarketPlace.Services.Implements;
using EVMarketPlace.Services.Interfaces;
using EVMarketPlace.Repositories.Context;
using Microsoft.EntityFrameworkCore;
using EVMarketPlace.Repositories.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<IPostService, PostService>();

// Add DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sql =>
        {
            sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(2), null);
            sql.CommandTimeout(30);
        }));

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

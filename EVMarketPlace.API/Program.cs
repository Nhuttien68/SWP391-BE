using EVMarketPlace.Repositories.Entity;
using EVMarketPlace.Repositories.Options;
using EVMarketPlace.Repositories.Repository;
using EVMarketPlace.Repositories.Utils;
using EVMarketPlace.Services;
using EVMarketPlace.Services.Implements;
using EVMarketPlace.Services.Interfaces;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();


builder.Services.AddMemoryCache();
builder.Services.AddScoped<IEmailSender,EmailSender>();
builder.Services.AddScoped<IOtpService, OtpService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IWalletService, WalletService>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<IBatteryBrandService, BatteryBrandService>();
builder.Services.AddScoped<IVehicleBrandService, VehicleBrandService>();
builder.Services.AddScoped<IFavoriteService, FavoriteService>();
builder.Services.AddScoped<IAuctionService, AuctionService>();
// Add resitory services
builder.Services.AddScoped<AuctionRepository>();
builder.Services.AddScoped<TransactionRepository>();
builder.Services.AddScoped<PostImageRepository>();
builder.Services.AddScoped<VehilceBrandRepository>();
builder.Services.AddScoped<BatteryBrandRepository>();
builder.Services.AddScoped<PostRepository>();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<WalletRepository>();
builder.Services.AddScoped<UserUtility>();
builder.Services.AddScoped<FavoriteRepositori>();
builder.Services.AddHttpContextAccessor();
// Add Firebase Storage Service
builder.Services.AddScoped<FirebaseStorageService>();
// Add VNPay Service
builder.Services.Configure<VnPayConfig>(builder.Configuration.GetSection("VnPay"));
builder.Services.AddScoped<IPaymentService, PaymentService>();
// Add Auction Background Service
builder.Services.AddHostedService<AuctionBackgroundService>();
// Add session services
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddSwaggerGen(option =>
{
    option.DescribeAllParametersInCamelCase();
    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    option.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

// Thêm CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder
            .AllowAnyOrigin()     
            .AllowAnyMethod()    
            .AllowAnyHeader();   
    });
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// khởi tạo Firebase Admin SDK
var credsPath = Path.Combine(builder.Environment.ContentRootPath, "firebase-adminsdk.json");
if (FirebaseApp.DefaultInstance == null)
{
    FirebaseApp.Create(new AppOptions
    {
        Credential = GoogleCredential.FromFile(credsPath),
    });
}


app.UseCors("AllowAll"); // Sử dụng CORS để cho phép tất cả các nguồn.
// Add Global Exception Handler
app.UseMiddleware<EVMarketPlace.API.Middleware.GlobalException>();

app.UseHttpsRedirection();

// Áp dụng CORS policy
app.UseCors("AllowAll");

// ✅ Thêm middleware
app.UseSession();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

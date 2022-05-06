using ListWish;
using ListWish.Models;
using ListWish.Utility;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddDbContext<ListwishDbContext>(opt => {
    string connStr = builder.Configuration.GetConnectionString("DefaultConnection");
    opt.UseSqlServer(connStr);
    });

builder.Services.AddDataProtection();
builder.Services.AddIdentityCore<ListUser>(opt => { });
IdentityBuilder identityBuilder = new IdentityBuilder(typeof(ListUser), typeof(ListRole), builder.Services);
identityBuilder.AddEntityFrameworkStores<ListwishDbContext>()
    .AddDefaultTokenProviders()
    .AddUserManager<UserManager<ListUser>>()
    .AddRoleManager<RoleManager<ListRole>>();


builder.Services.Configure<JWTOption>(builder.Configuration.GetSection("JWT"));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(x =>
    {
        var jwtOpt = builder.Configuration.GetSection("JWT").Get<JWTOption>();
        byte[] keyBytes = Encoding.UTF8.GetBytes(jwtOpt.SigningKey);
        var secKey = new SymmetricSecurityKey(keyBytes);
        x.TokenValidationParameters = new()
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = secKey
        };
    });

builder.Services.AddSwaggerGen(c =>
{
    var scheme = new OpenApiSecurityScheme()
    {
        Description = "Authorization header",
        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme,
        Id="Authorization"},
        Scheme = "oauth2",Name="Authorization",
        In = ParameterLocation.Header,Type= SecuritySchemeType.ApiKey
    };
    c.AddSecurityDefinition("Authorization", scheme);
    var requirement = new OpenApiSecurityRequirement();
    requirement[scheme] = new List<string>();
    c.AddSecurityRequirement(requirement);
   
});

builder.Services.AddMemoryCache();
builder.Services.Configure<MvcOptions>(opt => {
    opt.Filters.Add<JWTValidationFilter>();
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

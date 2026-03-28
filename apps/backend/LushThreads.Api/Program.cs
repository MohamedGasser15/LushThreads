using LushThreads.Application.Configurations;
using LushThreads.Infrastructure.Configurations;
using LushThreads.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Stripe;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ==============================
// 1. Register Clean Architecture Layers - without Presentation
// ==============================
builder.Services.AddInfrastructureServices(builder.Configuration);   // DbContext, Identity, Repos, Stripe
builder.Services.AddApplicationServices();                           // Services, Cache, Email

// ==============================
// 2. JWT Settings (for API)
// ==============================
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "JwtBearer";
    options.DefaultChallengeScheme = "JwtBearer";
})
.AddJwtBearer("JwtBearer", options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
        NameClaimType = ClaimTypes.Name,
        RoleClaimType = ClaimTypes.Role
    };
});

// Authorization Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("UserOnly", policy => policy.RequireRole("User"));
});

// ==============================
// 3. MVC (Controllers with Views) and API Services
// ==============================
builder.Services.AddControllersWithViews();  // Includes both API and MVC
builder.Services.AddEndpointsApiExplorer();

// ==============================
// 4. Swagger Services for OpenAPI document generation and UI
// ==============================
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "My Combined API",
        Version = "v1",
        Description = "API for both MVC and API endpoints"
    });

    // Configure JWT in Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Please enter JWT token: Bearer {your token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { "Admin", "User" }
        }
    });
});

// ==============================
// 5. CORS (for API)
// ==============================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// ==============================
// 6. Session Services (for MVC)
// ==============================
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddHttpContextAccessor();

// ==============================
// 7. External Authentication (optional)
// ==============================
builder.Services.AddAuthentication()
    .AddFacebook(facebookOptions =>
    {
        facebookOptions.ClientId = builder.Configuration.GetSection("Facebook:ClientId").Value;
        facebookOptions.ClientSecret = builder.Configuration.GetSection("Facebook:ClientSecret").Value;
    })
    .AddGoogle(googleOptions =>
    {
        googleOptions.ClientId = builder.Configuration.GetSection("Google:ClientId").Value;
        googleOptions.ClientSecret = builder.Configuration.GetSection("Google:ClientSecret").Value;
    });

// ==============================
// 8. Stripe Settings
// ==============================
StripeConfiguration.ApiKey = builder.Configuration.GetSection("Stripe:SecretKey").Get<string>();

var app = builder.Build();

// ==============================
// 9. Middleware Pipeline
// ==============================

if (app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseStatusCodePagesWithReExecute("/Home/Error/{0}");
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseStatusCodePagesWithReExecute("/Home/Error/{0}");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

// ==============================
// 10. Swagger Middleware
// ==============================
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
    c.RoutePrefix = "swagger"; // Accessible at /swagger
});

// ==============================
// 11. Route Configuration
// ==============================
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Catch-all for 404
app.MapControllerRoute(
    name: "catch-all",
    pattern: "{*url}",
    defaults: new { controller = "Home", action = "Error", statusCode = 404 });

// API Controllers
app.MapControllers();

// Redirect root to Swagger UI
app.MapGet("/", context =>
{
    context.Response.Redirect("/swagger");
    return Task.CompletedTask;
});

// ==============================
// 12. Seed Database
// ==============================
using (var scope = app.Services.CreateScope())
{
    await DbInitializer.InitializeAsync(scope.ServiceProvider);
}

app.Run();
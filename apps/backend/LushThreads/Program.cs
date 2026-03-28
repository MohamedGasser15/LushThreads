using LushThreads.Application.Configurations;
using LushThreads.Infrastructure.Configurations;
using LushThreads.Infrastructure.Data;
using LushThreads.Web.Configurations;
using Stripe;

var builder = WebApplication.CreateBuilder(args);

// Add logging (already included by default, but explicit for clarity)
builder.Services.AddLogging();

// Register layer services
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddPresentationServices(builder.Configuration);

var app = builder.Build();

// Configure middleware pipeline
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

StripeConfiguration.ApiKey = builder.Configuration.GetSection("Stripe:SecretKey").Get<string>();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{area=Customer}/{controller=Home}/{action=Home}/{id?}");

app.MapControllerRoute(
    name: "catch-all",
    pattern: "{*url}",
    defaults: new { controller = "Home", action = "Error", statusCode = 404 });

// Seed database
using (var scope = app.Services.CreateScope())
{
    await DbInitializer.InitializeAsync(scope.ServiceProvider);
}

app.Run();
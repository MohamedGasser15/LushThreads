using LushThreads.Application.Configurations;
using LushThreads.Configurations;
using LushThreads.Domain.Constants;
using LushThreads.Domain.Entites;
using LushThreads.Infrastructure.Configurations;
using Microsoft.AspNetCore.Identity;
using Stripe;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddApplicationServices();

builder.Services.AddPresentationServices(builder.Configuration);

var app = builder.Build();

// تكوين middleware
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

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    string adminEmail = "admin@example.com";
    string adminPassword = "Admin1234@";

    if (!await roleManager.RoleExistsAsync(SD.Admin))
    {
        await roleManager.CreateAsync(new IdentityRole(SD.Admin));
        await roleManager.CreateAsync(new IdentityRole(SD.User));
    }

    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true,
            Name = "Admin",
        };
        var result = await userManager.CreateAsync(adminUser, adminPassword);

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
            Console.WriteLine("Admin user created successfully!");
        }
        else
        {
            Console.WriteLine("Error creating admin user:");
            foreach (var error in result.Errors)
            {
                Console.WriteLine($"- {error.Description}");
            }
        }
    }
    else
    {
        Console.WriteLine("Admin user already exists.");
    }
}


app.Run();
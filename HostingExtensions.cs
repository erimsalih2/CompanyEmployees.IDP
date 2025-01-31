using Serilog;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using CompanyEmployees.IDP.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity;
using EmailService;
using CompanyEmployees.IDP.CustomTokenProviders;

namespace CompanyEmployees.IDP;

internal static class HostingExtensions
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        // uncomment if you want to add a UI
        builder.Services.AddRazorPages();
        builder.Services.AddAutoMapper(typeof(Program));
        var emailConfig = builder.Configuration
        .GetSection("EmailConfiguration")
        .Get<EmailConfiguration>();
        builder.Services.AddSingleton(emailConfig);
        builder.Services.AddScoped<IEmailSender, EmailSender>();
        var migrationAssembly = typeof(Program).GetTypeInfo().Assembly.GetName().Name;
        builder.Services.AddDbContext<UserContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("identityPostgreConnection")));
        builder.Services.AddIdentity<User, IdentityRole>(opt =>
        {
            opt.Password.RequireDigit = false;
            opt.Password.RequiredLength = 7;
            opt.Password.RequireUppercase = false;
            opt.User.RequireUniqueEmail = true;
            opt.SignIn.RequireConfirmedEmail = true;
            opt.Tokens.EmailConfirmationTokenProvider = "emailconfirmation";
        })
        .AddEntityFrameworkStores<UserContext>()
        .AddDefaultTokenProviders()
        .AddTokenProvider<EmailConfirmationTokenProvider<User>>("emailconfirmation"); 
        builder.Services.AddIdentityServer(options =>
            {
                
                options.EmitStaticAudienceClaim = true;
            })
            
            .AddConfigurationStore(opt =>
            {
                opt.ConfigureDbContext = c =>
               c.UseNpgsql(builder.Configuration.GetConnectionString("postgreConnection"),
                sql => sql.MigrationsAssembly(migrationAssembly));
            })
             .AddOperationalStore(opt =>
             {
                 opt.ConfigureDbContext = o =>
                o.UseNpgsql(builder.Configuration.GetConnectionString("postgreConnection"),
                 sql => sql.MigrationsAssembly(migrationAssembly));
             }).AddAspNetIdentity<User>();


        builder.Services.Configure<DataProtectionTokenProviderOptions>(opt =>
         opt.TokenLifespan = TimeSpan.FromHours(2));

        builder.Services.Configure<EmailConfirmationTokenProviderOptions>(opt =>
         opt.TokenLifespan = TimeSpan.FromDays(3));


        return builder.Build();
    }
    
    public static WebApplication ConfigurePipeline(this WebApplication app)
    { 
        app.UseSerilogRequestLogging();
    
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        // uncomment if you want to add a UI
        app.UseStaticFiles();
        app.UseRouting();
            
        app.UseIdentityServer();

        // uncomment if you want to add a UI
        app.UseAuthorization();
        app.MapRazorPages().RequireAuthorization();

        return app;
    }
}

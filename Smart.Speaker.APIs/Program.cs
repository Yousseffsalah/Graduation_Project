using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Smart.Speaker.APIs.Errors;
using Smart.Speaker.APIs.Middlewares;
using Smart.Speaker.Core.Entities.Identity;
using Smart.Speaker.Core.Service;
using Smart.Speaker.Repository.Identity;
using Smart.Speaker.Services;
using System.Text;

namespace Smart.Speaker.APIs
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            #region Configure Services Add services to the container

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.Configure<ApiBehaviorOptions>(Options =>
            {
                Options.InvalidModelStateResponseFactory = (actionContext) =>
                {
                    var errors = actionContext.ModelState.Where(P => P.Value.Errors.Count() > 0)
                                                         .SelectMany(P => P.Value.Errors)
                                                         .Select(E => E.ErrorMessage)
                                                         .ToArray();

                    var ValidationErrorResponse = new ApiValidationErrorResponse()
                    {
                        Errors = errors
                    };

                    return new BadRequestObjectResult(ValidationErrorResponse);
                };
            });

            builder.Services.AddDbContext<AppIdentityDbContext>(Options =>
            {
                Options.UseSqlServer(builder.Configuration.GetConnectionString("IdentityConnection"));
            });

            builder.Services.AddIdentity<AppUser, IdentityRole>(Options =>
            {
                Options.Password.RequireNonAlphanumeric = true; // @ # $
                Options.Password.RequireDigit = true; // 123
                Options.Password.RequireUppercase = true; // ASD
                Options.Password.RequireLowercase = true; // asd
            }).AddEntityFrameworkStores<AppIdentityDbContext>();

            //Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            builder.Services.AddAuthentication(Options =>
            {
                Options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                Options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                    .AddJwtBearer(Options =>
                    {
                        Options.TokenValidationParameters = new TokenValidationParameters()
                        {
                            ValidateIssuer = true,
                            ValidIssuer = builder.Configuration["JWT:ValidIssure"],
                            ValidateAudience = true,
                            ValidAudience = builder.Configuration["JWT:ValidAudience"],
                            ValidateLifetime = true,
                            ValidateIssuerSigningKey = true,
                            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"]))
                        };
                    });

            builder.Services.AddScoped<ITokenService, TokenService>();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("Policy", options =>
                {
                    options.AllowAnyHeader();
                    options.AllowAnyOrigin();
                    options.AllowAnyMethod();
                });
            });

            #endregion

            var app = builder.Build();

            #region Update-Database

            using var Scope = app.Services.CreateScope(); // Group Of Services LifeTime Scoped
            var Services = Scope.ServiceProvider; // Services It's Self
            var LoggerFactory = Services.GetRequiredService<ILoggerFactory>();

            try
            {
                var IdentityDbContext = Services.GetRequiredService<AppIdentityDbContext>();
                // ASK CLR For Creating Object From DbContext Explicitly

                await IdentityDbContext.Database.MigrateAsync(); // Update-Database

                var UserManager = Services.GetRequiredService<UserManager<AppUser>>();
                await AppIdentityDbContextSeed.SeedUserAsync(UserManager);
            }
            catch (Exception ex)
            {
                var Logger = LoggerFactory.CreateLogger<Program>();
                Logger.LogError(ex, "An Error Occured During Appling The Migration");
            }


            #endregion

            #region Configure - Configure the HTTP request pipeline.

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseMiddleware<ExceptionMiddleWare>();

            app.UseStatusCodePagesWithReExecute("/errors/{0}");

            app.UseHttpsRedirection();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseStaticFiles();

            app.UseCors("Policy");

            app.MapControllers();

            #endregion

            app.Run();
        }
    }
}

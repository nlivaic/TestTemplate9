using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Azure.Monitor.OpenTelemetry.Exporter;
using FluentValidation.AspNetCore;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using SparkRoseDigital.Infrastructure.Caching;
using SparkRoseDigital.Infrastructure.HealthCheck;
using SparkRoseDigital.Infrastructure.Logging;
using TestTemplate9.Api.Helpers;
using TestTemplate9.Api.Middlewares;
using TestTemplate9.Application;
using TestTemplate9.Core;
using TestTemplate9.Data;

namespace TestTemplate9.Api
{
    public class Startup
    {
        private readonly IConfiguration _configuration;
        private readonly IHostEnvironment _hostEnvironment;

        public Startup(IConfiguration configuration, IHostEnvironment hostEnvironment)
        {
            _configuration = configuration;
            _hostEnvironment = hostEnvironment;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddControllers(configure =>
                {
                    configure.ReturnHttpNotAcceptable = true;
                    configure.Filters.Add(new ProducesResponseTypeAttribute(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest));
                    configure.Filters.Add(new ProducesResponseTypeAttribute(typeof(ProblemDetails), StatusCodes.Status404NotFound));
                    configure.Filters.Add(new ProducesResponseTypeAttribute(typeof(object), StatusCodes.Status406NotAcceptable));
                    configure.Filters.Add(new ProducesResponseTypeAttribute(StatusCodes.Status500InternalServerError));
                })
                .ConfigureApiBehaviorOptions(options =>
                {
                    options.InvalidModelStateResponseFactory = actionContext =>
                    {
                        var actionExecutingContext = actionContext as ActionExecutingContext;
                        var validationProblemDetails = ValidationProblemDetailsFactory.Create(actionContext);
                        if (actionContext.ModelState.ErrorCount > 0
                            && actionExecutingContext?.ActionArguments.Count == actionContext.ActionDescriptor.Parameters.Count)
                        {
                            validationProblemDetails.Status = StatusCodes.Status422UnprocessableEntity;
                            return new UnprocessableEntityObjectResult(validationProblemDetails);
                        }
                        validationProblemDetails.Status = StatusCodes.Status400BadRequest;
                        return new BadRequestObjectResult(validationProblemDetails);
                    };
                })
                .AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<Startup>());

            services.AddDbContext<TestTemplate9DbContext>(options =>
            {
                //var connString = new SqlConnectionStringBuilder(_configuration.GetConnectionString("TestTemplate9DbConnection") ?? string.Empty)
                //{
                //    UserID = _configuration["DB_USER"] ?? string.Empty,
                //    Password = _configuration["DB_PASSWORD"] ?? string.Empty
                //};
                var connString = new SqlConnectionStringBuilder("Server=tcp:wedevtesttemplate9sql1.database.windows.net,1433;Initial Catalog=wedevtesttemplate9sql1db1;Persist Security Info=False;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;")
                {
                    Authentication = SqlAuthenticationMethod.ActiveDirectoryManagedIdentity
                };
                options.UseSqlServer(connString.ConnectionString);
                if (_hostEnvironment.IsDevelopment())
                {
                    options.EnableSensitiveDataLogging(true);
                }
            });

            services.AddGenericRepository();
            services.AddSpecificRepositories();
            services.AddCoreServices();

            services
                .AddAuthentication("Bearer")
                .AddJwtBearer(options =>
                {
                    options.Authority = _configuration["AUTH:AUTHORITY"];
                    options.Audience = _configuration["AUTH:AUDIENCE"];
                    options.TokenValidationParameters.ValidIssuer = _configuration["AUTH:VALID_ISSUER"];
                });

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddLoggingScopes();
            if (!string.IsNullOrEmpty(_configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]))
            {
                services
                    .AddOpenTelemetry()
                    .WithTracing(tracerProviderBuilder =>
                    {
                        tracerProviderBuilder
                            .AddSource(ApiAssemblyInfo.Value.GetName().Name)
                            .SetResourceBuilder(
                                ResourceBuilder
                                    .CreateDefault()
                                    .AddService(serviceName: ApiAssemblyInfo.Value.GetName().Name))
                            .AddAspNetCoreInstrumentation()
                            .AddEntityFrameworkCoreInstrumentation()
                            .AddSqlClientInstrumentation()
                            .AddSource("MassTransit")
                            .AddAzureMonitorTraceExporter(o =>
                            {
                                o.ConnectionString = _configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];
                            });
                    })
                    // Not supported by Application Insights yet.
                    //.WithMetrics(meterProviderBuilder =>
                    //{
                    //    meterProviderBuilder
                    //        .SetResourceBuilder(
                    //            ResourceBuilder
                    //                .CreateDefault()
                    //                .AddService(serviceName: "TestTemplate9"))
                    //        .AddAspNetCoreInstrumentation()
                    //        .AddAzureMonitorMetricExporter(o =>
                    //        {
                    //            //o.ConnectionString = "InstrumentationKey=f051d7dd-dbaf-450a-a6f1-9f78bc0f8c91";
                    //            o.ConnectionString = "InstrumentationKey=f051d7dd-dbaf-450a-a6f1-9f78bc0f8c91;IngestionEndpoint=https://westeurope-5.in.applicationinsights.azure.com/;LiveEndpoint=https://westeurope.livediagnostics.monitor.azure.com/";
                    //        })
                    //        .AddConsoleExporter();
                    //})
                    .StartWithHost();
            }

            services.AddAutoMapper(Assembly.GetExecutingAssembly(), typeof(Startup).Assembly);

            services.AddSingleton<ICache, Cache>();
            services.AddMemoryCache();

            services.AddSwaggerGen(setupAction =>
            {
                setupAction.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Name = "Authorization",
                    Description = "Bearer Authentication with JWT Token",
                    Type = SecuritySchemeType.Http
                });
                setupAction.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Id = "Bearer",
                                Type = ReferenceType.SecurityScheme
                            }
                        },
                        new List<string>()
                    }
                });
                setupAction.SwaggerDoc(
                    "TestTemplate9OpenAPISpecification",
                    new OpenApiInfo
                    {
                        Title = "TestTemplate9 API",
                        Version = "v1",
                        Description = "This API allows access to TestTemplate9.",
                        Contact = new OpenApiContact
                        {
                            Name = "Author Name",
                            Url = new Uri("https://github.com")
                        },
                        License = new OpenApiLicense
                        {
                            Name = "MIT",
                            Url = new Uri("https://www.opensource.org/licenses/MIT")
                        },
                        TermsOfService = new Uri("https://www.my-terms-of-service.com")
                    });

                // A workaround for having multiple POST methods on one controller.
                // setupAction.ResolveConflictingActions(r => r.First());
                setupAction.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "TestTemplate9.Api.xml"));
            });

            // Commented out as we are running front end as a standalone app.
            // services.AddSpaStaticFiles(configuration =>
            // {
            //     configuration.RootPath = "ClientApp/build";
            // });
            services.AddCors(o => o.AddPolicy("All", builder =>
            {
                builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .WithExposedHeaders(Constants.Headers.Pagination);
            }));

            services.AddCors(o => o.AddPolicy("TestTemplate9Client", builder =>
            {
                var allowedOrigins = _configuration["AllowedOrigins"]?.Split(',') ?? Array.Empty<string>();
                builder
                    .WithOrigins(allowedOrigins)
                    .WithHeaders("Authorization", "Content-Type")
                    .WithExposedHeaders(Constants.Headers.Pagination)
                    .WithMethods(HttpMethods.Get, HttpMethods.Post, HttpMethods.Put, HttpMethods.Delete);
            }));

            services.AddMassTransit(x =>
            {
                if (string.IsNullOrEmpty(_configuration.GetConnectionString("MessageBroker")))
                {
                    x.UsingInMemory();
                }
                else
                {
                    x.UsingAzureServiceBus((ctx, cfg) =>
                    {
                        cfg.Host(_configuration.GetConnectionString("MessageBroker"));

                        // Use the below line if you are not going with SetKebabCaseEndpointNameFormatter() above.
                        // Remember to configure the subscription endpoint accordingly (see WorkerServices Program.cs).
                        // cfg.Message<VoteCast>(configTopology => configTopology.SetEntityName("vote-cast-topic"));
                    });
                }
                x.AddEntityFrameworkOutbox<TestTemplate9DbContext>(o =>
                {
                    // configure which database lock provider to use (Postgres, SqlServer, or MySql)
                    o.UseSqlServer();

                    // enable the bus outbox
                    o.UseBusOutbox();
                    o.QueryDelay = TimeSpan.FromSeconds(15);
                });
            });
            services.AddTestTemplate9ApplicationHandlers();

            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });

            services
                .AddHealthChecks()
                .AddDbContextCheck<TestTemplate9DbContext>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            app.UseHostLoggingMiddleware();

            // First use of Logging Exceptions.
            // This instance is here to catch and log any exceptions coming from middlewares
            // executed early in the pipeline.
            app.UseApiExceptionHandler(options =>
            {
                options.ApiErrorHandler = UpdateApiErrorResponse;
                options.LogLevelHandler = LogLevelHandler;
            });

            // Use headers forwarded by reverse proxy.
            app.UseForwardedHeaders();

            // if (env.IsProduction())
            // {
            //    app.UseHsts();
            // }
            app.UseCors("TestTemplate9Client");
            app.UseHttpsRedirection();

            // Commented out as we are running front end as a standalone app.
            // app.UseSpaStaticFiles();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/TestTemplate9OpenAPISpecification/swagger.json", "TestTemplate9 API");
                c.RoutePrefix = string.Empty;
            });

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseUserLoggingMiddleware();

            // Second use of Logging Exceptions.
            // This instance is here to catch and log any exceptions coming from the controllers.
            // The reason for two logging middlewares is we can log user id and claims only
            // after .UseAuthentication() and .UseAuthorization() are executed. So the first
            // .UseApiExceptionHandler() has no access to user id and claims but has access to
            // machine name and thus at least provides some insight into any potential exceptions
            // coming from early in the pipeline. The second .UseApiExceptionHandler() has access
            // to machine name, user id and claims and can log any exceptions from the controllers.
            app.UseApiExceptionHandler(options =>
            {
                options.ApiErrorHandler = UpdateApiErrorResponse;
                options.LogLevelHandler = LogLevelHandler;
            });

            app.UseEndpoints(endpoints =>
            {
                // Liveness check does not include database connectivity check because even a transient
                // error will cause the orchestractor/load balancer to take the service down and restart it.
                // Readiness check includes database connectivity check to tell the orchestractor/load balancer
                // whether all the project dependencies are up and running.
                endpoints.MapHealthChecks("/health/live", new HealthCheckOptions
                {
                    Predicate = _ => false, // No additional health checks.
                });
                endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions
                {
                    ResponseWriter = HealthCheckResponses.WriteJsonResponse
                });
                endpoints.MapControllers();
            });

            // Commented out as we are running front end as a standalone app.
            // app.UseSpa(spa =>
            // {
            //     spa.Options.SourcePath = "ClientApp";
            //     if (env.IsDevelopment())
            //     {
            //         // This is used if starting both front end and back end with the same command.
            //         // spa.UseReactDevelopmentServer(npmScript: "start");
            //         // This is used if starting front end separately from the back end, most likely to get better
            //         // separation. Faster hot reload when changing only front end and not having to go through front end
            //         // rebuild every time you change something on the back end.
            //         spa.UseProxyToSpaDevelopmentServer("http://localhost:3000");
            //     }
            // });
        }

        /// <summary>
        /// A demonstration of how returned message can be modified.
        /// </summary>
        private void UpdateApiErrorResponse(HttpContext context, Exception ex, ProblemDetails problemDetails)
        {
            // if (ex is LimitNotMappable)
            // {
            //     problemDetails.Detail = "A general error occurred.";
            // }
        }

        /// <summary>
        /// Define cases where a different log level is needed for logging exceptions.
        /// </summary>
        private LogLevel LogLevelHandler(HttpContext context, Exception ex) =>

            // if (ex is Exception)
            // {
            //     return LogLevel.Critical;
            // }
            // return LogLevel.Error;
            LogLevel.Critical;
    }
}

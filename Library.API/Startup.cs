using AutoMapper;
using Library.API.Contexts;
using Library.API.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
//[assembly: ApiConventionType(typeof(DefaultApiConventions))]            //Adds the Deafult API Conventions to all Controllers in the Project. These can be overriden by ProducesResponseType Attribute

namespace Library.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(setupAction =>
            {
                //Adds these 3 Response Types as Default to all API Methods of both the API Controllers.
                setupAction.Filters.Add(new ProducesResponseTypeAttribute(StatusCodes.Status400BadRequest));
                setupAction.Filters.Add(new ProducesResponseTypeAttribute(StatusCodes.Status406NotAcceptable));
                setupAction.Filters.Add(new ProducesResponseTypeAttribute(StatusCodes.Status500InternalServerError));

                setupAction.ReturnHttpNotAcceptable = true;     //This is used to return an Error incase the Accept Header Type in Response is not supported

                setupAction.OutputFormatters.Add(new XmlSerializerOutputFormatter());

                var jsonOutputFormatter = setupAction.OutputFormatters
                    .OfType<JsonOutputFormatter>().FirstOrDefault();

                if (jsonOutputFormatter != null)
                {
                    // remove text/json as it isn't the approved media type
                    // for working with JSON at API level
                    if (jsonOutputFormatter.SupportedMediaTypes.Contains("text/json"))
                    {
                        jsonOutputFormatter.SupportedMediaTypes.Remove("text/json");
                    }
                }
            })
            .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            // register the DbContext on the container, getting the connection string from
            // appSettings (note: use this during development; in a production environment,
            // it's better to store the connection string in an environment variable)
            var connectionString = Configuration["ConnectionStrings:LibraryDBConnectionString"];
            services.AddDbContext<LibraryContext>(o => o.UseSqlServer(connectionString));

            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = actionContext =>
                {
                    var actionExecutingContext =
                        actionContext as Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext;

                    // if there are modelstate errors & all keys were correctly
                    // found/parsed we're dealing with validation errors
                    if (actionContext.ModelState.ErrorCount > 0
                        && actionExecutingContext?.ActionArguments.Count == actionContext.ActionDescriptor.Parameters.Count)
                    {
                        return new UnprocessableEntityObjectResult(actionContext.ModelState);
                    }

                    // if one of the keys wasn't correctly found / couldn't be parsed
                    // we're dealing with null/unparsable input
                    return new BadRequestObjectResult(actionContext.ModelState);
                };
            });

            services.AddScoped<IBookRepository, BookRepository>();
            services.AddScoped<IAuthorRepository, AuthorRepository>();

            services.AddAutoMapper();

            services.AddVersionedApiExplorer(setupAction => 
            {
                setupAction.GroupNameFormat = "'v'VV";            
            });
            
            services.AddApiVersioning(setupAction =>
            {
                setupAction.AssumeDefaultVersionWhenUnspecified = true;
                setupAction.DefaultApiVersion = new ApiVersion(1, 0);
                setupAction.ReportApiVersions = true;

                //setupAction.ApiVersionReader = new HeaderApiVersionReader("api-version");
                //setupAction.ApiVersionReader = new MediaTypeApiVersionReader();
            });

            var apiVersionDescriptionProvider = services.BuildServiceProvider().GetService<IApiVersionDescriptionProvider>();

            services.AddSwaggerGen(setupAction =>
            {
                foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions)       //Lopps through all the Defined Versions and accordingly creates a Swagger Document for each Version
                {
                    setupAction.SwaggerDoc(
                        $"LibraryOpenAPISpecification{description.GroupName}",
                    new Microsoft.OpenApi.Models.OpenApiInfo()              //Adds General Info for the API
                    {
                        Title = "Library API",
                        Version = description.ApiVersion.ToString(),
                        Description = "Through this API you can access authors and their books.",
                        Contact = new Microsoft.OpenApi.Models.OpenApiContact()
                        {
                            Email = "derekvenom@gmail.com",
                            Name = "Derrick Nazareth",
                            Url = new Uri("https://www.twitter.com/derekvenom")
                        },
                        License = new Microsoft.OpenApi.Models.OpenApiLicense()
                        {
                            Name = "MIT License",
                            Url = new Uri("https://opensource.org/licenses/MIT")
                        }
                    });
                }

                setupAction.DocInclusionPredicate((documentName, apiDescription) =>             //This Part is responsible for Inclduing Methods of the Controllers under the respective Versions
                {
                   var actionApiVersionModel = apiDescription.ActionDescriptor                 //Gets the applicable Version for each Action Method i.e. to which Version it belongs.
                    .GetApiVersionModel(ApiVersionMapping.Explicit | ApiVersionMapping.Implicit);       //Explicit meaning defined explicitly on the Controller or Implicitly through Default Version
                
                   if (actionApiVersionModel == null)
                   {
                        return true;
                   }

                   if (actionApiVersionModel.DeclaredApiVersions.Any())                             //If there are any Declared API Versions then we construct a Corresponding Document Specification
                   {
                        return actionApiVersionModel.DeclaredApiVersions.Any(v =>
                       $"LibraryOpenAPISpecificationv{v.ToString()}" == documentName);
                   }

                   return actionApiVersionModel.ImplementedApiVersions.Any(v =>                      //Similar;y If there are any Implemented API Versions then we construct a Corresponding Document Specification
                          $"LibraryOpenAPISpecificationv{v.ToString()}" == documentName);
                
                });
                var xmlCommentsFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlCommentsFullPath = Path.Combine(AppContext.BaseDirectory, xmlCommentsFile);

                setupAction.IncludeXmlComments(xmlCommentsFullPath);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApiVersionDescriptionProvider apiVersionDescriptionProvider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. 
                // You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseSwagger();           //Adds Swagger to the Pipeline to generate OpenAPI Specification

            app.UseSwaggerUI(setupAction =>     //Adds UI using the generateed OpenAPI Specification
            {
                setupAction.RoutePrefix = "";                                   //This Sets the Path at which the Swagger UI will be available. We have set it at the Index of the App.
                
                foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions)           //Lopps through all the Defined Versions and accordingly creates a Swagger Endpoint for each Version
                {
                    setupAction.SwaggerEndpoint($"/swagger/" +
                        $"LibraryOpenAPISpecification{description.GroupName}/swagger.json",
                        description.GroupName.ToUpperInvariant());
                }
                
                
                //setupAction.SwaggerEndpoint(
                //    "/swagger/LibraryOpenAPISpecification/swagger.json",        //Endpoint where the Specification is generated
                //    "Library API");
                
                setupAction.DefaultModelExpandDepth(2);
                setupAction.DefaultModelRendering(Swashbuckle.AspNetCore.SwaggerUI.ModelRendering.Model);
                //setupAction.DefaultModelsExpandDepth(-1);                       //Hides all the Schemas
            });

            app.UseStaticFiles();

            app.UseMvc();
        }
    }
}

﻿using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Steeltoe.Discovery.Client;
using PivotalServices.WebApiTemplate.CSharp.Bootstrap;
using PivotalServices.WebApiTemplate.CSharp.Extensions;
using PivotalServices.WebApiTemplate.CSharp.Features.Values;
using Steeltoe.CloudFoundry.Connector;
using Steeltoe.Management.CloudFoundry;

namespace PivotalServices.WebApiTemplate.CSharp
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
            services.AddOptions();

            if (Configuration.HasCloudFoundryServiceConfigurations())
            {
                services.AddDiscoveryClient(Configuration);
            }

            services.AddMediatR(typeof(Startup).Assembly);
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            services.AddActuatorsAndHealthContributors(Configuration);
            services.AddMvc().AddFluentValidation(fv => { fv.RegisterValidatorsFromAssemblyContaining<GetValues>(); });

            
            services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new OpenApiInfo {Title = "Values API", Version = "v1"}); });

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IConfiguration configuration)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();
            else
                app.UseHsts();

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Form API V1");
                c.RoutePrefix = "swagger";
            });

            app.UseMiddleware<ValidationExceptionMiddleware>();
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            if (configuration.HasCloudFoundryServiceConfigurations())
            {
                app.UseDiscoveryClient();
                app.UseCloudFoundryActuators();
            }
        }
    }
}
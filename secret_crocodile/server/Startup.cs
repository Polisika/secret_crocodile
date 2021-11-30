using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace server
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                int count = -1;
                endpoints.MapGet("/", async context =>
                {
                    try
                    {
                        Session session = new Session(count);
                        await context.Response.WriteAsync(count.ToString());
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        await context.Response.WriteAsync("Need more players.");
                    }
                });

                endpoints.MapGet("/entry", async context => {
                    count++;
                    await context.Response.WriteAsync(count.ToString());
                });
            });
        }
    }
}

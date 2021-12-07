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
                Session session = new Session();
                endpoints.MapGet("/start", async context =>
                {
                    context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                    try
                    {
                        session.StartSession(count);
                        await context.Response.WriteAsync(count.ToString());
                        Console.WriteLine("Session started");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        await context.Response.WriteAsync("Need more players.");
                        Console.WriteLine("Session not started");
                    }
                });

                endpoints.MapGet("/entry", async context => {
                    if (!(session.players is null))
                        await context.Response.WriteAsync("Session started");
                    count++;
                    Console.WriteLine("����� ����� ������! ��� �����: " + count.ToString());
                    context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                    await context.Response.WriteAsync(count.ToString());
                });

                endpoints.MapGet("/get_role/{num:int}", async context =>
                {
                    context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                    string param = (string)context.Request.RouteValues["num"];
                    Console.WriteLine("������ ������: num=" + param);
                    await context.Response.WriteAsync(session.players[int.Parse(param)].role.ToString());
                });

                endpoints.MapGet("/is_started", async context =>
                {
                    context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                    Console.WriteLine("������� ��������");
                    if (!(session.players is null))
                    {
                        await context.Response.WriteAsync(count.ToString());
                        Console.WriteLine("������ ����������");
                    }
                    else
                    {
                        await context.Response.WriteAsync("0");
                    }
                });

                endpoints.MapGet("/get_num/{role:int}", async context =>
                {
                    Console.WriteLine("������ �� �������� ���� �����...");
                    context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                    int i = 0;
                    int croco = 0;
                    int faschist = 0;
                    foreach (var player in session.players)
                    {
                        if (player.role == RoleType.Crokodile)
                            croco = i;
                        else if (player.role == RoleType.Fascist)
                            faschist = i;
                        i++;
                    }
                    string role = (string)context.Request.RouteValues["role"];
                    if (role == "0")
                        await context.Response.WriteAsync(faschist.ToString());
                    else if (role == "1")
                        await context.Response.WriteAsync(croco.ToString());
                    else
                    {
                        Console.WriteLine("���� �������� �������� :(");
                        await context.Response.WriteAsync("OMG cheater");
                    }
                });
            });
        }
    }
}

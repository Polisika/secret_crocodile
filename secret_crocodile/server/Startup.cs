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
    // C:\Users\karasyuk.2018\source\repos\secret_crocodile\secret_crocodile\server\bin\Debug\netcoreapp3.1
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
                int l = 0;
                Session session = new Session();
                endpoints.MapGet("/start", async context =>
                {
                    context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                    try
                    {
                        session.StartSession(count);
                        await context.Response.WriteAsync(count.ToString());
                        Task k = session.play();
                        await Task.Delay(500);
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
                    Console.WriteLine("Зашёл новый чубрик! Его номер: " + count.ToString());
                    context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                    await context.Response.WriteAsync(count.ToString());
                });
                endpoints.MapGet("/get_role/{num:int}", async context =>
                {
                    context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                    string param = (string)context.Request.RouteValues["num"];
                    Console.WriteLine("Пришел запрос: num=" + param);
                    await context.Response.WriteAsync(session.players[int.Parse(param)].role.ToString());
                });
                endpoints.MapGet("/is_started", async context =>
                {
                    context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                    Console.WriteLine("Чубрики стучатся");
                    if (!(session.players is null))
                    {
                        await context.Response.WriteAsync(count.ToString());
                        Console.WriteLine("Чубрик достучался");
                    }
                    else
                    {
                        await context.Response.WriteAsync("0");
                    }
                });
                endpoints.MapGet("/get_num/{role:int}", async context =>
                {
                    Console.WriteLine("Пришли за номерами этих ребят...");
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
                        Console.WriteLine("Меня пытаются взломать :(");
                        await context.Response.WriteAsync("OMG cheater");
                    }
                });
                endpoints.MapGet("/get_info/{num:int}", async context =>
                {
                    context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                    int num = int.Parse((string)context.Request.RouteValues["num"]);

                    string info = "";
                    var player = session.players[num];

                    if (player == null)
                    {
                        if (l++ % 25 == 0)
                            Console.WriteLine("Игрок убит, информации нет.");
                        return;
                    }

                    if (player.isChancellor)
                        info += "1";
                    else
                        info += "0";

                    if (player.isPresident)
                        info += "1";
                    else
                        info += "0";

                    if (player.wereCancellor)
                        info += "1";
                    else
                        info += "0";

                    foreach (var card in player.cards)
                        if (card.isLiberal)
                            info += "1";
                        else
                            info += "0";
                    if (info.Length != 3)
                        Console.WriteLine("Пришли за информацией: " + num.ToString() + ". Вернул: " + info);

                    await context.Response.WriteAsync(info);
                });
                endpoints.MapGet("/set_cancellor/{num:int}", async context =>
                {
                    context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                    int num = int.Parse((string)context.Request.RouteValues["num"]);
                    Console.WriteLine("Канцлер выбрал: " + num.ToString());
                    if (session.players[num].wereCancellor)
                        await context.Response.WriteAsync("omfg");
                    else
                    {
                        Console.WriteLine("Назначил " + num.ToString() + " канцлером");
                        session.President.PlayerNumCancellor = num;
                        await context.Response.WriteAsync("ok");
                    }
                });
                endpoints.MapGet("/drop_card/{card:int}/{num:int}", async context =>
                {
                    context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                    int card = int.Parse((string)context.Request.RouteValues["card"]);
                    int num = int.Parse((string)context.Request.RouteValues["num"]);
                    
                    session.players[num].DropCard(card);

                    Console.WriteLine("Игрок " + num.ToString() + " дропнул карту " + card.ToString());
                    await context.Response.WriteAsync("ok");
                });
                endpoints.MapGet("/send_vote_veto/{num:int}/{vote:int}", async context =>
                {
                    context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                    int vote = int.Parse((string)context.Request.RouteValues["vote"]);
                    int num = int.Parse((string)context.Request.RouteValues["num"]);

                    session.players[num].vote = vote == 1;
                    await context.Response.WriteAsync("ok");
                    Console.WriteLine("Голос за право Вето пришел: " + vote.ToString() + " " + num.ToString());
                });
                endpoints.MapGet("/who_president", async context =>
                {
                    context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                    string res = "";
                    if (session.President != null)
                        res += session.President.num.ToString();
                    else
                        res += "9";
                    if (session.Cancellor != null)
                        res += session.Cancellor.num.ToString();
                    else
                        res += "9";
                    res += session.whowin.ToString();
                    await context.Response.WriteAsync(res);
                });
                endpoints.MapGet("/send_vote_kill/{num:int}", async context => { 
                    session.President.KillPlayer = int.Parse((string)context.Request.RouteValues["num"]);
                });
            });
        }
    }
}

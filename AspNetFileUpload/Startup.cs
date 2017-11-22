using System;
using System.Diagnostics;
using System.Text;
using AspNetFileUpload.Helpers;
using AspNetFileUpload.Models;
using AspNetFileUpload.Rabbit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace AspNetFileUpload
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
            services.AddSingleton(Configuration);
            
            services.AddSingleton<IMessageQueueAccessLayer, RabbitMqAccessLayer>();
            
            services.AddCors();

            services.AddMvc();

            services.AddDbContext<DatabaseContext>(options => options.UseSqlServer(Configuration["Data:ConnectionString"]));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors(cfg =>
            {
                cfg.AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowAnyOrigin();
            });

            SetupRabbitConsumer();

            app.UseMvc();
        }

        private void SetupRabbitConsumer()
        {
            var factory = new ConnectionFactory
            {
                HostName = Configuration["RabbitMq:HostName"].Default("localhost"),
                Port = Configuration["RabbitMq:Port"].ToInt(5672),
                UserName = Configuration["RabbitMq:UserName"].Default("guest"),
                Password = Configuration["RabbitMq:Password"].Default("guest")
            };

            var connection = factory.CreateConnection();

            var channel = connection.CreateModel();

            channel.ExchangeDeclare(exchange: "moorea", type: "direct");
            var queueName = channel.QueueDeclare().QueueName;
            channel.QueueBind(queue: queueName, exchange: "moorea", routingKey: "moorea-messages");
            channel.QueueBind(queue: queueName, exchange: "moorea", routingKey: "moorea-actions");

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                try
                {
                    var dbOptions = new DbContextOptionsBuilder<DatabaseContext>().UseSqlServer(Configuration["Data:ConnectionString"]).Options;
                    using (var dbContext = new DatabaseContext(dbOptions))
                    {
                        var body = ea.Body;
                        var message = Encoding.UTF8.GetString(body);

                        if (message.StartsWith("{"))
                        {
                            var action = JsonConvert.DeserializeObject<IMessageQueueBaseAction>(message);
                            Console.WriteLine("{0} received action: {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm.ss"), action.Action);
                                
                            var sw = new Stopwatch();
                            sw.Start();
                                
                            if (ActionExecuter.Execute(dbContext, action))
                                channel.BasicAck(ea.DeliveryTag, false);

                            sw.Stop();
                            Console.WriteLine($"  tempo di esecuzione: {sw.ElapsedMilliseconds} ms");
                        }
                        else
                        {
                            Console.WriteLine("- received message: {0}", message);
                            channel.BasicAck(ea.DeliveryTag, false);                                
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("errore in gestione messaggio: {0}", e.Message);
                }
            };
            channel.BasicConsume(queueName, false, consumer);
        }
    }
}
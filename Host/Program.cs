using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Owin.Host.HttpListener;
using OwinExamples;

namespace Host
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var properties = new Dictionary<string, object>{
                {"host.Addresses", new [] { new Dictionary<string, object> {
                    {"port", "1337"},
                    {"host", "localhost"}
                } as IDictionary<string,object> }.ToList()}
            };

            //var server = Server.AppFuncServer();
            var server = Server.MiddlewareFuncServer();

            var cancelWait = new ManualResetEvent(false);

            Console.CancelKeyPress += (sender, e) => { cancelWait.Set(); };
            using (OwinServerFactory.Create(server, properties))
            {
                Console.WriteLine("Listening on http://localhost:1337");
                cancelWait.WaitOne();
            }
        }
    }
}
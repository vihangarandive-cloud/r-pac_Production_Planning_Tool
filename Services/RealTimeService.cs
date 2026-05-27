using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RPACProductionPlanner.Services
{
    public static class RealTimeService
    {
        private static readonly ConcurrentQueue<string> _updates = new ConcurrentQueue<string>();
        private static readonly List<HttpResponseBase> _clients = new List<HttpResponseBase>();
        private static readonly object _lock = new object();

        public static void NotifyUpdate(string message)
        {
            lock (_lock)
            {
                var inactiveClients = new List<HttpResponseBase>();
                foreach (var client in _clients)
                {
                    try
                    {
                        client.Write($"data: {message}\n\n");
                        client.Flush();
                    }
                    catch
                    {
                        inactiveClients.Add(client);
                    }
                }
                
                foreach (var client in inactiveClients)
                {
                    _clients.Remove(client);
                }
            }
        }

        public static void RegisterClient(HttpResponseBase client)
        {
            lock (_lock)
            {
                _clients.Add(client);
            }
        }
    }
}

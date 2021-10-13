using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Timers;

namespace EventProducer
{
    class Program
    {
        static readonly TcpListener listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 12500);
        static readonly List<TcpClient> observers = new List<TcpClient> { };
        static readonly Timer timer = new Timer();

        static void Attach(IAsyncResult result)
        {
            TcpClient observer = listener.EndAcceptTcpClient(result);
            observers.Add(observer);
            listener.BeginAcceptTcpClient(Attach, null);
        }

        static void Notify(string message)
        {
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(message);
            for (int i = 0; i <= observers.Count - 1; i++)
            {
                TcpClient subscriber = observers[i];
                try
                {
                    NetworkStream stream = subscriber.GetStream();
                    stream.Write(buffer, 0, buffer.Length);
                }
                catch
                {
                    subscriber.Dispose();
                    observers.Remove(subscriber);
                }
            }
        }

        static void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            var message = $"The current server time is: {DateTime.Now}";
            Notify(message);
        }

        static void Main()
        {
            listener.Start();
            Console.WriteLine($"The event producer service has started at {DateTime.Now}");
            Console.WriteLine($"Listening for observers on port: { ((IPEndPoint)listener.LocalEndpoint).Port}");

            listener.BeginAcceptTcpClient(Attach, null);
            timer.Interval = 5000;
            timer.Elapsed += Timer_Elapsed;
            timer.Start();

            while (true)
            {
                Console.Write("Enter a message to be broadcasted: ");
                var broadcast = Console.ReadLine();
                if (broadcast != null)
                {
                    Notify(broadcast);
                }
                   
            }
        }
    }
}

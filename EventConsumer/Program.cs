using System;
using System.Net.Sockets;

namespace EventConsumer
{
    class Program
    {
        static void Main()
        {
            try
            {
                using(TcpClient client = new TcpClient("127.0.0.1", 12500))
                {
                    using (NetworkStream stream = client.GetStream())
                        while (true)
                        {
                            try
                            {
                                byte[] dataReceived = new byte[256];
                                Int32 bytes = stream.Read(dataReceived, 0, dataReceived.Length);
                                var responseData = System.Text.Encoding.ASCII.GetString(dataReceived, 0, bytes);
                                Console.WriteLine(responseData);
                            }
                            catch
                            {
                                break;
                            }
                        }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + "\n\nPress Enter to quit.");
                Console.ReadLine();
            }
        }
    }
}

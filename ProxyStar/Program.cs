using System;
using System.Net;
using KisaraLib;
using KisaraLib.Data;
using KisaraLib.ConsoleManager;
using KisaraLib.ProxySettings;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;

namespace ProxyStar
{
   class Program
    {
        // Made by KisaraTendo on 5/16/2019
        private static Queue<string> ProxyQueue = new Queue<string>(File.ReadAllLines("proxies.txt"));
        public static int threads;
        public enum ProxyTypes
        {
            Http,
            Socks4,
            Socks5
        }
        public static ProxyTypes type;
        public static int Working, Dead, Checked;


        public static void Menu()
        {
            Console.Clear();
            WriteConsole.AsciiText("ProxyStar", true, true, ConsoleColor.Cyan);
            WriteConsole.ColorText($"Welcome to ProxyStar V1.0.0!\n\nRead {ProxyQueue.Count.ToString()} Proxies from proxies.txt\n", ConsoleColor.Cyan);
            WriteConsole.ColorText("Menu:\n\n1) Check HTTP(s) proxies\n2) Check SOCKS4 proxies\n3) Check SOCKS5 proxies\n\nInput: ", ConsoleColor.Cyan);

            var UserInput = Console.ReadKey();
            if (UserInput.Key == ConsoleKey.D1)
            {
                type = ProxyTypes.Http;
            }
            else if (UserInput.Key == ConsoleKey.D2)
            {
                type = ProxyTypes.Socks4;
            }
            else if (UserInput.Key == ConsoleKey.D3)
            {
                type = ProxyTypes.Socks5;
            }
            else { Menu(); }

            Console.Clear();
            WriteConsole.ColorText($"Loaded: {ProxyQueue.Count.ToString()} Proxies from proxies.txt\n", ConsoleColor.Cyan);
            Console.WriteLine("Threads: ");
            threads = Convert.ToInt32(Console.ReadLine());
            CheckProxy();
        }

        static void CheckProxy()
        {
            int ProxyCount = ProxyQueue.Count;
            var uri = new Uri("https://api.ipify.org");
            Parallel.ForEach(ProxyQueue, new ParallelOptions { MaxDegreeOfParallelism = threads}, FProxy =>
            {
                try
                {
                    string[] proxy = FProxy.Split(':'); string ip = proxy[0]; int port = Convert.ToInt32(proxy[1]);
                    var HttpClientHandler = new HttpClientHandler(); HttpClientHandler.UseProxy = true;
                    switch (type)
                    {
                        case ProxyTypes.Http:
                            HttpClientHandler.Proxy = new WebProxy($"http://{proxy[0]}:{proxy[1]}");
                            break;
                        case ProxyTypes.Socks4:
                            var Socks4Proxy = new ProxyClient(ip, port, ProxyType.Socks4);
                            HttpClientHandler.Proxy = Socks4Proxy;
                            break;
                        case ProxyTypes.Socks5:
                            var Socks5Proxy = new ProxyClient(ip, port, ProxyType.Socks5);
                            HttpClientHandler.Proxy = Socks5Proxy;
                            break;
                    }
                    var client = new HttpClient(HttpClientHandler);
                    HttpResponseMessage httpResponse = client.GetAsync(uri).Result;
                    string response = httpResponse.Content.ReadAsStringAsync().Result;
                    if (response.Contains(ip))
                    {
                        Working++;
                        SaveData.WriteLinesToTxt($"Working-Proxies-{DateTime.Now.ToString("MM-dd-yyyy")}.txt", $"{ip}:{port}");
                    }
                    else
                    {
                        Console.Write(response + "\n\n");
                        Dead++;
                    }
                    Console.Title = $"ProxyStar V1.0.0 | Progress: {Checked}/{ProxyCount} | Working: {Working} | Dead Proxies: {Dead}";
                }
                catch (HttpRequestException) { Dead++; }
                catch (AggregateException) { Dead++; }
                catch (NullReferenceException) { Dead++; }
                catch (TaskCanceledException) {}

                Checked++;
            });
        }

        static void Main(string[] args)
        {
            Menu();
        }
    }
}

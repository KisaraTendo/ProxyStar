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
        private static Queue<string> ProxyQueue = new Queue<string>(File.ReadAllLines("proxies.txt"));
        public static int threads;
        public static bool http, socks4, socks5;
        public static int working, bad, Checked;
        public static void Menu()
        {
            Console.Clear();
            WriteConsole.AsciiText("ProxyStar", true, true, ConsoleColor.Cyan);
            WriteConsole.ColorText($"Welcome to ProxyStar V1.0.0!\n\nRead {ProxyQueue.Count.ToString()} Proxies from proxies.txt\n", ConsoleColor.Cyan);
            WriteConsole.ColorText("Menu:\n\n1) Check HTTP(s) proxies\n2) Check SOCKS4 proxies\n3) Check SOCKS5 proxies\n\nInput: ", ConsoleColor.Cyan);

            var UserInput = Console.ReadKey();
            if (UserInput.Key == ConsoleKey.D1)
            { 
                http = true;
            }
            else if (UserInput.Key == ConsoleKey.D2)
            {
                socks4 = true;
            }
            else if (UserInput.Key == ConsoleKey.D3)
            {
                socks5 = true;
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
                    if (http)
                    {
                        var HttpClientHandler = new HttpClientHandler { Proxy = new WebProxy($"http://{proxy[0]}:{proxy[1]}", false), UseProxy = true };
                        var client = new HttpClient(HttpClientHandler); 
                        HttpResponseMessage httpResponse = client.GetAsync(uri).Result;
                        string response = httpResponse.Content.ReadAsStringAsync().Result;
                        if (response.Contains(ip))
                        {

                            working++;
                        }
                        else
                        {
                            Console.Write(response + "\n\n");
                            bad++;
                        }

                    }
                    else if (socks4)
                    {
                        var Socks4Proxy = new ProxyClient(ip, port, ProxyType.Socks4);
                        var HttpClientHandler = new HttpClientHandler { Proxy = Socks4Proxy, UseProxy = true };
                        var client = new HttpClient(HttpClientHandler);

                        HttpResponseMessage httpResponse = client.GetAsync(uri).Result;
                        string response = httpResponse.Content.ReadAsStringAsync().Result;

                        if (httpResponse.IsSuccessStatusCode)
                        {
                            working++;
                        }
                        else
                        {
                            bad++;
                        }
                    }
                    else if (socks5)
                    {
                        var Socks5Proxy = new ProxyClient(ip, port, ProxyType.Socks5);
                        var HttpClientHandler = new HttpClientHandler { Proxy = Socks5Proxy, UseProxy = true };
                        var client = new HttpClient(HttpClientHandler);

                        HttpResponseMessage httpResponse = client.GetAsync(uri).Result;
                        string response = httpResponse.Content.ReadAsStringAsync().Result;

                        if (httpResponse.IsSuccessStatusCode)
                        {
                            working++;
                        }
                        else
                        {
                            bad++;
                        }
                    }
                    Checked++;
                    Console.Title = $"ProxyStar V1.0.0 | Progress: {Checked}/{ProxyCount} | Working: {working} | Dead Proxies: {bad}";
                }
                catch (HttpRequestException) { bad++; }
                catch (NullReferenceException) { bad++; }
                catch (TaskCanceledException ex) {}
                
                
            });
        }

        static void Main(string[] args)
        {
            
            Menu();

        }
    }
}

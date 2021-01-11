using LibGit2Sharp;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Tweetinvi.Streams;


namespace ConsoleApp4
{
    class Program
    {
        private static readonly HttpClient client = new HttpClient();
        static async Task Main(string[] args)
        {
            await ProcessRepositories();
        }

        private static async Task ProcessRepositories()
        {
            string pair1 = "EXM_USDT"; // пара для запроса EXM_USDT---BTC_USDT---EXM_BTC
            string pair2 = "BTC_USDT"; // пара для запроса
            string pair3 = "EXM_BTC"; // пара для запроса
            int limit = 1; // глубина запорса с позициях
            double StartDepo = 50;
            double comsa = 0.004;
            double lastZ = 0;
           
            try
            {
                int s = 0;
                do
                {
                    Console.Clear();
                
                var EXM_USDT = RequestAPI(pair1, limit);
                var BTC_USDT = RequestAPI(pair2, limit);
                var EXM_BTC = RequestAPI(pair3, limit);

                double EXM = StartDepo / EXM_USDT.AskPrice * (1 - comsa); // купили за депо биток
                double BTC = EXM * EXM_BTC.BidPrice  * (1 - comsa); // купили монеро за биток
                double USDT = BTC * BTC_USDT.BidPrice * (1 - comsa); //купили за монеро баксы
                string writePath = @"C:\hta.txt";
                                   

                    using (StreamWriter sw = new StreamWriter(writePath, true, System.Text.Encoding.Default))
                    {
                        if(USDT > 49)
                        sw.WriteLine(USDT);
                        
                    }
                    lastZ = USDT;

                    

                } while (s == 0);
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR - {ex.Message}");
            }
        }
        public static Pair RequestAPI(string pair, int limit)
        {
            var client = new RestClient("https://api.exmo.com/v1.1/order_book");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddParameter("pair", pair);
            request.AddParameter("limit", limit);
            IRestResponse response = client.Execute(request);

            #region Вырезаем данные
            // вырезаем данные ask и bid
            string v, v1;
            v1 = v = response.Content;
            //v1 = response.Content;
            int indexOfChar = v.IndexOf("\"ask\":[[");//номер первого вхождения
            v = v.Substring(indexOfChar + 8);
            int lastindexOfChar = v.LastIndexOf("]]}}"); //номер последнего вхождения двойных ковычек
            v = v.Substring(0, lastindexOfChar);
            v = v.Replace("]],\"bid\":[[", ",");
            string[] words = v.Split(new char[] { ',' });
            // создаем объект
            Pair req = new Pair();
            req.AskPrice = double.Parse((words[0].Replace("\"", "")).Replace(".", ","));
            req.AskQuantity = double.Parse((words[1].Replace("\"", "")).Replace(".", ","));
            req.BidPrice = double.Parse((words[3].Replace("\"", "")).Replace(".", ","));
            req.BidQuantity = double.Parse((words[4].Replace("\"", "")).Replace(".", ","));
            #endregion

            return req;
        }
    

    }
}

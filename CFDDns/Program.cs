using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CFDDns
{
    internal class Program
    {
        private static readonly string cfApiKey = Environment.GetEnvironmentVariable("CF_API_KEY");
        private static readonly string cfApiEmail = Environment.GetEnvironmentVariable("CF_API_EMAIL");
        private static readonly string cfDnsRecordId = Environment.GetEnvironmentVariable("CF_DNS_RECORD_ID");
        private static readonly string cfDnsZoneId = Environment.GetEnvironmentVariable("CF_DNS_ZONE_ID");
        private static readonly string cfDnsRecordName = Environment.GetEnvironmentVariable("CF_DNS_NAME");
        private static readonly string cfApiJsonInput = "{  \"comment\": \"Domain verification record\",  \"name\": \"CF_DNS_RECORD_VAL\",  \"proxied\": false,  \"settings\": {},  \"tags\": [],  \"ttl\": 3600,  \"content\": \"CF_DNS_RECORD_IP\",  \"type\": \"A\"}";

        private static string targetUrl = null;
        private static string oldIp = "0.0.0.0";

        static void Main(string[] args)
        {
            Console.WriteLine("Cloudflare Dynamic DNS Updater");

            int checkFrequency = 30; // In minutes.

            Console.WriteLine(string.Concat("Cloudflare API Key (CF_API_KEY): ", cfApiKey != null ? "PRESENT" : "NOT PRESENT"));
            Console.WriteLine(string.Concat("Cloudflare API Email (CF_API_EMAIL): ", cfApiEmail != null ? "PRESENT" : "NOT PRESENT"));
            Console.WriteLine(string.Concat("Cloudflare DNS Record ID (CF_DNS_RECORD_ID): ", cfDnsRecordId != null ? "PRESENT" : "NOT PRESENT"));
            Console.WriteLine(string.Concat("Cloudflare DNS Zone ID (CF_DNS_ZONE_ID): ", cfDnsZoneId != null ? "PRESENT" : "NOT PRESENT"));
            Console.WriteLine(string.Concat("Cloudflare DNS NAME (CF_DNS_NAME): ", cfDnsRecordName != null ? "PRESENT" : "NOT PRESENT"));

            if (cfApiKey != null && cfApiEmail != null && cfDnsRecordId != null && cfDnsZoneId != null && cfDnsRecordName != null)
            {
                targetUrl = string.Format("https://api.cloudflare.com/client/v4/zones/{0}/dns_records/{1}", cfDnsZoneId, cfDnsRecordId);

                while (true)
                {
                    CheckDynamicDns();

                    System.Threading.Thread.Sleep(checkFrequency * 60000);
                }
            }
            else
            {
                Console.WriteLine("Error, one or more environment variables were not set: CF_API_KEY, CF_API_EMAIL, CF_DNS_CNAME, CF_DNS_RECORD_ID, or CF_DNS_ZONE_ID. Cannot continue.");
                Console.ReadLine();
            }
        }

        private static async void CheckDynamicDns()
        {
            string currentIp = GetExternalIp();

            if (currentIp != oldIp)
            {
                Console.WriteLine(string.Format("Detected IP address change (from {0} to {1}). Updating DNS entry in Cloudflare...", oldIp, currentIp));
                oldIp = currentIp;

                HttpClient client = new HttpClient();
                HttpRequestMessage request = new HttpRequestMessage
                {
                    Method = new HttpMethod("PATCH"),
                    RequestUri = new Uri(targetUrl),
                    Headers =
                    {
                        { 
                            "X-Auth-Email", cfApiEmail
                        },
                        {
                            "Authorization", string.Concat("Bearer ", cfApiKey)
                        }
                    },
                    Content = new StringContent(cfApiJsonInput.Replace("CF_DNS_RECORD_VAL", cfDnsRecordName).Replace("CF_DNS_RECORD_IP", currentIp))
                    {
                        Headers =
                        {
                            ContentType = new MediaTypeHeaderValue("application/json")
                        }
                    }
                };
                using (HttpResponseMessage response = await client.SendAsync(request))
                {
                    string body = await response.Content.ReadAsStringAsync();

                    if (JObject.Parse(body).Value<string>("success").ToLower() == "true")
                    {
                        Console.WriteLine("Cloudflare DNS entry updated successfully.");
                    }
                    else
                    {
                        Console.WriteLine("Cloudflare DNS entry was not updated due to an error: " + body);
                    }
                }
            }
        }

        private static string GetExternalIp()
        {
            WebRequest request = WebRequest.Create("https://api.ipify.org/");

            WebResponse response = request.GetResponse();
            Stream data = response.GetResponseStream();
            string html = string.Empty;

            using (StreamReader sr = new StreamReader(data))
            {
                html = sr.ReadToEnd();
            }

            return html;
        }
    }
}

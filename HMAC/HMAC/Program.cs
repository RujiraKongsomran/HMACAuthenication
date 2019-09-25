using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Threading;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Globalization;

namespace HMAC
{
    class Program
    {
        static void Main(string[] args)
        {
            // Wait : work this function util finish
            RunAsync().Wait();
        }

        static async Task RunAsync()
        {
            var apiBaseAddress = new Uri("https://api.fieldclimate.com/v1");

            CustomDelegatingHandler customDelegatingHandler = new CustomDelegatingHandler(new HttpClientHandler());
            // Library request http
            HttpClient client = new HttpClient(customDelegatingHandler);

            client.BaseAddress = apiBaseAddress;
            // Format response
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            // GET Method
            HttpResponseMessage response = await client.GetAsync("user/stations");

            if (response.IsSuccessStatusCode)
            {
                string responseString = await response.Content.ReadAsStringAsync();

                // Process JSON here
                // Print status code and response
                Console.WriteLine("Status {0}:{1}", response.StatusCode, JsonHelper.FormatJson(responseString));
            }
            else
            {
                // Handle response codes not equal to 200 here
                Console.WriteLine("Status {0}:{1}", response.StatusCode, await response.Content.ReadAsStringAsync());
            }
        }
    }

    public class CustomDelegatingHandler : DelegatingHandler
    {
        private const string publicKey = "e69acf976fe887bbf522aefcc50db387bc4c20001455181d";
        private const string privateKey = "3006f1fbe55d0bdbf9e407db69feed3722ce799161299f93";
        static readonly CultureInfo enUsCulture = new CultureInfo("en-us"); // 2019

        public CustomDelegatingHandler(HttpClientHandler handler)
        {
            base.InnerHandler = handler;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // customize header to send HMAC
            // requestHttpMethod = GET
            string requestHttpMethod = request.Method.Method;
            // /user/stations
            string requestPath = request.RequestUri.AbsolutePath;

            // UTC 0
            DateTimeOffset date = DateTimeOffset.UtcNow;
            // put date in header
            request.Headers.Date = date;
            // set format date
            string requestTimeStamp = date.ToString("ddd, dd MMM yyyy HH:mm:ss 'GMT'", enUsCulture);

            string signatureRawData = String.Format("{0}{1}{2}{3}", requestHttpMethod, requestPath, requestTimeStamp, publicKey);
            // convert privateKey to byte
            byte[] privateKeyByteArray = Encoding.UTF8.GetBytes(privateKey);
            // convert signatureRawData to byte
            byte[] signature = Encoding.UTF8.GetBytes(signatureRawData);

            // using : use hmac within {} only
            using (HMACSHA256 hmac = new HMACSHA256(privateKeyByteArray))
            {
                byte[] signatureBytes = hmac.ComputeHash(signature);
                string requestSignatureString = ByteArrayToString(signatureBytes);
                request.Headers.Authorization = new AuthenticationHeaderValue("hmac", string.Format("{0}:{1}", publicKey, requestSignatureString));
            }

            //response = base.SendAsync(request, cancellationToken);
            //return response;

            return base.SendAsync(request, cancellationToken);
        }

        private string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }
    }

    class JsonHelper
    {
        private const string INDENT_STRING = "    ";
        public static string FormatJson(string str)
        {
            var indent = 0;
            var quoted = false;
            var sb = new StringBuilder();
            for (var i = 0; i < str.Length; i++)
            {
                var ch = str[i];
                switch (ch)
                {
                    case '{':
                    case '[':
                        sb.Append(ch);
                        if (!quoted)
                        {
                            sb.AppendLine();
                            Enumerable.Range(0, ++indent).ForEach(item => sb.Append(INDENT_STRING));
                        }
                        break;
                    case '}':
                    case ']':
                        if (!quoted)
                        {
                            sb.AppendLine();
                            Enumerable.Range(0, --indent).ForEach(item => sb.Append(INDENT_STRING));
                        }
                        sb.Append(ch);
                        break;
                    case '"':
                        sb.Append(ch);
                        bool escaped = false;
                        var index = i;
                        while (index > 0 && str[--index] == '\\')
                            escaped = !escaped;
                        if (!escaped)
                            quoted = !quoted;
                        break;
                    case ',':
                        sb.Append(ch);
                        if (!quoted)
                        {
                            sb.AppendLine();
                            Enumerable.Range(0, indent).ForEach(item => sb.Append(INDENT_STRING));
                        }
                        break;
                    case ':':
                        sb.Append(ch);
                        if (!quoted)
                            sb.Append(" ");
                        break;
                    default:
                        sb.Append(ch);
                        break;
                }
            }
            return sb.ToString();
        }
    }
    static class Extensions
    {
        public static void ForEach<T>(this IEnumerable<T> ie, Action<T> action)
        {
            foreach (var i in ie)
            {
                action(i);
            }
        }
    }
}

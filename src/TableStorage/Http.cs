﻿//<auto-generated/>
#nullable enable
using System;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace Devlooped
{
    static class Http
    {
        public static HttpClient Client { get; }

        static Http()
        {
            Client = new HttpClient();
            Client.DefaultRequestHeaders.Add("Accept", "application/json; odata=nometadata");
            Client.DefaultRequestHeaders.Add("Accept-Charset", "UTF-8");
            Client.DefaultRequestHeaders.Add("User-Agent", $"Azure-Cosmos-Table/1.0.8 ({RuntimeInformation.FrameworkDescription}; {Environment.OSVersion.Platform} {Environment.OSVersion.Version})");
            Client.DefaultRequestHeaders.Add("DataServiceVersion", "3.0;NetFx");
            Client.DefaultRequestHeaders.Add("MaxDataServiceVersion", "3.0;NetFx");
            Client.DefaultRequestHeaders.Add("x-ms-version", "2017-07-29");
        }

        public static HttpRequestMessage AddAuthorizationHeader(this HttpRequestMessage request, CloudStorageAccount account)
        {
            if (!request.Headers.TryGetValues("x-ms-date", out var values) || 
                values.FirstOrDefault() is not string date || 
                string.IsNullOrEmpty(date))
            {
                date = DateTime.UtcNow.ToString("R", CultureInfo.InvariantCulture);
                request.Headers.Add("x-ms-date", date);
            }

            var resource = request.RequestUri?.GetComponents(UriComponents.Path, UriFormat.Unescaped);
            var toSign = string.Format("{0}\n/{1}/{2}",
                    request.Headers.GetValues("x-ms-date").First(),
                    account.Credentials.AccountName,
                    resource?.TrimStart('/'));

            var hasher = new HMACSHA256(Convert.FromBase64String(account.Credentials.Key ?? ""));
            var signature = hasher.ComputeHash(Encoding.UTF8.GetBytes(toSign));
            var authentication = new AuthenticationHeaderValue("SharedKeyLite", account.Credentials.AccountName + ":" + Convert.ToBase64String(signature));

            request.Headers.Authorization = authentication;

            return request;
        }
    }
}

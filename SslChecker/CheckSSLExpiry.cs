using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

namespace SslChecker
{
    public static class CheckSSLExpiry
    {
        private static ILogger Logger { get; set; }
        private static int _daysBeforeExpiry = 10;

        [FunctionName("CheckSSLExpiry")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            Logger = log;

            string domain = req.Query["domain"];
            int daysbeforeexpiry = Convert.ToInt32(req.Query["daysbeforeexpiry"]);
            if (daysbeforeexpiry > 0)
            {
                _daysBeforeExpiry = daysbeforeexpiry;
            }

            // Create an HttpClientHandler object and set to use default credentials
            using var handler = new HttpClientHandler
            {
                // Set custom server validation callback
                ServerCertificateCustomValidationCallback = ServerCertificateCustomValidation
            };

            var takeActionOnCertificate = false;
            // Call URL in try/catch block
            try
            {
                using HttpClient client = new HttpClient(handler);
                var response = await client.GetAsync($"https://{domain}/");

                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                log.LogInformation($"Read {responseBody.Length} characters");
            }
            catch (HttpRequestException ex)
            {
                log.LogError($"Error with request: {ex.Message} ");
                takeActionOnCertificate = true;
            }

            // Return either error or OK based on validation of callback
            if (takeActionOnCertificate)
            {
                return new HttpResponseMessage(HttpStatusCode.InternalServerError) { Content = new StringContent($"Warning, SSL certificate is expired or expires within {_daysBeforeExpiry} days") };
            }

            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("No problems found") };
        }

        private static bool ServerCertificateCustomValidation(HttpRequestMessage requestMessage, X509Certificate2 certificate, X509Chain chain, SslPolicyErrors sslErrors)
        {
            // Debug information of certificate, if any
            Logger.LogDebug($"Requested URI: {requestMessage.RequestUri}");
            Logger.LogDebug($"Effective date: {certificate.GetEffectiveDateString()}");
            Logger.LogDebug($"Exp date: {certificate.GetExpirationDateString()}");
            Logger.LogDebug($"Issuer: {certificate.Issuer}");
            Logger.LogDebug($"Subject: {certificate.Subject}");

            // Based on the custom logic it is possible to decide whether the client considers certificate valid or not
            Logger.LogDebug($"Errors: {sslErrors}");

            // Return if anything is wrong with the SSL certificate, or it expires within set number of days
            return sslErrors == SslPolicyErrors.None && certificate.NotAfter > DateTime.Now.AddDays(_daysBeforeExpiry);
        }
    }
}

using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Graffle.FlowEventProcessor
{
    public class HMACDelegatingHandler : DelegatingHandler
    {
        private readonly string hmacToken;
        private readonly byte[] hmacBytes;

        public HMACDelegatingHandler(string hmac)
        {
            InnerHandler = new HttpClientHandler();
            hmacToken = hmac;

            if (!string.IsNullOrWhiteSpace(hmacToken))
            {
                hmacBytes = Convert.FromBase64String(hmacToken);
            }
        }

        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(hmacToken))
            {
                //no hmac token
                return await base.SendAsync(request, cancellationToken);
            }

            //get the company and project ids from headers set in EventWebHooks
            //using headers instead of extra method parameters because we need to override this method
            var companyId = Guid.Parse(request.Headers.GetValues("x-graffle-company-id").First());
            var projectId = Guid.Parse(request.Headers.GetValues("x-graffle-project-id").First());

            //Otherwise, start building HMAC token
            HttpResponseMessage response = null;
            string requestContentBase64String = string.Empty;
            //Get the Request URI
            string requestUri = HttpUtility.UrlEncode(request.RequestUri.AbsoluteUri.ToLower());
            //Get the Request HTTP Method type
            string requestHttpMethod = request.Method.Method;
            //Calculate UNIX time
            DateTime epochStart = new DateTime(1970, 01, 01, 0, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan timeSpan = DateTime.UtcNow - epochStart;
            string requestTimeStamp = Convert.ToUInt64(timeSpan.TotalSeconds).ToString();
            //Create the random nonce for each request
            string nonce = Guid.NewGuid().ToString("N");
            //Checking if the request contains body, usually will be null with HTTP GET and DELETE
            if (request.Content != null)
            {
                // Hashing the request body, so any change in request body will result a different hash
                byte[] content = await request.Content.ReadAsByteArrayAsync();
                using MD5 md5 = MD5.Create();
                byte[] requestContentHash = md5.ComputeHash(content);
                requestContentBase64String = Convert.ToBase64String(requestContentHash);
            }
            //Creating the raw signature string by combinging
            //companyId, request Http Method, request Uri, request TimeStamp, nonce, request Content Base64 String
            string signatureRawData = String.Format("{0}{1}{2}{3}{4}{5}", companyId, requestHttpMethod, requestUri, requestTimeStamp, nonce, requestContentBase64String);
            //Converting the HMAC token into byte array
            var secretKeyByteArray = hmacBytes;
            //Converting the signatureRawData into byte array
            byte[] signature = Encoding.UTF8.GetBytes(signatureRawData);
            //Generate the hmac signature and set it in the Authorization header
            using (HMACSHA256 hmac = new HMACSHA256(secretKeyByteArray))
            {
                byte[] signatureBytes = hmac.ComputeHash(signature);
                string requestSignatureBase64String = Convert.ToBase64String(signatureBytes);
                //Setting the values in the Authorization header using custom scheme (hmacauth)
                request.Headers.Authorization = new AuthenticationHeaderValue("hmacauth", string.Format("{0}:{1}:{2}:{3}", companyId, requestSignatureBase64String, nonce, requestTimeStamp));
            }
            response = await base.SendAsync(request, cancellationToken);
            return response;
        }
    }
}
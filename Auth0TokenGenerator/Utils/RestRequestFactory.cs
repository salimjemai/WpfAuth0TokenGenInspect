using Newtonsoft.Json;
using RestSharp;

namespace Auth0TokenGenerator.Utils
{
    internal static class RestRequestFactory
    {
        public static RestRequest CreateRestRequest(Method method, object payload, string token = "")
        {
            RestRequest request = new RestRequest
            {
                Method = method
            };

            if (payload != null)
            {
                request.AddHeader("content-type", "application/json");

                request.AddParameter("application/json", JsonConvert.SerializeObject(payload), ParameterType.RequestBody);
            }

            if (!string.IsNullOrEmpty(token))
            {
                request.AddHeader("authorization", $"Bearer {token}");
            }

            return request;
        }
    }
}

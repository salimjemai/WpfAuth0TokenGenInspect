using System.Text;
using RestSharp;

namespace Auth0TokenGenerator.Utils
{
    internal static class RestClientFactory
    {
        public static RestClient CreateRestClient(string domain, string endPoint)
        {
            if (string.IsNullOrWhiteSpace(domain) || string.IsNullOrWhiteSpace(endPoint))
            {
                return null;
            }
            StringBuilder sb = new StringBuilder();
            if (domain.StartsWith("https"))
            {
                sb.Append(domain);
            }
            else
            {
                sb.Append(@"https://" + domain);
            }

            if (endPoint.StartsWith(@"/"))
            {
                sb.Append(endPoint);
            }
            else
            {
                sb.Append(@"/" + endPoint);
            }
            return new RestClient(sb.ToString());
        }
    }
}

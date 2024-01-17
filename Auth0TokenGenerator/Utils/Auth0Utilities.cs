using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading;
using System.Web;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using RestSharp;

namespace Auth0TokenGenerator.Utils
{
    public class Auth0Utilities
    {
        private string _apiClientId = string.Empty;
        private string _apiClientSecret = string.Empty;
        private string _managementApiUrl = string.Empty;

        public Auth0Utilities()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        }

        public bool IsAuth0TokenValid(string token)
        {
            bool isValid = false;

            try
            {
               isValid = GetUserIdFromToken(token)!=null;
            }
            catch (Exception e)
            {
                throw new Exception($"An error occurred while validating the token: {e.Message}");
            }
            return isValid;
        }

        public bool IsExpired(JwtSecurityToken token, DateTime utcNow)
        {
            return utcNow > token.ValidTo;
        }

        /// <summary>
        /// Gets an access token to make Auth0 API calls
        /// </summary>
        /// <param name="domain"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public Auth0TokenResponse GetAuth0ApiToken(string domain)
        {
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
            RestSharp.RestClient client = new RestSharp.RestClient($"{domain}oauth/token");
            RestRequest request = new RestRequest { Method = Method.Post };
            request.AddHeader("content-type", "application/json");
            Auth0TokenRequest tokenRequest = new Auth0TokenRequest()
            {
                client_id = _apiClientId,
                client_secret = _apiClientSecret,
                audience = _managementApiUrl,
                grant_type = "client_credentials"
            };

            request.AddParameter("application/json", JsonConvert.SerializeObject(tokenRequest), ParameterType.RequestBody);
            RestResponse response = client.Execute(request);
            if (!response.StatusDescription.Equals("OK"))
            {
                throw new Exception($"Current application do not have access to this api {_managementApiUrl}");
            }
            string tokenResponseContent = response.Content;
            return JsonConvert.DeserializeObject<Auth0TokenResponse>(tokenResponseContent);

        }

        public JwtSecurityToken DeserializeAuth0AccessToken(string accessToken)
        {
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            return handler.ReadJwtToken(accessToken);
        }

        /// <summary>
        /// Get the Auth0 user information
        /// </summary>
        /// <param name="userId">Auth0 user ID</param>
        /// <param name="domain">Auth0 domain</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public Auth0UserData GetAuth0UserInfo(string userId, string domain)
        {
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
            var auth0ApiToken = GetAuth0ApiToken(domain).access_token;
            if(auth0ApiToken == null)
                throw new Exception($"Failed to generate a LoginAPI token to request user data");
            Auth0UserData user = new Auth0UserData();
            RestSharp.RestClient apiclient = new RestSharp.RestClient($"{domain}api/v2/users/{userId}");
            RestSharp.RestRequest apirequest = new RestSharp.RestRequest
            {
                Method = Method.Get
            };
            apirequest.AddHeader("authorization", $"Bearer {auth0ApiToken}");
            RestResponse apiresponse = apiclient.Execute(apirequest);

            if (apiresponse.Content.Contains("Unauthorized") || !apiresponse.IsSuccessful)
            {
                throw new Exception($"The application is not authorized to access the management api");
            }

            if (apiresponse.Content != null)
            {
                return JsonConvert.DeserializeObject<Auth0UserData>(apiresponse.Content);
            }

            return null;
        }

        /// <summary>
        /// Gets a user email email address fromt eh token
        /// </summary>
        /// <param name="accessToken">Access token</param>
        /// <param name="apiClientId">Auth0 API client ID</param>
        /// <param name="apiClientSecret">Auth0 API client secret</param>
        /// <param name="managementApiUrl">Subscription manager</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public string GetAuth0EmailAddress(string accessToken, string apiClientId, string apiClientSecret, string managementApiUrl)
        {
            string email = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(accessToken)) 
                    throw new Exception($"LoginApp Token must be provided to extract the user email address.");
                
                _apiClientId = apiClientId;

                _apiClientSecret = !string.IsNullOrEmpty(apiClientSecret)?
                    UserFunctions.DecodeString(apiClientSecret):
                                    throw new Exception($"The API client secret cannot be empty.");

                // decode the token
                var deserializedToken = DeserializeAuth0AccessToken(accessToken);
                if (deserializedToken == null) return null;
                var payloads = deserializedToken.Payload.ToList();
                var tokenClaims = deserializedToken.Claims.ToList();
                if (tokenClaims.Count() == 0) return null;
                var _tenantDomain = payloads.Count() > 0 ? payloads.First(a => a.Key == "iss").Value : null;
                var _userId = tokenClaims.First(a => a.Type == "sub")?.Value;
                if (!string.IsNullOrEmpty(_userId) && !_userId.StartsWith("auth")) 
                    return null;
                _managementApiUrl = !string.IsNullOrEmpty(managementApiUrl) ? managementApiUrl : _tenantDomain + "api/v2/";

                // Call Auth0 to get the user info for a particular user
                var userInfo = GetAuth0UserInfo(_userId, (string)_tenantDomain);
                email = userInfo?.email;

            }
            catch (Exception e)
            {
                throw new Exception($"An error occurred while extracting the LoginApp " +
                                    $"email address - Auth0Utilies.cs, exception detail: {e.Message}");
            }

            return email;
        }

        /// <summary>
        /// Gets an Auth0 user ID for a user
        /// </summary>
        /// <param name="email">Users email</param>
        /// <param name="token">Access token</param>
        /// <param name="domain">Auth0 domain</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public string GetUserId(string email, string token, string domain)
        {
            string clientemail = HttpUtility.UrlEncode($":\"{email}\"");
            RestSharp.RestClient apiclient = new RestSharp.RestClient($"https://{domain}/api/v2/users?q=email{clientemail}&search_engine=v3");
            RestRequest apirequest = new RestRequest
            {
                Method = Method.Get
            };
            apirequest.AddHeader("authorization", $"Bearer {token}");
            RestResponse apiresponse = apiclient.Execute(apirequest);
            if (!apiresponse.StatusDescription.Equals("OK"))
            {
                throw new Exception($"Unable to get the user ID for {email}");
            }
            Auth0User user = JsonConvert.DeserializeObject<Auth0User[]>(apiresponse.Content).FirstOrDefault();
            return user?.user_id;
        }

        /// <summary>
        /// Get teh Auth0 user information
        /// </summary>
        /// <param name="email">Users email</param>
        /// <param name="connection">Auth0 connection string</param>
        /// <param name="domain">Auth0 domain</param>
        /// <param name="token">Access token</param>
        /// <param name="erpClientId">Auth0 application ID</param>
        /// <returns></returns>
        public string CreateAuth0User(string email, string connection, string domain, string token, string erpClientId)
        {
            Auth0NewUser u = new Auth0NewUser
            {
                email = email,
                password = "Auth0Password!",
                connection = connection
            };

            RestSharp.RestClient userclient = new RestSharp.RestClient($"https://{domain}/api/v2/users");
            RestRequest userrequest = new RestRequest
            {
                Method = Method.Post
            };
            userrequest.AddHeader("authorization", $"Bearer {token}");
            userrequest.AddParameter("application/json", JsonConvert.SerializeObject(u), ParameterType.RequestBody);
            RestResponse userresponse = userclient.Execute(userrequest);
            if (userresponse.StatusDescription != null && !userresponse.StatusDescription.Equals("Created"))
            {
                return $"There was an error creating a user for {email}";
            }
            Auth0UserData usertokenresponse = JsonConvert.DeserializeObject<Auth0UserData>(userresponse.Content);
            SendChangePassword(domain, connection, email, erpClientId);
            return usertokenresponse?.user_id;
        }

        /// <summary>
        /// Sends an Auth0 change password request
        /// </summary>
        /// <param name="domain">Auth0 domain</param>
        /// <param name="connection">Auth0 connection</param>
        /// <param name="email">The users email</param>
        /// <param name="erpClientId">The Auth0 application ID</param>
        /// <exception cref="Exception"></exception>
        public void SendChangePassword(string domain, string connection, string email, string erpClientId)
        {
            RestSharp.RestClient client = new RestSharp.RestClient($"https://{domain}/dbconnections/change_password");
            RestRequest request = new RestRequest
            {
                Method = Method.Post
            };
            request.AddHeader("content-type", "application/json");
            PasswordReset reset = new PasswordReset
            {
                client_id = erpClientId, 
                email = email,
                connection = connection
            };
            request.AddParameter("application/json", JsonConvert.SerializeObject(reset), ParameterType.RequestBody);
            RestResponse response = client.Execute(request);
            if (!response.StatusDescription.Equals("OK"))
            {
                throw new Exception($"Unable to send change password request to {email}");
            }
        }

        /// <summary>
        /// Gets an Auth0 API token
        /// </summary>
        /// <param name="domain">Auth0 domain</param>
        /// <param name="clientId">API Client ID</param>
        /// <param name="clientSecret">API Client secret</param>
        /// <returns>The token</returns>
        /// <exception cref="Exception"></exception>
        public string GetApiToken(string domain, string clientId, string clientSecret)
        {
            string apiResponse = null;
            try
            {
                RestSharp.RestClient client = new RestSharp.RestClient($"https://{domain}/oauth/token");
                RestRequest request = new RestRequest
                {
                    Method = Method.Post
                };
                request.AddHeader("content-type", "application/json");
                if (!string.IsNullOrWhiteSpace(clientSecret))
                {
                    clientSecret = UserFunctions.DecodeString(clientSecret);
                }
                else
                {
                    apiResponse = string.Empty;
                }
                Auth0TokenRequest tokenRequest = new Auth0TokenRequest()
                {
                    client_id = clientId,
                    client_secret = clientSecret,
                    audience = $"https://{domain}/api/v2/",
                    grant_type = "client_credentials"
                };
                request.AddParameter("application/json", JsonConvert.SerializeObject(tokenRequest),
                    ParameterType.RequestBody);

                RestResponse response = client.Execute(request);
                if (response.StatusDescription != null && !response.StatusDescription.Equals("OK"))
                {
                    throw new Exception($"Unable to get the API access token");
                }
                string tokenResponseContent = response.Content;
                if (response.IsSuccessful)
                {
                    Auth0TokenResponse tokenResponse =
                        JsonConvert.DeserializeObject<Auth0TokenResponse>(tokenResponseContent);
                    apiResponse =  tokenResponse.access_token;
                }
                else
                {
                    return apiResponse;
                }
            }
            catch(Exception ex)
            {
                throw new Exception($"An exception occurred while getting Api Token, see exception details here: {ex}");
            }
            return apiResponse;
        }

        public OrgData GetOrgData(string domain, string organization, string token, string clientId, string clientSecret)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                token = GetApiToken(domain, clientId, clientSecret);
            }
            RestSharp.RestClient client = new RestSharp.RestClient($"https://{domain}/api/v2/organizations/{organization}");
            RestRequest request = RestRequestFactory.CreateRestRequest(Method.Get, null, token);
            RestResponse response = client.Execute(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                OrgData org = JsonConvert.DeserializeObject<OrgData>(response.Content);
                return org;
            }

            return null;
        }

        public void UpdateEnableMfa(string domain, string organization, string clientId, string clientSecret, string value)
        {
            string token = GetApiToken(domain, clientId, clientSecret);
            OrgData oldOrgData = GetOrgData(domain, organization, token, clientId, clientSecret);
            if (oldOrgData == null)
            {
                return;
            }

            UpdateOrgData newOrgData = CopyOrgData(oldOrgData);
            newOrgData.metadata.enableMFA = value;
            RestSharp.RestClient client = new RestSharp.RestClient($"https://{domain}/api/v2/organizations/{organization}");
            RestRequest request = RestRequestFactory.CreateRestRequest(Method.Patch, newOrgData, token);
            RestResponse response = client.Execute(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                // left in as a debugging device. If you check the org object you can tell if the org was updated or not.
                OrgData org = JsonConvert.DeserializeObject<OrgData>(response.Content);
            }
        }

        public void UpdateMfaProvider(string domain, string organization, string clientId, string clientSecret, string value)
        {
            string token = GetApiToken(domain, clientId, clientSecret);
            OrgData oldOrgData = GetOrgData(domain, organization, token, clientId, clientSecret);
            if (oldOrgData == null)
            {
                return;
            }

            UpdateOrgData newOrgData = CopyOrgData(oldOrgData);
            newOrgData.metadata.mfaProvider = value;
            RestSharp.RestClient client = new RestSharp.RestClient($"https://{domain}/api/v2/organizations/{organization}");
            RestRequest request = RestRequestFactory.CreateRestRequest(Method.Patch, newOrgData, token);
            RestResponse response = client.Execute(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                // left in as a debugging device. If you check the org object you can tell if the org was updated or not.
                OrgData org = JsonConvert.DeserializeObject<OrgData>(response.Content);
            }
        }

        public void UpdateRememberBrowser(string domain, string organization, string clientId, string clientSecret, string value)
        {
            string token = GetApiToken(domain, clientId, clientSecret);
            OrgData oldOrgData = GetOrgData(domain, organization, token, clientId, clientSecret);
            if (oldOrgData == null)
            {
                return;
            }

            UpdateOrgData newOrgData = CopyOrgData(oldOrgData);
            newOrgData.metadata.rememberBrowser = value;
            RestSharp.RestClient client = new RestSharp.RestClient($"https://{domain}/api/v2/organizations/{organization}");
            RestRequest request = RestRequestFactory.CreateRestRequest(Method.Patch, newOrgData, token);
            RestResponse response = client.Execute(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                // left in as a debugging device. If you check the org object you can tell if the org was updated or not.
                OrgData org = JsonConvert.DeserializeObject<OrgData>(response.Content);
            }
        }

        //Validates the token and returns the auth0 userId
        public string GetUserIdFromToken(string token )
        {
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
            var deserializedToken = DeserializeAuth0AccessToken(token);
            if (deserializedToken == null) return null;
            var payloads = deserializedToken.Payload.ToList();
            var tokenClaims = deserializedToken.Claims.ToList();
            if (tokenClaims.Count() == 0) return null;
            var _tenantDomain = payloads.Count() > 0 ? payloads.First(a => a.Key == "iss").Value : null;
            var _audience = tokenClaims.First(a => a.Type == "aud")?.Value;

            IConfigurationManager<OpenIdConnectConfiguration> configurationManager =
                new ConfigurationManager<OpenIdConnectConfiguration>($"{_tenantDomain}.well-known/openid-configuration",
                new OpenIdConnectConfigurationRetriever());
            var asyncHelp = new AsyncHelper();
            OpenIdConnectConfiguration openIdConfig = asyncHelp.RunSync(async () => await configurationManager.GetConfigurationAsync(CancellationToken.None));

            TokenValidationParameters validationParameters = new TokenValidationParameters
            {
                ValidateLifetime = true,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = (string)_tenantDomain,
                ValidAudiences = new[] { _audience },
                IssuerSigningKeys = openIdConfig.SigningKeys
            };
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            var isTokenExpired = IsExpired(deserializedToken, DateTime.UtcNow);
            if (!isTokenExpired)
            {
                ClaimsPrincipal user = handler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                return  user?.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            }
            return null;
        }

        private UpdateOrgData CopyOrgData(OrgData orgdata)
        {
            UpdateOrgData newOrgdata = new UpdateOrgData
            {
                // No ID tag! You will get an error if you try
                metadata = new OrgMetadata(),
                display_name = orgdata.display_name,
                name = orgdata.name
            };

            // The way the metadata works is that if you try and send a null value
            // you will get a "bad request" error when you send it. So, we give it
            // a default value of 0. If the metadata variable does not exist, it is created.
            // Found out the hard way that metadata values MUST BE STRINGS
            newOrgdata.metadata.tenantCode = orgdata.metadata.tenantCode;
            newOrgdata.metadata.tenantId = orgdata.metadata.tenantId;
            newOrgdata.metadata.tenantName = orgdata.metadata.tenantName;
            newOrgdata.metadata.tenantType = orgdata.metadata.tenantType;
            newOrgdata.metadata.enableMFA = orgdata.metadata.enableMFA ?? "0";
            newOrgdata.metadata.mfaProvider = orgdata.metadata.mfaProvider ?? "0";
            newOrgdata.metadata.rememberBrowser = orgdata.metadata.rememberBrowser ?? "0";

            return newOrgdata;
        }


    }
}

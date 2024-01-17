using System;
using System.Collections.Generic;
using System.Windows;
using System.Net;
using Auth0TokenGenerator.Auth0TokenManagement;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Collections.ObjectModel;
using Auth0.OidcClient;
using Auth0TokenGenerator.Pages;
using RestSharp;
using System.Windows.Navigation;
using Auth0TokenGenerator.Database;
using Newtonsoft.Json.Linq;
using IdentityModel.Client;
using System.Threading;
using IdentityModel.OidcClient;
using System.Linq;
using System.Windows.Media;
using System.Runtime.InteropServices;

namespace Auth0TokenGenerator
{
    public partial class MainWindow : Window
    {
        // This is the order that the command line args are in
        // 0 is always the executable in .NET
        public CollectionView orgList { get; set; }
        public string DefaultOrg { get; set; } = "internal-eis-org";
        private string _domain;

        public string accessToken { get; set; }
        public string idToken { get; set; }
        public int counter { get; set; } = 0;
        public MainWindow()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12; // you MUST have this
            InitializeComponent();
            OrganizationsList();
        }

        private void OrganizationsList()
        {
            var organizationList = new ObservableCollection<string>()
            {
                "internal-eis-org", "8410_corridor_eis-qa_mainline_ne", "8443_corridor_qa_mainline_ent2"
            };

            orgList = new CollectionView(organizationList);
        }

        public async void GenerateToken_OnClick(object sender, RoutedEventArgs e)
        {
            // , RedirectUri = "https://eis-development.us.auth0.com/mobile" 
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            string accessToken;
            string idToken;
            try
            {
                if (counter == 0)
                {
                    var authCred = new Auth0Creds()
                    {
                        Domain = domain.Text,
                        Client = clientId.Text,
                        Email = email.Text,
                        Org = orgId.Text
                    };

                    var loginParameters = new Dictionary<string, string>() { { "login_hint", email.Text }, { "organization", orgId.Text } };
                    var clientOptions = new Auth0ClientOptions { Domain = domain.Text, ClientId = clientId.Text };
                    var client = new Auth0Client(clientOptions);
                    var loginResult = await client.LoginAsync(loginParameters);
                    clientOptions.PostLogoutRedirectUri = clientOptions.RedirectUri;
                    if (string.IsNullOrEmpty(loginResult.Error))
                    {
                        counter++;
                        Authentication_aquired.Text += " " + counter + "\nThis application calls Auth0 once per runtime.";
                    }
                    else
                        throw new Exception(loginResult.Error);
                        //MessageBox.Show($"You've cancelled the operation!", "Auth0 Token", MessageBoxButton.OK, MessageBoxImage.Error);

                    MessageBox.Show($"Successfully Authorized!!!\nLogged in user: {loginResult.User.FindFirst(c => c.Type == "name")?.Value}, " +
                                    $"email: {loginResult.User.FindFirst(c => c.Type == "email")?.Value}",
                                    "Auth0 Token",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);

                    accessToken = loginResult.AccessToken;
                    idToken = loginResult.IdentityToken;
                    DatabaseHelper.WriteDataIntoSession(idToken, accessToken, DateTime.UtcNow, email.Text);
                }
                else
                {
                    Authentication_aquired.Text = "Getting the recently generated tokens from the local Database.";
                    Authentication_aquired.Foreground = Brushes.Red;
                    accessToken = GetTokens().Last();
                    idToken = GetTokens().First();
                }


                if (!string.IsNullOrEmpty(accessToken) && !string.IsNullOrEmpty(idToken))
                {
                    var auth0Mg = new Auth0Management(new Auth0User(), accessToken, idToken);
                    NavAuthenticationBrowser.Navigate(auth0Mg);
                }
                else
                {
                    throw new Exception("Unable to access local database to retrieve access/id token..");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while generating token, exceptiong details: {ex.Message}", "Token Gen", MessageBoxButton.OK, MessageBoxImage.Stop);
            }
        }




        public List<string> GetTokens()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            try
            {
                var idToken = string.Empty;
                var accessToken = string.Empty;
                string sqlQuery = "select idToken, accessToken from Sessions order by DateCreated desc limit 1";
                using (var selectSession = DatabaseHelper.SelectRecords(sqlQuery))
                {

                    while (selectSession.Read())
                    {
                        idToken = selectSession.GetString(0);
                        accessToken = selectSession.GetString(1);
                    }

                    if (!string.IsNullOrEmpty(idToken) && !string.IsNullOrEmpty(accessToken))
                    {
                        return new List<string> { idToken, accessToken };
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to Get tokens from the Database. see exception: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return null;
        }

        private async Task GetAuth0Login(Auth0Creds creds)
        {

            Uri url = new Uri("/MainWindow.xaml");
        }

        public void GetAllOrgs(string domain)
        {
            var client = new RestClient($"https://{domain}/api/v2/organizations");
            var request = new RestRequest { Method = Method.Get };
            request.AddHeader("authorization", "Bearer MGMT_API_ACCESS_TOKEN");
            var response = client.Execute(request);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            bool purged = false;
            try
            {
                var dialogueResult = MessageBox.Show("Do you really want to clear all the stored tokens ?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (dialogueResult == MessageBoxResult.Yes)
                {
                    purged = DatabaseHelper.PurgeTable();
                    if (purged)
                    {
                        MessageBox.Show("Successfully purged all existing session tokens.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                        throw new Exception("Failed to purge the database");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to purge Database. see exception: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void NavAuthenticationBrowser_Navigated(object sender, NavigationEventArgs e)
        {

        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            var dialogueResult = MessageBox.Show("Do you really want to exit the application ?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (dialogueResult == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }

        private void GetPeviousToken_Click(object sender, RoutedEventArgs e)
        {
            var accessToken = GetTokens()?.Last();
            var idToken = GetTokens()?.First();


            if (!string.IsNullOrEmpty(accessToken) && !string.IsNullOrEmpty(idToken))
            {
                var auth0Mg = new Auth0Management(new Auth0User(), accessToken, idToken);
                NavAuthenticationBrowser.Navigate(auth0Mg);
            }
            else
            {
                MessageBox.Show("Unable to access local database to retrieve access/id token..", "Token Gen", MessageBoxButton.OK, MessageBoxImage.Stop);
            }

        }
    }
}
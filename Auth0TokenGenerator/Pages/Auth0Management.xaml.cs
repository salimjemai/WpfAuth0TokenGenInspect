using Auth0TokenGenerator.Auth0TokenManagement;
using Auth0TokenGenerator.Utils;
using System;
using System.ComponentModel;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace Auth0TokenGenerator.Pages
{
    /// <summary>
    /// Interaction logic for Auth0Management.xaml
    /// </summary>
    public partial class Auth0Management : Page
    {
        private Auth0TokenManagement.Auth0User auth0User;
        private string accessToken1;
        private string identityToken;

        public Auth0Management(Auth0TokenManagement.Auth0User user, string accessToken, string idToken)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            InitializeComponent();
            PopulateUserInfo(user, accessToken, idToken);
            HeaderTitle.Text = "Auth0 User Token info";
        }

        //private NavigationCommands navigation { get; set; }
        public void PopulateUserInfo(Auth0TokenManagement.Auth0User user, string acToken, string idTkn)
        {
            if (user != null)
            {
                accessToken.Text = acToken;
                idToken.Text = idTkn;

            }
        }

        public void OnWindowClosing(object sender, CancelEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void CopyAccessTokenToClipboard(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(this.accessToken.Text);
        }
        private void CopyIdTokenToClipboard(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(this.idToken.Text);
        }

        private void InspectAccessToken(object sender, RoutedEventArgs e)
        {
            var auth0Util = new Auth0Utilities();
            var accessTokenDeser = auth0Util.DeserializeAuth0AccessToken(this.accessToken.Text);
            HeaderTitle.Text = "Token attributes";
            GetInspectionData.Navigate(new ObjectInTreeView() { ObjectToVisualize = accessTokenDeser });

        }

        private void InspectIdToken(object sender, RoutedEventArgs e)
        {
            var auth0Util = new Auth0Utilities();
            var idTokenDeser = auth0Util.DeserializeAuth0AccessToken(this.idToken.Text);
            GetInspectionData.Navigate(new ObjectInTreeView() { ObjectToVisualize = idTokenDeser });
        }

        private void NavigateBackHome(object sender, RoutedEventArgs e)
        {
            var clickedButton = e.OriginalSource as NavButton;
            NavigationService.Navigate(clickedButton.NavUri);
        }

        private void Back_Home(object sender, RoutedEventArgs e)
        {        
            this.Content = null;
        }
    }
}

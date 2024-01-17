using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace Auth0TokenGenerator.Pages
{
    /// <summary>
    /// Interaction logic for InspectToken.xaml
    /// </summary>
    public partial class InspectToken : Page
    {
        public InspectToken(JwtSecurityToken token)
        {
            InitializeComponent();
            try
            {
                var convertToJson = JsonConvert.SerializeObject(token);
                var obj = JObject.Parse(convertToJson);
                var array = JsonArray.Parse(convertToJson);
                var jArray = JArray.Parse(convertToJson);
                tView.Items.Add(Json2Tree(JArray.Parse(convertToJson), "Root"));
            }
            catch (JsonReaderException jre)
            {
                MessageBox.Show($"Invalid Json {jre.Message}");
            }
        }

        public static TreeViewItem Json2Tree(JToken root, string rootName = "")
        {
            var parent = new TreeViewItem() { Header = rootName };

            foreach (var obj in root)
            {
                foreach (KeyValuePair<string, JToken> token in (JObject)obj)
                    switch (token.Value.Type)
                    {
                        case JTokenType.Array:
                            var jArray = token.Value as JArray;

                            if (jArray?.Any() ?? false)
                                parent.Items.Add(Json2Tree(token.Value as JArray, token.Key));
                            else
                                parent.Items.Add($"\x22{token.Key}\x22 : [ ]"); // Empty array   
                            break;

                        case JTokenType.Object:
                            parent.Items.Add(Json2Tree((JObject)token.Value, token.Key));
                            break;

                        default:
                            parent.Items.Add(GetChild(token));
                            break;
                    }

            }
            return parent;
        }

        private static TreeViewItem GetChild(KeyValuePair<string, JToken> token)
        {
            var value = token.Value.ToString();
            var outputValue = string.IsNullOrEmpty(value) ? "null" : value;
            return new TreeViewItem() { Header = $" \x22{token.Key}\x22 : \x22{outputValue}\x22" };
        }

        private void GoBack(object sender, RoutedEventArgs e)
        {
            //NavigationCommands nav = new 
            //this.Frame.CanGoBack();
        }

        //public static bool TryGoBack()
        //{
        //    Frame rootFrame = this as Frame;
        //    if (rootFrame.CanGoBack)
        //    {
        //        rootFrame.GoBack();
        //        return true;
        //    }
        //    return false;
        //}
        private void NavigateBack(object sender, RoutedEventArgs e)
        {
            var clicked = e.OriginalSource as NavButton;
            NavigationService.Navigate(clicked.NavUri);
        }
    }
}

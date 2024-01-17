using System;

namespace Auth0TokenGenerator.Auth0TokenManagement
{
    public class Auth0User
    {
        public string sub { get; set; }
        public string nickname { get; set; }
        public string name { get; set; }
        public string picture { get; set; }
        public DateTime updated_at { get; set; }
        public bool new_user { get; set; }
        public string org_id { get; set; }
    }
}


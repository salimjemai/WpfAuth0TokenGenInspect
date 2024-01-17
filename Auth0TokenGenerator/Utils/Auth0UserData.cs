using System;

namespace Auth0TokenGenerator.Utils
{
    //public class Auth0UserData
    //{
    //    public DateTime created_at { get; set; }
    //    public string email { get; set; }
    //    public bool email_verified { get; set; }
    //    public Identity[] identities { get; set; }
    //    public string name { get; set; }
    //    public string nickname { get; set; }
    //    public string picture { get; set; }
    //    public DateTime updated_at { get; set; }
    //    public string user_id { get; set; }
    //    public string username { get; set; }
    //    public User_Metadata user_metadata { get; set; }
    //    public DateTime last_login { get; set; }
    //    public string last_ip { get; set; }
    //    public int logins_count { get; set; }
    //    public App_Metadata app_metadata { get; set; }
    //}

    public class User_Metadata
    {
    }

    public class App_Metadata
    {
        public string UserName { get; set; }
    }

    //public class Identity
    //{
    //    public string connection { get; set; }
    //    public string provider { get; set; }
    //    public string user_id { get; set; }
    //    public bool isSocial { get; set; }
    //}
}

using System;
using System.Collections.Generic;

namespace Auth0TokenGenerator.Utils
{
    public class Auth0TokenRequest
    {
        public string client_id { get; set; }
        public string client_secret { get; set; }
        public string audience { get; set; }
        public string grant_type { get; set; }
        public string scope { get; set; }
    }

    public class Auth0TokenResponse
    {
        public string access_token { get; set; }
        public string scope { get; set; }
        public long expires_in { get; set; }
        public string token_type { get; set; } }

    public class Identity
    {
        public string connection { get; set; }
        public string provider { get; set; }
        public string user_id { get; set; }
        public bool isSocial { get; set; }
    }

    public class Auth0User
    {
        public DateTime created_at { get; set; }
        public string email { get; set; }
        public bool email_verified { get; set; }
        public List<Identity> identities { get; set; }
        public string name { get; set; }
        public string nickname { get; set; }
        public string picture { get; set; }
        public DateTime updated_at { get; set; }
        public string user_id { get; set; }
        public DateTime multifactor_last_modified { get; set; }
        public DateTime last_password_reset { get; set; }
        public string last_ip { get; set; }
        public DateTime last_login { get; set; }
        public int logins_count { get; set; }
    }

    public class OrganizationMember
    {
        public List<string> members { get; set; }
    }

    public class Auth0NewUser
    {
        public string email { get; set; }
        public string password { get; set; }
        public string connection { get; set; }
    }

    public class AppMetadata
    {
    }

    public class Auth0UserData
    {
        public string email { get; set; }
        public string phone_number { get; set; }
        public UserMetadata user_metadata { get; set; }
        public bool blocked { get; set; }
        public bool email_verified { get; set; }
        public bool phone_verified { get; set; }
        public AppMetadata app_metadata { get; set; }
        public string given_name { get; set; }
        public string family_name { get; set; }
        public string name { get; set; }
        public string nickname { get; set; }
        public string picture { get; set; }
        public string user_id { get; set; }
        public string connection { get; set; }
        public string password { get; set; }
        public bool verify_email { get; set; }
        public string username { get; set; }
    }

    public class UserMetadata
    {
    }

    public class PasswordReset
    {
        public string email { get; set; }
        public string connection { get; set; }
        public string client_id { get; set; }
    }

    public class OrgMetadata : ICloneable
    {
        public string tenantCode { get; set; }
        public string tenantId { get; set; }
        public string tenantName { get; set; }
        public string tenantType { get; set; }
        public string enableMFA { get; set; }
        public string mfaProvider { get; set; }
        public string rememberBrowser { get; set; }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }

    public class OrgData : ICloneable
    {
        public string id { get; set; }
        public string name { get; set; }
        public string display_name { get; set; }
        public OrgMetadata metadata { get; set; }
        public object Clone()
        {
            OrgData orgdata = (OrgData)MemberwiseClone();
            return orgdata;
        }
    }

    public class UpdateOrgData
    {
        // note that including ID in this class will cause the update to fail
        public string name { get; set; }
        public string display_name { get; set; }
        public OrgMetadata metadata { get; set; }
    }
}

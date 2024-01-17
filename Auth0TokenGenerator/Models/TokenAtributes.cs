using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth0TokenGenerator.Models
{
    public class TokenAtributes
    {
        public string Actor { get; set; }
        public List<string> Audience { get; set; }
        public List<Claims> Claims { get; set; }
        public string EncodedHeader { get; set; }
        public string EncodedPayload { get; set; }
        public string Id { get; set; }
        public string Issuer { get; set; }
        public Payload Payload { get; set; }
        public string InnerToken { get; set; }
        public string RawAuthenticationTag { get; set; }
        public string RawCiphertext { get; set; }
        public string RawData { get; set; }
        public string RawEncryptedKey { get; set; }
        public string RawInitializationVector { get; set; }
        public string RawHeader { get; set; }
        public string RawPayload { get; set; }
        public string RawSignature { get; set; }
        public string SecurityKey { get; set; }
        public string SignatureAlgorithm { get; set; }
        public string SigningCredentials { get; set; }
        public string EncryptingCredentials { get; set; }
        public string SigningKey { get; set; }
        public string Subject { get; set; }
        public string ValidFrom { get; set; }
        public string ValidTo { get; set; }
        public string IssuedAt { get; set; }

    }

    public class Claims
    {
        public string  Issuer { get; set; }

        public string OriginalIssuer { get; set; }

        public object Properties { get; set; }

        public string Subject { get; set; }

        public string Type { get; set; }

        public bool Value { get; set; } = false;

        public string  ValueType { get; set; }
    }

    public class Header
    {
        public string Algorithm { get; set; }
        public string Type { get; set; }
        public string Kid { get; set; }
    }

    public class Payload
    {
        public bool NewUser { get; set; } = false;
        public string OrgDisplayName { get; set; }

        public string given_name { get; set; }

        public string family_name { get; set; }

        public string nickname { get; set; }

        public string name { get; set; }

        public string picture { get; set; }

        public string updated_at { get; set; }

        public string email { get; set; }
        public bool email_verified { get; set; } = false;
        public string iss { get; set; }
        public string aud { get; set; }
        public string iat { get; set; }
        public string exp { get; set; }
        public string sub { get; set; }
        public string acr { get; set; }
        public List<amr> amr { get; set; }
        public string sid { get; set; }
        public string nonce { get; set; }
        public string org_id { get; set; }

    }

    public class amr
    {
        public object listOfAmr { get; set; }
    }
}

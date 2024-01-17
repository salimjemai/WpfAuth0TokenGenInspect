using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth0TokenGenerator.Auth0TokenManagement
{
    public class Auth0Creds
    {
        public string Domain
        {
            get;
            set;
        }
        public string Client
        {
            get;
            set;
        }
        public  string Email
        {
            get;
            set;
        }
        public  string Org
        {
            get;
            set;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth0TokenGenerator
{
    internal class Program
    {
        [STAThread]
        static void Main()
        {
            var app = new App();
            app.InitializeComponent();
            Database.DatabaseHelper.InitializeDatabase();
            app.Run();
        }
    }
}

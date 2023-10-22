using Microsoft.AspNetCore.Hosting;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using VizRecord;

namespace VizRecord
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            RunWebHostAsync();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }

        static async Task RunWebHostAsync()
        {
            await Task.Run(() =>
            {
                new WebHostBuilder()
                    .UseKestrel()
                    .UseStartup<Startup>()
                    .Build()
                    .Run();
            });
        }
    }
}




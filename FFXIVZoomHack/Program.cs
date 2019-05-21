using System;
using System.Net;
using System.Windows.Interop;
using System.Windows.Media;
using FFXIVZoomHack.WPF;

namespace FFXIVZoomHack
{
    public static class Program
    {
        [STAThread]
        private static void Main()
        {
            // enabled TLS1.2
            ServicePointManager.SecurityProtocol |=
                SecurityProtocolType.Tls12;

            var app = new System.Windows.Application()
            {
                ShutdownMode = System.Windows.ShutdownMode.OnMainWindowClose
            };

            RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;

            app.Run(new MainView());

#if false
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
#endif
        }
    }
}

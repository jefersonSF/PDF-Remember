using Microsoft.Win32;
using System.Configuration;
using System.Data;
using System.Windows;

namespace PDFViewLastReadedPage
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            string? caminhoPdf = e.Args.Length > 0
                ? e.Args[0]
                : null;

            var janela = new MainWindow(caminhoPdf);
            ConfigRegisterWindows();
            janela.Show();
        }

        static void ConfigRegisterWindows()
        {
            string exePath = Environment.ProcessPath!;
            string exeName = System.IO.Path.GetFileName(exePath);

            using (var key = Registry.CurrentUser.CreateSubKey($@"Software\Classes\Applications\{exeName}\shell\open\command"))
            {
                key.SetValue("", $"\"{exePath}\" \"%1\"");
            }

            using (var key = Registry.CurrentUser.CreateSubKey($@"Software\Classes\.pdf\OpenWithList\{exeName}")) { }
        }
    }

}

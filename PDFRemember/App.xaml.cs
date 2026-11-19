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
            //string exeName = System.IO.Path.GetFileName(exePath);
            //string exeName = "PDF Remember";
            //string exePath = Environment.ProcessPath!;
            string progId = "PdfRemember.Document"; // Definir um ProgID exclusivo
            string friendlyName = "PDF Remember";   // Nome exibido no menu

            // 1. Registrar ProgID e FriendlyAppName
            using (var progIdKey = Registry.CurrentUser.CreateSubKey($@"Software\Classes\{progId}"))
            {
                progIdKey.SetValue("", "PDF Remember Document"); // Descrição do tipo de arquivo

                using (var shellKey = progIdKey.CreateSubKey(@"shell\open"))
                {
                    // A chave é FriendlyAppName, o valor é o nome exibido
                    shellKey.SetValue("FriendlyAppName", friendlyName);

                    using (var cmdKey = shellKey.CreateSubKey("command"))
                    {
                        cmdKey.SetValue("", $"\"{exePath}\" \"%1\"");
                    }
                }
            }

            // 2. Associar o ProgID à extensão .pdf (usar OpenWithProgIds)
            using (var pdfKey = Registry.CurrentUser.CreateSubKey(@"Software\Classes\.pdf\OpenWithProgIds"))
            {
                // O nome do valor é o ProgID, os dados podem estar vazios
                pdfKey.SetValue(progId, "", RegistryValueKind.String);
            }

            // 3. (Opcional) Se quiser que seu aplicativo apareça também no menu "Abrir com" para todos os arquivos
            // usando (var appKey = Registry.CurrentUser.CreateSubKey($@"Software\Classes\Applications\{Path.GetFileName(exePath)}"))
            // {
            //     using (var supported = appKey.CreateSubKey("SupportedTypes"))
            //         supported.SetValue(".pdf", "");
            // }
        }
    }

}

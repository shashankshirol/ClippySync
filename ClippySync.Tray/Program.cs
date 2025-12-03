using ClippySync.Web;
using Microsoft.Extensions.Hosting;

namespace ClippySync.Tray;

internal static class Program
{
    /// <summary>
    ///     The main entry point for the application.
    /// </summary>
    [STAThread]
    private static void Main(string[] args)
    {
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        var webApplication = ClippyWebApp.BuildWebApp(args);
        webApplication.Start();
        ApplicationConfiguration.Initialize();
        Application.Run(new TrayContent(webApplication));
    }
}
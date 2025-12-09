using ClippySync.Web;
using Microsoft.AspNetCore.Builder;
using System.Runtime.CompilerServices;

namespace ClippySync.Tray;

public class TrayContent : ApplicationContext
{
    private readonly NotifyIcon _trayIcon;
    private readonly WebApplication _webapp;
    private ConnectionWizard? _connectionWizard;

    public TrayContent(WebApplication webapp)
    {
        _webapp = webapp;

        var contextMenu = new ContextMenuStrip();
        var showInfoItem = new ToolStripMenuItem("Show connection info", null, OnShowInfoClicked);
        var exitItem = new ToolStripMenuItem("Exit", null, OnExitClicked);

        contextMenu.Items.Add(showInfoItem);
        contextMenu.Items.Add(new ToolStripSeparator());
        contextMenu.Items.Add(exitItem);

        _trayIcon = new NotifyIcon
        {
            Icon = GetIcon(),
            Visible = true,
            Text = "ClippySync",
            ContextMenuStrip = contextMenu
        };

        _trayIcon.MouseClick += NotifyIconOnMouseClick;
    }

    private Icon GetIcon()
    {
        var assembly = typeof(TrayContent).Assembly;
        using var stream = assembly.GetManifestResourceStream("ClippySync.Tray.Resources.ClippySync.Tray.ico");
        if (stream == null) throw new InvalidOperationException("Could not find embedded icon resource.");
        return new Icon(stream);
    }

    private void NotifyIconOnMouseClick(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left) ShowConnectionWizard();
    }

    private void OnShowInfoClicked(object? sender, EventArgs e)
    {
        ShowConnectionWizard();
    }

    private void ShowConnectionWizard()
    {
        var ip = Util.GetLocalIPv4();
        var port = Util.GetPort();

        var apiBaseUrl = $"http://{ip}:{port}";
        var deviceKey = Environment.MachineName;

        if (ip == null)
        {
            MessageBox.Show("Could not determine local IP address.", "ClippySync", MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        var shortcuts = QRGenerator.GenerateAllShortcutQrs(apiBaseUrl, deviceKey);
        _connectionWizard = new ConnectionWizard(shortcuts, apiBaseUrl, deviceKey);

        _connectionWizard.Show();
        _connectionWizard.BringToFront();
        _connectionWizard.Activate();
    }

    private async void OnExitClicked(object? sender, EventArgs e)
    {
        try
        {
            _trayIcon.Visible = false;
            _trayIcon.Dispose();

            try
            {
                await _webapp.StopAsync();
            }
            finally
            {
                await _webapp.DisposeAsync();
            }
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync(ex.Message);
        }
        finally
        {
            Application.Exit();
        }
    }
}
using ClippySync.Web;
using Microsoft.AspNetCore.Builder;

namespace ClippySync.Tray;

public class TrayContent : ApplicationContext
{
    private readonly NotifyIcon _trayIcon;
    private readonly WebApplication _webapp;

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
        if (e.Button == MouseButtons.Left) ShowConnectionInfo();
    }

    private void OnShowInfoClicked(object? sender, EventArgs e)
    {
        ShowConnectionInfo();
    }

    private void ShowConnectionInfo()
    {
        var ip = Util.GetLocalIPv4();
        if (ip == null)
        {
            MessageBox.Show("Could not determine local IP address.", "ClippySync", MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        var port = 8877; // keep in sync with your server
        var message =
            $"ClippySync is running.\n\n" +
            $"Local IP: {ip}\n" +
            $"URL: http://{ip}:{port}\n\n" +
            $"Use this URL in your iOS Shortcuts.";

        MessageBox.Show(message, "ClippySync", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
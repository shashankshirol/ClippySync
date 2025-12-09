namespace ClippySync.Tray;

public partial class ConnectionWizard : Form
{
    private readonly Dictionary<string, Bitmap> _shortcuts;
    private readonly string _apiBaseUrl;
    private readonly string _deviceKey;

    private Label lblTitle = null!;
    private Label lblSubtitle = null!;
    private TabControl tabSteps = null!;
    private TabPage tabInstall = null!;
    private TabPage tabConfigure = null!;

    private PictureBox pbSetClipboard = null!;
    private PictureBox pbGetClipboard = null!;
    private PictureBox pbConfigureImport = null!;
    private PictureBox pbConfigureRun = null!;

    private Label lblSetClipboard = null!;
    private Label lblGetClipboard = null!;
    private Label lblConfigureImport = null!;
    private Label lblConfigureRun = null!;

    private Button btnBack = null!;
    private Button btnNext = null!;
    private Button btnClose = null!;

    public ConnectionWizard(
        Dictionary<string, Bitmap> shortcuts,
        string apiBaseUrl,
        string deviceKey,
        string configureShortcutName = "ClippySync Configure")
    {
        _shortcuts = shortcuts;
        _apiBaseUrl = apiBaseUrl;
        _deviceKey = deviceKey;

        InitializeComponent();
        SetQRCodes();
        UpdateButtons();
    }

    private void InitializeComponent()
    {
        Text = "ClippySync – iOS Setup";
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = true;
        ClientSize = new Size(960, 720);

        // Title
        lblTitle = new Label
        {
            Text = "Set up ClippySync on your iPhone",
            Font = new Font(Font.FontFamily, 12, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(16, 12)
        };

        lblSubtitle = new Label
        {
            Text = "Follow these steps to install the Shortcuts and configure this device.",
            AutoSize = true,
            Location = new Point(16, 38)
        };

        // Tab control
        tabSteps = new TabControl
        {
            Location = new Point(16, 68),
            Size = new Size(ClientSize.Width - 32, ClientSize.Height - 130),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
        };

        tabInstall = new TabPage("1. Install Shortcuts");
        tabConfigure = new TabPage("2. Configure Device");

        tabSteps.TabPages.Add(tabInstall);
        tabSteps.TabPages.Add(tabConfigure);
        tabSteps.SelectedIndexChanged += (_, __) => UpdateButtons();

        InitializeInstallPage();
        InitializeConfigurePage();

        // Bottom buttons
        btnBack = new Button
        {
            Text = "Back",
            Enabled = false,
            Location = new Point(ClientSize.Width - 270, ClientSize.Height - 50),
            Anchor = AnchorStyles.Bottom | AnchorStyles.Right
        };
        btnBack.Click += (_, __) => GoToPreviousStep();

        btnNext = new Button
        {
            Text = "Next ▶",
            Location = new Point(ClientSize.Width - 180, ClientSize.Height - 50),
            Anchor = AnchorStyles.Bottom | AnchorStyles.Right
        };
        btnNext.Click += (_, __) => GoToNextStep();

        btnClose = new Button
        {
            Text = "Close",
            Location = new Point(ClientSize.Width - 90, ClientSize.Height - 50),
            Anchor = AnchorStyles.Bottom | AnchorStyles.Right
        };
        btnClose.Click += (_, __) => Close();

        Controls.Add(lblTitle);
        Controls.Add(lblSubtitle);
        Controls.Add(tabSteps);
        Controls.Add(btnBack);
        Controls.Add(btnNext);
        Controls.Add(btnClose);
    }

    private void InitializeInstallPage()
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(10)
        };

        var lblIntro = new Label
        {
            Text = "Step 1: Setup the Shortcuts",
            Font = new Font(Font.FontFamily, 10, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(0, 0)

        };
        // Root layout inside the group: grid + tip at the bottom
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(0),
            Margin = new Padding(0)
        };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Intro
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // grid
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));     // tip

        // Grid for the three cards
        var grid = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2,
            Padding = new Padding(0),
            Margin = new Padding(0)
        };
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        grid.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        grid.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

        // Card 1 – Set Clipboard
        pbSetClipboard = CreateQrPictureBox();
        lblSetClipboard = CreateCardLabel("Set Clipboard shortcut");
        var panelSet = CreateCardPanel(
            pbSetClipboard,
            lblSetClipboard,
            "Installs the shortcut used to send text from your phone to this PC.");

        // Card 2 – Get Clipboard
        pbGetClipboard = CreateQrPictureBox();
        lblGetClipboard = CreateCardLabel("Get Clipboard shortcut");
        var panelGet = CreateCardPanel(
            pbGetClipboard,
            lblGetClipboard,
            "Installs the shortcut used to pull this PC's clipboard onto your phone.");

        // Card 3 – Configure shortcut
        pbConfigureImport = CreateQrPictureBox();
        lblConfigureImport = CreateCardLabel("Configure shortcut");
        var panelConfigure = CreateCardPanel(
            pbConfigureImport,
            lblConfigureImport,
            "Installs the helper shortcut used to store your device settings.");

        // Put panels into the grid
        grid.Controls.Add(panelSet, 0, 0);
        grid.Controls.Add(panelGet, 1, 0);
        grid.Controls.Add(panelConfigure, 0, 1);
        grid.SetColumnSpan(panelConfigure, 2);

        // Tip at the bottom
        var lblTip = new Label
        {
            Text = "Tip: Open the Camera app on your iPhone and point it at a QR code to install the shortcut.",
            AutoSize = true,
            Dock = DockStyle.Fill,
        };

        root.Controls.Add(lblIntro, 0, 0);
        root.Controls.Add(grid, 0, 1);
        root.Controls.Add(lblTip, 0, 2);

        panel.Controls.Add(root);
        tabInstall.Controls.Add(panel);
    }

    private void InitializeConfigurePage()
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(10)
        };

        var lblIntro = new Label
        {
            Text = "Step 2: Configure this device",
            Font = new Font(Font.FontFamily, 10, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(0, 0)
        };

        var lblDesc = new Label
        {
            Text = "Scan this QR code to run the \"Configure\" shortcut. It will store your PC's API URL and device key on your iPhone.",
            AutoSize = true,
            Location = new Point(0, 24),
            MaximumSize = new Size(550, 0)
        };

        pbConfigureRun = new PictureBox
        {
            Size = new Size(220, 220),
            Location = new Point(10, 70),
            SizeMode = PictureBoxSizeMode.Zoom,
            BorderStyle = BorderStyle.FixedSingle
        };

        lblConfigureRun = new Label
        {
            Text = "Configure this device",
            AutoSize = true,
            Location = new Point(10, 300),
            Font = new Font(Font.FontFamily, 9, FontStyle.Bold)
        };

        var lblDetails = new Label
        {
            Text = $"These values will be sent to the shortcut:\r\n • API base URL: {_apiBaseUrl}\r\n • Device key: {_deviceKey}",
            AutoSize = true,
            Location = new Point(250, 90),
            MaximumSize = new Size(330, 0)
        };

        var lblNote = new Label
        {
            Text = "You only need to run this once per device, or when changing your ClippySync settings.",
            AutoSize = true,
            Location = new Point(250, 180),
            MaximumSize = new Size(330, 0)
        };

        panel.Controls.Add(lblIntro);
        panel.Controls.Add(lblDesc);
        panel.Controls.Add(pbConfigureRun);
        panel.Controls.Add(lblConfigureRun);
        panel.Controls.Add(lblDetails);
        panel.Controls.Add(lblNote);

        tabConfigure.Controls.Add(panel);
    }

    private PictureBox CreateQrPictureBox()
    {
        return new PictureBox
        {
            Size = new Size(140, 140),
            SizeMode = PictureBoxSizeMode.Zoom,
            Anchor = AnchorStyles.None,
            BorderStyle = BorderStyle.FixedSingle,
            Margin = new Padding(5)
        };
    }

    private Label CreateCardLabel(string text)
    {
        return new Label
        {
            Text = text,
            Font = new Font(Font.FontFamily, 9, FontStyle.Bold),
            AutoSize = true,
            Anchor = AnchorStyles.None,
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Top,
            Padding = new Padding(0, 4, 0, 4)
        };
    }

    private Panel CreateCardPanel(PictureBox pictureBox, Label titleLabel, string description)
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(8),
            Padding = new Padding(8),
            BorderStyle = BorderStyle.FixedSingle
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3
        };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));        // title
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));    // QR
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));        // description


        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        pictureBox.Anchor = AnchorStyles.None;
        pictureBox.Margin = new Padding(4);

        var lblDesc = new Label
        {
            Text = description,
            Anchor = AnchorStyles.None,
            Dock = DockStyle.Fill,
            AutoSize = true,
            Margin = new Padding(4, 4, 4, 0)
        };

        layout.Controls.Add(titleLabel, 0, 0);
        layout.Controls.Add(pictureBox, 0, 1);
        layout.Controls.Add(lblDesc, 0, 2);

        panel.Controls.Add(layout);
        return panel;
    }

    private void SetQRCodes()
    {
        // Step 1 QR codes – import shortcuts from iCloud
        if (_shortcuts.GetValueOrDefault("iOS2Win") != null)
            pbSetClipboard.Image = _shortcuts["iOS2Win"];

        if (_shortcuts.GetValueOrDefault("Win2iOS") != null)
            pbGetClipboard.Image = _shortcuts["Win2iOS"];

        if (_shortcuts.GetValueOrDefault("Config") != null)
            pbConfigureImport.Image = _shortcuts["Config"];

        // Step 2 QR code – run configure shortcut with payload
        if(_shortcuts.GetValueOrDefault("RunConfig") != null)
            pbConfigureRun.Image = _shortcuts["RunConfig"];

        return;
    }

    private void GoToNextStep()
    {
        if (tabSteps.SelectedIndex < tabSteps.TabPages.Count - 1)
        {
            tabSteps.SelectedIndex++;
        }
        else
        {
            Close();
        }
    }

    private void GoToPreviousStep()
    {
        if (tabSteps.SelectedIndex > 0)
            tabSteps.SelectedIndex--;
    }

    private void UpdateButtons()
    {
        btnBack.Enabled = tabSteps.SelectedIndex > 0;

        if (tabSteps.SelectedIndex == tabSteps.TabPages.Count - 1)
            btnNext.Text = "Done ✔";
        else
            btnNext.Text = "Next ▶";
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        // dispose bitmaps to avoid leaks
        pbSetClipboard.Image?.Dispose();
        pbGetClipboard.Image?.Dispose();
        pbConfigureImport.Image?.Dispose();
        pbConfigureRun.Image?.Dispose();

        base.OnFormClosed(e);
    }
}

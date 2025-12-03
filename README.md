# ClippySync

<p align="center">
  <img src="assets/logo.svg" width="400" alt="ClippySync logo">
</p>

ClippySync is a lightweight clipboard synchronization tool that runs as a Windows System Tray application. It hosts a local .NET Web API that allows trusted devices on your network (such as a phone via iOS Shortcuts) to read from and write to your computer's clipboard.

## Features

- **System Tray Integration**: Runs quietly in the background with a tray icon.
- **Clipboard API**:
  - `GET /clipboard`: Retrieve text from the PC clipboard.
  - `POST /set-clipboard`: Send text to the PC clipboard.
- **Simple Authentication**: Secures endpoints using a device key header (defaults to the host Machine Name).
- **Connection Info**: Easily view your local IP and connection URL via the tray menu.

## Project Structure

- **[ClippySync.Tray](ClippySync.Tray/ClippySync.Tray.csproj)**: A Windows Forms application that hosts the web server and manages the system tray icon.
- **[ClippySync.Web](ClippySync.Web/ClippySync.Web.csproj)**: The ASP.NET Core Web API logic that handles the actual clipboard operations and authentication.

## Getting Started

### Running the Application

1. Build the solution.
2. Run the `ClippySync.Tray` project.
3. An icon will appear in your system tray.
4. Click the icon (or right-click and select "Show connection info") to see the IP address and port (default: `8877`).

### Running via Command Line (Dev)

You can also run the web API standalone without the tray icon:

```powershell
cd ClippySync.Web
dotnet run
```

## API Usage

### Authentication

Requests to clipboard endpoints require the `X-Device-Key` header.
- **Header Name**: `X-Device-Key`
- **Value**: Must match the host's `Environment.MachineName` (case-insensitive).

### Endpoints

| Method | Endpoint | Description |
| :--- | :--- | :--- |
| `GET` | `/clipboard` | Returns the current text in the host clipboard. |
| `POST` | `/set-clipboard` | Sets the host clipboard to the text provided in the request body. |
| `GET` | `/get-device-name` | Returns the machine name (no auth required). |

### Examples

**Get Clipboard:**
```bash
curl -H "X-Device-Key: MY-PC-NAME" http://192.168.1.x:8877/clipboard
```

**Set Clipboard:**
```bash
curl -X POST -H "X-Device-Key: MY-PC-NAME" -d "Hello World" http://192.168.1.x:8877/set-clipboard
```

## iOS Shortcuts

This tool is designed to work with iOS Shortcuts. You can create a shortcut on your iPhone that:

- Put the iOS clipboard content on Windows clipboard.
- Get the Windows clipboard content on iOS clipboard.

### Push iOS Clipboard to Windows

1. Open the Shortcuts app on iOS and create a new shortcut.
2. Add the `Get Clipboard` action so the shortcut reads the current iOS clipboard.
3. Add a `Text` action and set it to your ClippySync endpoint, for example `http://192.168.1.x:8877/set-clipboard`.
4. Add a `Get Contents of URL` action.
  - Set `Method` to `POST`.
  - Set `Request Body` to `Form` → `Request Body` → `Text` and choose the output of `Get Clipboard`.
  - Add a header with key `X-Device-Key` and value matching your PC's `Environment.MachineName`.
5. Optionally add `Show Result` or `Notification` actions so you get confirmation when the clipboard sync succeeds.
6. Save the shortcut and add it to the share sheet or home screen for quick access.

### Pull Windows Clipboard to iOS

1. Create another shortcut in the Shortcuts app.
2. Add a `Text` action containing your ClippySync endpoint, e.g. `http://192.168.1.x:8877/clipboard`.
3. Add a `Get Contents of URL` action.
  - Ensure `Method` is `GET`.
  - Add the `X-Device-Key` header with your PC's device key.
4. Add a `Set Clipboard` action and feed it the response from `Get Contents of URL`.
5. Optionally add a `Show Result` action to preview the fetched text before it replaces your iOS clipboard.
6. Save the shortcut and trigger it through Siri, widgets, or the share sheet.

> Tip: If your network assigns IPs dynamically, consider creating a Shortcut variable for the base URL so you only update it in one place when it changes.
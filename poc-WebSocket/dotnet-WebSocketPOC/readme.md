## Usage

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download) or later
- [Node.js](https://nodejs.org/) (for frontend client)

### Running the Server

#### Using dotnet run (Development)
```bash
# Navigate to project directory
cd dotnet-websocket-poc

# Restore dependencies
dotnet restore

# Run the application
dotnet run

# Or specify a port
dotnet run --urls="http://localhost:5000"
```

#### Using Visual Studio
1. Open `WebSocketPOC.csproj`
2. Press `F5` or click the "Run" button
3. Server will start on `http://localhost:5000`

#### Using Docker (Optional)
```bash
# Build the image
docker build -t dotnet-websocket-poc .

# Run the container
docker run -p 5000:5000 dotnet-websocket-poc
```

### Shutting Down the Server

#### If running with dotnet run
Press `Ctrl + C` in the terminal window where the server is running.

#### If running with Visual Studio
Press `Shift + F5` or click the "Stop" button in Visual Studio.

#### If running as a background process
```bash
# On Linux/Mac
kill $(ps aux | grep 'dotnet WebSocketPOC.dll' | grep -v grep | awk '{print $2}')

# On Windows (PowerShell)
Get-Process -Name "dotnet" | Where-Object {$_.CommandLine -like "*WebSocketPOC*"} | Stop-Process
```

### Testing the Connection

#### 1. Using the React Client
```bash
cd ../react-websocket-client
npm install
npm start
```
Open `http://localhost:3000` in your browser.

#### 2. Using WebSocket Client Test Page
Open `ws-test.html` in any browser:
```bash
# Serve the test page
npx serve .
# Then open http://localhost:3000/ws-test.html
```

#### 3. Using cURL (WebSocket not supported)
For WebSocket testing, use `wscat`:
```bash
# Install wscat
npm install -g wscat

# Connect to server
wscat -c ws://localhost:5000/ws
```

#### 4. Using Online WebSocket Testers
- [WebSocket.org Echo Test](https://websocket.org/tools/websocket-echo-tool/)
- [PieSocket WebSocket Tester](https://www.piesocket.com/websocket-tester)
- URL: `ws://localhost:5000/ws`

### Verifying Server is Running
```bash
# Check if server is responding
curl http://localhost:5000/
# Expected response: "WebSocket Server Running"
```

### Logs
Server logs are output to the console by default. To save logs to a file:
```bash
dotnet run > server.log 2>&1
```

### Common Issues & Solutions

| Issue | Solution |
|-------|----------|
| Port 5000 already in use | Change port in `launchSettings.json` or use `--urls="http://localhost:5001"` |
| CORS errors | Ensure React app is running on `http://localhost:3000` |
| WebSocket connection fails | Check firewall settings and ensure no proxy is blocking |
| Connection refused | Verify server is running and port is correct |

### Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `ASPNETCORE_URLS` | Server URLs | `http://localhost:5000` |
| `ASPNETCORE_ENVIRONMENT` | Environment (Development/Production) | `Development` |

### Production Deployment

#### Publish to folder
```bash
dotnet publish -c Release -o ./publish
```

#### Run published version
```bash
cd publish
dotnet WebSocketPOC.dll
```

#### Run as Windows Service
```bash
sc.exe create WebSocketPOC binPath="C:\path\to\dotnet.exe C:\path\to\WebSocketPOC.dll"
```

#### Run as Linux daemon (systemd)
Create `/etc/systemd/system/websocket-poc.service`:
```ini
[Unit]
Description=WebSocket POC .NET Service
After=network.target

[Service]
Type=simple
ExecStart=/usr/bin/dotnet /path/to/WebSocketPOC.dll
Restart=on-failure
User=www-data

[Install]
WantedBy=multi-user.target
```
Then:
```bash
sudo systemctl start websocket-poc
sudo systemctl enable websocket-poc
```
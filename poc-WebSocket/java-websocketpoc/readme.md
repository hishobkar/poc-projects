## Usage

### Prerequisites
- [Java 17](https://adoptium.net/) or later
- [Maven 3.6+](https://maven.apache.org/) or [Gradle](https://gradle.org/)
- [Node.js](https://nodejs.org/) (for frontend client)

### Running the Server

#### Using Maven (Recommended)
```bash
# Navigate to project directory
cd springboot-websocket-poc

# Clean and compile
mvn clean compile

# Run the application
mvn spring-boot:run

# Or with specific profile
mvn spring-boot:run -Dspring-boot.run.profiles=dev
```

#### Using the JAR file
```bash
# Build the JAR
mvn clean package

# Run the JAR
java -jar target/websocket-poc-0.0.1-SNAPSHOT.jar

# Or with custom port
java -jar target/websocket-poc-0.0.1-SNAPSHOT.jar --server.port=8081
```

#### Using IntelliJ IDEA
1. Open the project
2. Find `WebsocketPocApplication.java`
3. Right-click → Run 'WebsocketPocApplication'
4. Server will start on `http://localhost:8080`

#### Using VS Code
1. Install Spring Boot Extension Pack
2. Open the project
3. Click "Run" on the main application class
4. Or use terminal: `mvn spring-boot:run`

#### Using Docker
```bash
# Build the image
mvn spring-boot:build-image

# Run the container
docker run -p 8080:8080 websocket-poc:0.0.1-SNAPSHOT
```

#### Using Gradle (if configured)
```bash
# Run with Gradle
./gradlew bootRun

# Or with specific profile
./gradlew bootRun --args='--spring.profiles.active=dev'
```

### Shutting Down the Server

#### If running with Maven
Press `Ctrl + C` in the terminal where the server is running.

#### If running as a JAR
Press `Ctrl + C` or close the terminal.

#### If running in IntelliJ/VS Code
Click the red square "Stop" button in the IDE.

#### Using Spring Boot Actuator (if enabled)
```bash
# Graceful shutdown
curl -X POST http://localhost:8080/actuator/shutdown

# Or using POST with content
curl -X POST http://localhost:8080/actuator/shutdown \
  -H "Content-Type: application/json" \
  -d '{"operation":"shutdown"}'
```

#### Find and kill process (Linux/Mac)
```bash
# Find the process
ps aux | grep websocket-poc

# Kill the process
kill -15 <PID>

# Or kill all instances
pkill -f websocket-poc
```

#### Find and kill process (Windows PowerShell)
```powershell
# Find the process
Get-Process -Name "java" | Where-Object {$_.CommandLine -like "*websocket-poc*"}

# Stop the process
Stop-Process -Id <PID> -Force
```

### Testing the Connection

#### 1. Health Check
```bash
curl http://localhost:8080/
# Expected: {"status":"WebSocket Server Running","version":"1.0.0"}
```

#### 2. Using the React Client
```bash
cd ../react-websocket-client
npm install
npm start
```
Open `http://localhost:3000` in your browser.

#### 3. Using wscat (WebSocket CLI)
```bash
# Install wscat
npm install -g wscat

# Connect to WebSocket
wscat -c ws://localhost:8080/ws

# Send a message
> Hello from wscat!

# Expected response
< {"type":"confirmation","content":"Message delivered",...}
< {"type":"message","content":"Hello from wscat!",...}
```

#### 4. Using JavaScript in Browser Console
```javascript
const ws = new WebSocket('ws://localhost:8080/ws');

ws.onopen = () => console.log('Connected');
ws.onmessage = (e) => console.log('Received:', JSON.parse(e.data));
ws.onclose = () => console.log('Disconnected');

// Send a message
ws.send('Hello from browser console!');
```

#### 5. Using Postman
1. Create new WebSocket Request
2. URL: `ws://localhost:8080/ws`
3. Click "Connect"
4. Send messages in the "Message" tab

#### 6. Multiple Client Testing
Open multiple browser windows/tabs at `http://localhost:3000` and send messages to see real-time broadcasting.

### Logs & Monitoring

#### Viewing logs in real-time
```bash
# Maven with logs
mvn spring-boot:run -Dspring-boot.run.arguments="--logging.level.com.example=DEBUG"

# JAR with logs
java -jar target/websocket-poc-0.0.1-SNAPSHOT.jar --debug
```

#### Enable detailed WebSocket logging
```yaml
# In application.properties
logging.level.org.springframework.web.socket=DEBUG
logging.level.com.example.websocketpoc=DEBUG
```

### Configuration

#### application.properties
```properties
# Server
server.port=8080

# Logging
logging.level.com.example.websocketpoc=DEBUG

# CORS (allow React app)
spring.mvc.cors.allowed-origins=http://localhost:3000
```

#### Environment Variables
```bash
# Custom port
java -jar target/websocket-poc-0.0.1-SNAPSHOT.jar --server.port=9090

# Multiple origins for CORS
java -jar target/websocket-poc-0.0.1-SNAPSHOT.jar \
  --spring.mvc.cors.allowed-origins=http://localhost:3000,http://localhost:3001
```

### Common Issues & Solutions

| Issue | Solution |
|-------|----------|
| Port 8080 already in use | Use `--server.port=8081` or change in application.properties |
| CORS errors | Verify `allowedOrigins` includes your frontend URL |
| WebSocket connection fails | Check if server is running and no firewall blocking port |
| Java version mismatch | Ensure Java 17+ is installed: `java -version` |
| Maven build fails | Run `mvn clean` then `mvn compile` again |

### Production Deployment

#### Build for production
```bash
# Build with Maven
mvn clean package -Pproduction

# Build with specific profile
mvn clean package -Dspring.profiles.active=prod
```

#### Run in production mode
```bash
# Using profile
java -jar target/websocket-poc-0.0.1-SNAPSHOT.jar --spring.profiles.active=prod

# With external config
java -jar target/websocket-poc-0.0.1-SNAPSHOT.jar \
  --spring.config.location=file:/path/to/application-prod.yml
```

#### Run as a systemd service (Linux)
Create `/etc/systemd/system/websocket-poc.service`:
```ini
[Unit]
Description=WebSocket POC Spring Boot Service
After=network.target

[Service]
Type=simple
ExecStart=/usr/bin/java -jar /path/to/websocket-poc-0.0.1-SNAPSHOT.jar
Restart=on-failure
User=www-data
RestartSec=10

[Install]
WantedBy=multi-user.target
```
Then:
```bash
sudo systemctl start websocket-poc
sudo systemctl enable websocket-poc
sudo systemctl status websocket-poc
```

#### Run as a Windows Service
```bash
# Using WinSW (Windows Service Wrapper)
# 1. Download WinSW
# 2. Create websocket-poc.xml
<service>
  <id>websocket-poc</id>
  <name>WebSocket POC</name>
  <description>WebSocket POC Spring Boot Service</description>
  <executable>java</executable>
  <arguments>-jar websocket-poc-0.0.1-SNAPSHOT.jar</arguments>
</service>

# 3. Install service
websocket-poc.exe install
```

### API Endpoints

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/` | GET | Health check |
| `/ws` | WebSocket | WebSocket connection endpoint |
| `/actuator/health` | GET | Detailed health check (if actuator enabled) |
| `/actuator/shutdown` | POST | Graceful shutdown (if enabled) |
#!/bin/bash

# Start all services
echo "Starting all services..."

# Start .NET backend
cd dotnet-WebSocket-poc
dotnet run &
NET_PID=$!

# Start Spring Boot backend
cd ../java-websocketpoc
mvn spring-boot:run &
SPRING_PID=$!

# Start React frontend
cd ../websocket-poc-client
npm start &
REACT_PID=$!

echo "All services started!"
echo "NET PID: $NET_PID"
echo "Spring PID: $SPRING_PID"
echo "React PID: $REACT_PID"
echo "Press Ctrl+C to stop all services"

# Wait for interruption
trap "kill $NET_PID $SPRING_PID $REACT_PID; exit" INT
wait
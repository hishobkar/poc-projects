package com.example.websocketpoc.handler;

import com.fasterxml.jackson.databind.ObjectMapper;
import com.fasterxml.jackson.databind.node.ObjectNode;
import org.springframework.stereotype.Component;
import org.springframework.web.socket.CloseStatus;
import org.springframework.web.socket.TextMessage;
import org.springframework.web.socket.WebSocketSession;
import org.springframework.web.socket.handler.TextWebSocketHandler;

import java.time.LocalDateTime;
import java.time.format.DateTimeFormatter;
import java.util.concurrent.CopyOnWriteArrayList;

@Component
public class WebSocketHandler extends TextWebSocketHandler {
    
    private final CopyOnWriteArrayList<WebSocketSession> sessions = new CopyOnWriteArrayList<>();
    private final ObjectMapper objectMapper = new ObjectMapper();
    private final DateTimeFormatter formatter = DateTimeFormatter.ofPattern("HH:mm:ss");
    
    @Override
    public void afterConnectionEstablished(WebSocketSession session) throws Exception {
        sessions.add(session);
        System.out.println("Client connected: " + session.getId() + 
                          ". Total clients: " + sessions.size());
        
        // Send welcome message
        ObjectNode welcomeMessage = objectMapper.createObjectNode();
        welcomeMessage.put("type", "system");
        welcomeMessage.put("content", "Welcome to WebSocket Server!");
        welcomeMessage.put("timestamp", LocalDateTime.now().format(formatter));
        
        session.sendMessage(new TextMessage(welcomeMessage.toString()));
    }
    
    @Override
    protected void handleTextMessage(WebSocketSession session, TextMessage message) throws Exception {
        String payload = message.getPayload();
        System.out.println("Received from " + session.getId() + ": " + payload);
        
        // Create broadcast message
        ObjectNode broadcastMessage = objectMapper.createObjectNode();
        broadcastMessage.put("type", "message");
        broadcastMessage.put("content", payload);
        broadcastMessage.put("timestamp", LocalDateTime.now().format(formatter));
        broadcastMessage.put("clientCount", sessions.size() - 1);
        broadcastMessage.put("senderId", session.getId().substring(0, 8));
        
        String broadcastJson = broadcastMessage.toString();
        
        // Broadcast to all other clients
        for (WebSocketSession client : sessions) {
            if (!client.getId().equals(session.getId()) && client.isOpen()) {
                try {
                    client.sendMessage(new TextMessage(broadcastJson));
                } catch (Exception e) {
                    System.err.println("Error sending to client: " + e.getMessage());
                }
            }
        }
        
        // Send confirmation back to sender
        ObjectNode confirmation = objectMapper.createObjectNode();
        confirmation.put("type", "confirmation");
        confirmation.put("content", "Message delivered");
        confirmation.put("timestamp", LocalDateTime.now().format(formatter));
        
        session.sendMessage(new TextMessage(confirmation.toString()));
    }
    
    @Override
    public void afterConnectionClosed(WebSocketSession session, CloseStatus status) throws Exception {
        sessions.remove(session);
        System.out.println("Client disconnected: " + session.getId() + 
                          ". Total clients: " + sessions.size());
    }
}
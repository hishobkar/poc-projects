package com.example.websocketpoc.handler;

import com.example.websocketpoc.dto.MessageDTO;
import com.fasterxml.jackson.databind.ObjectMapper;
import org.springframework.stereotype.Component;
import org.springframework.web.socket.TextMessage;
import org.springframework.web.socket.WebSocketSession;
import org.springframework.web.socket.handler.TextWebSocketHandler;

import java.time.LocalDateTime;
import java.time.format.DateTimeFormatter;
import java.util.concurrent.ConcurrentHashMap;

@Component
public class EnhancedWebSocketHandler extends TextWebSocketHandler {
    
    private final ConcurrentHashMap<String, WebSocketSession> sessions = new ConcurrentHashMap<>();
    private final ObjectMapper objectMapper = new ObjectMapper();
    private final DateTimeFormatter formatter = DateTimeFormatter.ofPattern("HH:mm:ss");
    
    @Override
    public void afterConnectionEstablished(WebSocketSession session) {
        sessions.put(session.getId(), session);
        System.out.println("Client connected: " + session.getId() + 
                          ". Total clients: " + sessions.size());
        
        broadcastSystemMessage("New user joined the chat!");
    }
    
    @Override
    protected void handleTextMessage(WebSocketSession session, TextMessage message) throws Exception {
        String payload = message.getPayload();
        System.out.println("Received from " + session.getId() + ": " + payload);
        
        MessageDTO dto = new MessageDTO(
            "message",
            payload,
            LocalDateTime.now().format(formatter),
            sessions.size() - 1,
            session.getId().substring(0, 8)
        );
        
        String json = objectMapper.writeValueAsString(dto);
        
        // Broadcast to all other clients
        sessions.values().stream()
            .filter(s -> !s.getId().equals(session.getId()) && s.isOpen())
            .forEach(s -> {
                try {
                    s.sendMessage(new TextMessage(json));
                } catch (Exception e) {
                    System.err.println("Error sending to client: " + e.getMessage());
                }
            });
        
        // Send confirmation
        MessageDTO confirmation = new MessageDTO(
            "confirmation",
            "Message delivered",
            LocalDateTime.now().format(formatter),
            null,
            null
        );
        session.sendMessage(new TextMessage(objectMapper.writeValueAsString(confirmation)));
    }
    
    @Override
    public void afterConnectionClosed(WebSocketSession session, CloseStatus status) {
        sessions.remove(session.getId());
        System.out.println("Client disconnected: " + session.getId() + 
                          ". Total clients: " + sessions.size());
        broadcastSystemMessage("User left the chat");
    }
    
    private void broadcastSystemMessage(String content) {
        try {
            MessageDTO systemMessage = new MessageDTO(
                "system",
                content,
                LocalDateTime.now().format(formatter),
                sessions.size(),
                "Server"
            );
            String json = objectMapper.writeValueAsString(systemMessage);
            
            sessions.values().stream()
                .filter(WebSocketSession::isOpen)
                .forEach(s -> {
                    try {
                        s.sendMessage(new TextMessage(json));
                    } catch (Exception e) {
                        System.err.println("Error sending system message: " + e.getMessage());
                    }
                });
        } catch (Exception e) {
            System.err.println("Error broadcasting system message: " + e.getMessage());
        }
    }
}
package com.example.websocketpoc;

import org.springframework.boot.SpringApplication;
import org.springframework.boot.autoconfigure.SpringBootApplication;

@SpringBootApplication
public class WebsocketpocApplication {
    
    public static void main(String[] args) {
        SpringApplication.run(WebsocketpocApplication.class, args);
        System.out.println("WebSocket Server started on ws://localhost:8080/ws");
    }
}
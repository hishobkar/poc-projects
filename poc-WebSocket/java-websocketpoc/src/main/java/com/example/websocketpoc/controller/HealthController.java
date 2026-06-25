package com.example.websocketpoc.controller;

import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RestController;

import java.util.HashMap;
import java.util.Map;

@RestController
public class HealthController {
    
    @GetMapping("/")
    public Map<String, String> health() {
        Map<String, String> response = new HashMap<>();
        response.put("status", "WebSocket Server Running");
        response.put("version", "1.0.0");
        return response;
    }
}
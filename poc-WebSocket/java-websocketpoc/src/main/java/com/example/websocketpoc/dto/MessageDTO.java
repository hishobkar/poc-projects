package com.example.websocketpoc.dto;

import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;

@Data
@NoArgsConstructor
@AllArgsConstructor
public class MessageDTO {
    private String type;
    private String content;
    private String timestamp;
    private Integer clientCount;
    private String senderId;
}
namespace ApiCs.DTOs;

public record RegisterRequest(string Username, string Password, string Name);
public record LoginRequest(string Username, string Password);
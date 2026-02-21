using System;
using Newtonsoft.Json;
namespace Scipts.Models.DTOs
{
    [System.Serializable]
    public class LoginDto
    { 
        [JsonProperty("username")]public string Username { get; set; } = default!; 
        [JsonProperty("password")]public string Password { get; set; } = default!;
    }
    
    [System.Serializable]
    public class TokenResponseDto
    {
        [JsonProperty("token")]public string Token { get; set; } = default!;
    }

    [System.Serializable]
    public class RegisterDto
    {
        [JsonProperty("username")] public string Username { get; set; } = default!;
        [JsonProperty("password")] public string Password { get; set; } = default!;
        [JsonProperty("email")] public string Email { get; set; } = default!;
    }

    [System.Serializable]
    public class RegisterResponseDto
    {
        [JsonProperty("httpStatusCode")] public int HttpStatusCode { get; set; }
    }

    [System.Serializable]
    public class GameDataDto
    {
        public GameDataDto(int points, int level)
        {
            Points = points;
            Level = level;
        }

        public GameDataDto(int points, int levelNumber,int rowsDeleted,int iPieces, int jPieces, int lPieces, int oPieces, int sPieces, int tPieces, int zPieces)
        {
            Points = points;
            Level = levelNumber;
            RowsDeleted = rowsDeleted;
            IPieces = iPieces;
            JPieces = jPieces;
            LPieces = lPieces;
            OPieces = oPieces;
            SPieces = sPieces;
            TPieces = tPieces;
            ZPieces = zPieces;
        }

        [JsonProperty("points")] public int Points { get; set; }
        [JsonProperty("level")] public int Level { get; set; }
        
        [JsonProperty("rows_deleted")] public int RowsDeleted { get; set; } 
        
        [JsonProperty("i_pieces")] public int IPieces { get; set; }
        [JsonProperty("o_pieces")] public int OPieces { get; set; }
        [JsonProperty("t_pieces")] public int TPieces { get; set; }
        [JsonProperty("s_pieces")] public int SPieces { get; set; }
        [JsonProperty("z_pieces")] public int ZPieces { get; set; }
        [JsonProperty("j_pieces")] public int JPieces { get; set; }
        [JsonProperty("l_pieces")] public int LPieces { get; set; }
        
    }
    [System.Serializable]
    public class RegisterResponse
    {
        public string message;
        public string token;

        // OPTIONAL (recommended): add this on the API response to avoid guessing from message text
        public bool requiresEmailVerification;
    }
    
    [Serializable]
    public class UsernameResponse
    {
        public string username;
    }

    [Serializable]
    public class TokenVerificationDTO
    {
        [JsonProperty("token")]
        public string Token { get; set; }
    }
}
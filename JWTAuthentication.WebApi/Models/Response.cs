namespace JWTAuthentication.WebApi.Models
{
    public class Response
    {
        public bool IsSuccess { get; set; }
        public string? Message { get; set; }
        public IEnumerable<string> Errors { get; set; }
        public DateTime? Expiration { get; set; }
    }
}

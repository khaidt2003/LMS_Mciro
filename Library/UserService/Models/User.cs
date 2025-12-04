namespace UserService.Models
{
    public class User
    {
        public int Id { get; set; }
        public required string Username { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
        
        public required string Email { get; set; }
        public int Roles { get; set; }
    }
}

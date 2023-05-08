namespace Tofu.BallBoi.Abstractions.DataTransferObjects;
    public class UserDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ulong DiscordId { get; set; }
        public bool HasInteracted { get; set; }
        public int AvailTokens { get; set; }
    }

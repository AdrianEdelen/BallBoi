namespace Tofu.BallBoi.Core.Services
{
    public class slashCommandService
    {
        private readonly ILogger<slashCommandService> _logger;
        public slashCommandService(ILogger<slashCommandService> logger)
        {
            _logger = logger;
        }
    }
}

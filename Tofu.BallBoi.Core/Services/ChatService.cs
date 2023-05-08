using Rystem.OpenAi;
namespace Tofu.BallBoi.Core.Services
{
    public class ChatService
    {
        private readonly ILogger<ChatService> _logger;
        private readonly string openAIkey;
        private readonly OpenAiService _ai;
        
        public ChatService(ILogger<ChatService> logger, OpenAiService ai)
        {
            _logger = logger;
            openAIkey = Environment.GetEnvironmentVariable("APIKEY");
            _ai = ai;

        }

    }
}

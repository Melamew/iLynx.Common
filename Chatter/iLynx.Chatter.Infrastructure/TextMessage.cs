using System.Text;

namespace iLynx.Chatter.Infrastructure
{
    public class TextMessage : ChatMessage
    {
        public TextMessage(string text)
        {
            Data = Encoding.Unicode.GetBytes(text);
            Key = MessageKeys.TextMessage;
        }
    }
}
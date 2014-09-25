using System.Text;
using iLynx.Chatter.Infrastructure;

namespace iLynx.TestBench.ClientServerDemo
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
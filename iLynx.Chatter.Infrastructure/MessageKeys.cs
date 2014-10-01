namespace iLynx.Chatter.Infrastructure
{
    public static class MessageKeys
    {
        public const int TextMessage = int.MinValue;
        public const int ExitMessage = int.MinValue + 1;
        public const int ChangeNickMessage = int.MinValue + 2;
        public const int CredentialAuthenticationRequest = int.MinValue + 3;
        public const int CredentialAuthenticationResponse = int.MinValue + 4;
        public const int CredentialAuthenticationRejected = int.MinValue + 5;
        public const int CredentialAuthenticationAccepted = int.MinValue + 6;
        public const int RequestNick = int.MinValue + 7;
        public const int RequestDenied = int.MinValue + 100;
    }
}

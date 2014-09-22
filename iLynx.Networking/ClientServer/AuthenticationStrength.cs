namespace iLynx.Networking.ClientServer
{
    // Fuzzy...
    public enum AuthenticationStrength
    {
        None = int.MinValue,
        Weak = int.MinValue + 10,
        Reasonable = 0,
        Strong = 10,
    }
}

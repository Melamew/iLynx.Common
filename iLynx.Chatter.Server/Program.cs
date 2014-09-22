namespace iLynx.Chatter.Server
{
    public class Program
    {
        public static void Main(params string[] args)
        {
            var bootstrapper = new ServerBootstrapper();
            bootstrapper.Run(true);
        }
    }
}

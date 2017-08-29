using Televic.Mycro.Web;

namespace Mycro.Sample.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            new Server<SampleStartup>().Start();
            System.Console.ReadLine();
        }
    }
}

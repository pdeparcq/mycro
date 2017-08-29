using CQRSlite.Commands;

namespace Televic.Mycro.Bus
{
    public class BaseCommand : ICommand
    {
        public int ExpectedVersion { get; set; }
    }
}

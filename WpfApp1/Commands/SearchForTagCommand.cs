using System.Diagnostics;
using WpfApp1.MVVM;

namespace WpfApp1.Commands
{
    public class SearchForTagCommand : CommandBase
    {
        public override void Execute(object? parameter)
        {
            Trace.WriteLine("SearchForTagCommand");
        }
    }
}


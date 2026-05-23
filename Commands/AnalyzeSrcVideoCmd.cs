using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneColumnEncoder.Commands
{
    public class AnalyzeSrcVideoCmd : AsyncBaseCmd
    {
        protected override Task ExecuteAsync(object? parameter)
        {
            return Task.CompletedTask;
        }
    }
}

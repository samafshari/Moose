using Moose.Models;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MooseDrive
{
    public class ELMDriver : Driver
    {
        public override void InjectMessage(string message)
        {
            base.InjectMessage(message);
        }

        public override Task SetupAsync()
        {
            return base.SetupAsync();
        }
    }
}

using Moose;

using MooseDrive.Logger;

using System;
using System.Collections.Generic;
using System.Text;

namespace MooseDrive.Mobile.App.Services
{
    public interface IDatabaseService
    {
        Database Db { get; }
        ELMLogger ELMLogger { get; }
    }
}

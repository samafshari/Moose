using System;
using System.Collections.Generic;
using System.Text;

namespace Moose.Models
{
    public class DriverException : Exception
    {
        public DriverException(string message) : base(message)
        {
        }
    }
}

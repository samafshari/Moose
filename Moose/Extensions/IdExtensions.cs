using System;
using System.Collections.Generic;
using System.Text;

namespace Moose
{
    public static class IdExtensions
    {
        public static string GenerateId()
        {
            return $"{DateTime.UtcNow.Ticks}_{GuidId()}";
        }

        static string GuidId()
        {
            return Guid.NewGuid().ToString().Replace("-", "");
        }
    }
}

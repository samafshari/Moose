using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
namespace MooseDrive.Messages
{
    // https://python-obd.readthedocs.io/en/latest/Command%20Tables/
    public abstract class ELMMessage
    {
        public virtual string Message { get; }
        public virtual object Result { get; protected set; }
        public bool IsSending { get; set; }

        public virtual bool ProcessResponse(string response)
        {
            return true;
        }
    }

    public abstract class ELMMessage<T> : ELMMessage
    {
        public new virtual T Result { get; protected set; }
    }

    public abstract class ELMStringMessage : ELMMessage<string>
    {
        public override bool ProcessResponse(string response)
        {
            Result = response?.Trim();
            return true;
        }
    }

    public abstract class ELMIntMessage : ELMMessage<int>
    {
        public override bool ProcessResponse(string response)
        {
            //> 010D
            //41 0D FF
            //To get the speed, simply convert the value to decimal:
            //
            //0xFF = 255 km / h

            var hex = string.Join("", response.Split(' ').Skip(2));
            var integer = int.Parse(hex, System.Globalization.NumberStyles.HexNumber);
            Result = integer;
            return true;
        }
    }

    public class ATE0 : ELMMessage
    {
        public override string Message { get; } = "AT E0";
    }

    public class ATL0 : ELMMessage
    {
        public override string Message { get; } = "AT L0";
    }

    public class ATST00 : ELMMessage
    {
        public override string Message { get; } = "AT ST 00";
    }

    public class ATSP00 : ELMMessage
    {
        public override string Message { get; } = "AT SP 00";
    }

    public class ATZ : ELMStringMessage
    {
        public override string Message { get; } = "ATZ";
    }

    public class EngineLoad : ELMIntMessage
    {
        public override string Message { get; } = "0104";

        public override bool ProcessResponse(string response)
        {
            if (base.ProcessResponse(response))
            {
                Result = (int)(100 * (double)Result / 255);
                return true;
            }

            return false;
        }
    }

    public class CoolantTemp : ELMIntMessage
    {
        public override string Message { get; } = "0105";

        public override bool ProcessResponse(string response)
        {
            if (base.ProcessResponse(response))
            {
                Result -= 40;
                return true;
            }

            return false;
        }
    }

    public class FuelPressure : ELMIntMessage
    {
        public override string Message { get; } = "010A";

        public override bool ProcessResponse(string response)
        {
            if (base.ProcessResponse(response))
            {
                Result = (int)(Result * 3);
                return true;
            }

            return false;
        }
    }

    public class IntakePressure : ELMMessage<int>
    {
        public override string Message { get; } = "010B";
    }

    public class RPM : ELMIntMessage
    {
        public override string Message { get; } = "010C";

        public override bool ProcessResponse(string response)
        {
            if (base.ProcessResponse(response))
            {
                Result = (int)(Result / 4);
                return true;
            }

            return false;
        }
    }

    public class Speed : ELMIntMessage
    {
        public override string Message { get; } = "010D";
    }

    //public class TimingAdvance : ELMMessage<int>
    //{
    //    public override string Message { get; } = "010E";
    //}

    public class IntakeTemp : ELMIntMessage
    {
        public override string Message { get; } = "010F";

        public override bool ProcessResponse(string response)
        {
            if (base.ProcessResponse(response))
            {
                Result -= 40;
                return true;
            }

            return false;
        }
    }

    public class MAF : ELMIntMessage
    {
        public override string Message { get; } = "0110";

        public override bool ProcessResponse(string response)
        {
            if (base.ProcessResponse(response))
            {
                Result = (int)(Result / 100);
                return true;
            }

            return false;
        }
    }

    public class ThrottlePos : ELMIntMessage
    {
        public override string Message { get; } = "0111";
    }
}

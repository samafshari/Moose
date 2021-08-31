using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MooseDrive.Messages
{
    // https://python-obd.readthedocs.io/en/latest/Command%20Tables/
    public abstract class ELMMessage
    {
        public virtual string Message { get; }
        public virtual object Result { get; protected set; }

        public virtual Task<bool> ProcessResponse(ELMDriver driver)
        {
            return Task.FromResult(true);
        }
    }

    public abstract class ELMMessage<T> : ELMMessage
    {
        public new virtual T Result { get; protected set; }
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

    public class EngineLoad : ELMMessage<int>
    {
        public override string Message { get; } = "0104";
    }

    public class CoolantTemp : ELMMessage<int>
    {
        public override string Message { get; } = "0105";
    }

    public class FuelPressure : ELMMessage<int>
    {
        public override string Message { get; } = "010A";
    }

    public class IntakePressure : ELMMessage<int>
    {
        public override string Message { get; } = "010B";
    }

    public class RPM : ELMMessage<int>
    {
        public override string Message { get; } = "010C";
    }

    public class Speed : ELMMessage<int>
    {
        public override string Message { get; } = "010D";
    }

    public class TimingAdvance : ELMMessage<int>
    {
        public override string Message { get; } = "010E";
    }

    public class IntakeTemp : ELMMessage<int>
    {
        public override string Message { get; } = "010F";
    }

    public class MAF : ELMMessage<int>
    {
        public override string Message { get; } = "0110";
    }

    public class ThrottlePos : ELMMessage<int>
    {
        public override string Message { get; } = "0111";
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Moose.Models
{
    public abstract class Driver
    {
        public Func<byte[], Task> WriteAsyncFunc;
        public Func<string, Task> LogAsyncFunc;

        public event EventHandler<Driver> OnDisconnectRequest;

        public virtual TimeSpan Timeout => TimeSpan.FromSeconds(10);
        public virtual TimeSpan RetryWait => TimeSpan.FromMilliseconds(10);

        public virtual void InjectMessage(byte[] bytes)
        {
        }

        public virtual void InjectMessage(string message)
        {
        }
        
        protected async Task LogAsync(string message)
        {
            await LogAsyncFunc?.Invoke(message);
        }

        protected async void Log(string message)
        {
            await LogAsync(message);
        }

        protected async Task WaitAndCrashIfFail(Func<bool> boolean)
        {
            if (!await WaitAsync(boolean, Timeout))
                throw new DriverException("Device communication timed out.");
        }

        protected async Task<bool> WaitAsync(Func<bool> condition, TimeSpan? timeout, TimeSpan? wait = null)
        {
            if (wait == null) wait = RetryWait;
            var start = DateTime.Now;
            while (condition())
            {
                await Task.Delay(wait.Value);
                if (timeout.HasValue)
                {
                    if (DateTime.Now - start > timeout.Value)
                        return false;
                }
            }
            return true;
        }
    }
}

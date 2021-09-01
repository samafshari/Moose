using Moose.Models;

using MooseDrive.Messages;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MooseDrive
{
    public class ELMDriver : Driver
    {
        public event EventHandler<ELMMessage> OnResponseToMessage;
        public event EventHandler OnUpdate;

        volatile bool isSending = false;
        readonly ConcurrentQueue<ELMMessage> queue = new ConcurrentQueue<ELMMessage>();

        public int RPM { get; private set; }
        public int Speed { get; private set; }
        public int MAF { get; private set; }
        public int EngineLoad { get; private set; }

        public readonly List<ELMMessage> LoopMessages = new List<ELMMessage>
        {
            new RPM(),
            new Speed(),
            new MAF(),
            new EngineLoad()
        };

        readonly List<ELMMessage> initializationSequence = new List<ELMMessage>
        {
            new ATSP00(),
            new ATL0(),
            new ATST00(),
            new ATSP00()
        };

        public ELMMessage LastMessage { get; private set; }

        public override void InjectMessage(string message)
        {
            if (LastMessage == null) return; // Actually, this should not happen. This means a response to an unknown message has arrived
            LastMessage.IsSending = false;
            if (LastMessage.ProcessResponse(message))
            {
                if (LastMessage is RPM rpm) RPM = rpm.Result;
                else if (LastMessage is Speed speed) Speed = speed.Result;
                else if (LastMessage is MAF maf) MAF = maf.Result;
                else if (LastMessage is EngineLoad load) EngineLoad = load.Result;
                OnResponseToMessage?.Invoke(this, LastMessage);
            }
            OnUpdate?.Invoke(this, null);
            base.InjectMessage(message);
        }

        public async Task InitializeAsync()
        {
            await RunSequenceAsync(initializationSequence, TimeSpan.FromMilliseconds(500));
        }

        public async Task UpdateAsync()
        {
            await RunSequenceAsync(LoopMessages, TimeSpan.Zero);
        }

        public async Task RunSequenceAsync(IEnumerable<ELMMessage> messages, TimeSpan delay)
        {
            foreach (var item in messages)
            {
                await SendAsync(item);
                await Task.Delay(delay);
            }
        }

        public async Task SendAsync(ELMMessage message)
        {
            queue.Enqueue(message);
            message.IsSending = true;
            _ = Task.Run(ProcessQueueAsync);
            while (message.IsSending)
                await Task.Delay(10);
        }

        async Task ProcessQueueAsync()
        {
            if (isSending) return;
            isSending = true;
            try
            {
                while (queue.Count > 0)
                {
                    if (queue.TryDequeue(out var message))
                    {
                        LastMessage = message;
                        await WriteAsync(message.Message + "\r");
                        await WaitAndCrashIfFail(() => LastMessage != null && LastMessage.IsSending);
                    }
                    else break;
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                isSending = false;
            }
        }
    }
}

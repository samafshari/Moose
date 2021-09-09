using Moose.Models;

using MooseDrive.Messages;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using MooseDrive.Models;

namespace MooseDrive
{
    public class ELMDriver : Driver
    {
        public event EventHandler<ELMMessage> OnResponseToMessage;
        public event EventHandler OnUpdate;
        public event EventHandler<OBDResponse> OnResponse;

        volatile bool isSending = false;
        readonly ConcurrentQueue<ELMMessage> queue = new ConcurrentQueue<ELMMessage>();

        public int RPM { get; private set; }
        public int Speed { get; private set; }
        public int MAF { get; private set; }
        public int EngineLoad { get; private set; }

        public ConcurrentDictionary<string, string> RecentMessages { get; } = new ConcurrentDictionary<string, string>();

        readonly List<ELMMessage> handlers = new List<ELMMessage>();

        public readonly List<ELMMessage> LoopMessages = new List<ELMMessage>
        {
            new RPM(),
            new Speed(),
            new MAF(),
            new EngineLoad()
        };

        public List<string> CustomMessages = new List<string>();

        readonly List<ELMMessage> initializationSequence = new List<ELMMessage>
        {
            new ATE0(),
            new ATL0(),
            new ATSP00(),
            new _0100()
            //new ATST00(),
        };

        bool expectingEcho = false;

        StringBuilder messageBuffer = new StringBuilder();

        public override void InjectMessage(string message)
        {
            try
            {
                InjectMessage(message, true);
            }
            catch (Exception ex)
            {

            }
        }

        public void InjectMessage(string message, bool lookForEnd)
        {
            Log(message);
            if (message != null)
            {
                if (message[message.Length - 1] == '>' || !lookForEnd)
                {
                    message = messageBuffer.ToString() + message.Substring(0, message.Length - 1);
                    messageBuffer.Clear();
                }
                else
                {
                    messageBuffer.Append(message);
                    return;
                }
                message = message.Trim();
            }

            if (message.Contains("\r"))
            {
                foreach (var item in message.Split('\r'))
                {
                    InjectMessage(item, false);
                }
                return;
            }
            if (message.Contains("\n"))
            {
                foreach (var item in message.Split('\n'))
                {
                    InjectMessage(item, false);
                }
                return;
            }

            if (expectingEcho)
            {
                expectingEcho = false;
                return;
            }

            bool processed = false;
            var response = new OBDResponse
            {
                Response = message,
                Timestamp = DateTimeOffset.Now,
            };

            foreach (var handler in handlers.OrderByDescending(x => x.IsSending).ToList())
            {
                try
                {
                    if (handler.ProcessResponse(message))
                    {
                        handler.ValidateResponse(message);
                        handler.IsSending = false;
                        handler.LastInput = message;
                        if (handler.IsResponseValid)
                        {
                            if (handler is RPM rpm) RPM = rpm.Result;
                            else if (handler is Speed speed) Speed = speed.Result;
                            else if (handler is MAF maf) MAF = maf.Result;
                            else if (handler is EngineLoad load) EngineLoad = load.Result;
                        }
                        response.Code = handler.Message;
                        response.IsResponseValid = handler.IsResponseValid;
                        if (handler is ELMIntMessage elmInt) response.Value = elmInt.Result;
                        RecentMessages[response.Code] = response.Response;
                        OnResponseToMessage?.Invoke(this, handler);
                        processed = true;
                        break;
                    }
                }
                catch (Exception ex)
                {
                    handler.IsSending = false;
                }
            }

            if (!processed)
            {
                foreach (var handler in handlers.ToList())
                {
                    handler.IsSending = false;
                }
            }

            OnResponse?.Invoke(this, response);
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
            foreach (var item in CustomMessages)
            {
                try
                {
                    var msg = new ELMIntMessage(item);
                    await SendAsync(msg);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        public async Task RunSequenceAsync(IEnumerable<ELMMessage> messages, TimeSpan delay)
        {
            try
            {
                foreach (var item in messages)
                {
                    await SendAsync(item);
                    await Task.Delay(delay);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public async Task SendAsync(ELMMessage message)
        {
            queue.Enqueue(message);
            message.IsSending = true;
            _ = Task.Run(ProcessQueueAsync);
            await WaitAsync(() => message.IsSending, TimeSpan.FromSeconds(5));
            //while (message.IsSending) 
            //    await Task.Delay(10);
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
                        messageBuffer.Clear();
                        handlers.RemoveAll(x => x.GetType() == message.GetType());
                        handlers.Add(message);
                        await WriteAsync(message.Message + "\r");
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

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MooseDrive.Services
{
    public interface IErrorService
    {
        Task AlertErrorAsync(string message);
        void AlertError(string message);
        void Report(Exception ex);
        void ReportAndThrow(Exception ex);
    }
}

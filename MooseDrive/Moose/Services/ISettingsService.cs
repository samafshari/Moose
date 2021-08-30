using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Moose.Mobile.Services
{
    public interface ISettingsService<T> where T : class, new()
    {
        T Settings { get; }

        Task SaveAsync();
        void Save();
    }
}

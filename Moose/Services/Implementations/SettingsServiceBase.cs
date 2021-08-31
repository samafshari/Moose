using RedCorners.Components;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Moose.Mobile.Services.Implementations
{
    public abstract class SettingsServiceBase<T> : ISettingsService<T> where T : class, new()
    {
        volatile bool isSaving = false;
        ObjectStorage<T> settings = new ObjectStorage<T>();

        public T Settings => settings.Data;

        public async Task SaveAsync()
        {
            if (isSaving) return;

            isSaving = true;
            await Task.Run(() => Save());
            isSaving = false;
        }

        public void Save()
        {
            if (settings == null)
                settings = new ObjectStorage<T>();
            settings.Save();
        }
    }
}

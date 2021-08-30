using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MooseDrive.Models
{
    public class VisualException : Exception
    {
        public string Title { get; set; } = "Error";
        public string Button { get; set; } = "OK";

        public VisualException(string message) : base(message) { }

        public VisualException(string title, string message) : base(message)
        {
            Title = title;
        }

        public VisualException(string title, string message, string button) : base(message)
        {
            Title = title;
            Button = button;
        }

        public async Task DisplayAsync()
        {
            await App.Instance.DisplayAlertAsync(Title, Message, Button);
        }

        public void Display()
        {
            App.Instance.DisplayAlert(Title, Message, Button);
        }
    }
}

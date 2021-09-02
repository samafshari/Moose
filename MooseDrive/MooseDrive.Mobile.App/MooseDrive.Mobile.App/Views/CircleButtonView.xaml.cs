using RedCorners;
using RedCorners.Forms;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MooseDrive.Mobile.App.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CircleButtonView
    {
        public ImageSource Source
        {
            get => (ImageSource)GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        public static readonly BindableProperty SourceProperty = BindableProperty.Create(
                nameof(Source),
                typeof(ImageSource),
                typeof(CircleButtonView));

        public static readonly BindableProperty CommandProperty = BindableProperty.Create(
                nameof(Command),
                typeof(ICommand),
                typeof(CircleButtonView));

        public CircleButtonView()
        {
            InitializeComponent();
            grid.BindingContext = this;
        }
    }
}
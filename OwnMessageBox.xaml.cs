using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ComputerVisionMeter
{
    /// <summary>
    /// Логика взаимодействия для OwnMessageBox.xaml
    /// </summary>
    public partial class OwnMessageBox : Window
    {
        public OwnMessageBox()
        {
            InitializeComponent();
        }

        void AddButtons(MessageBoxButton buttons)
        {
            switch (buttons)
            {
                case MessageBoxButton.OK:
                    AddButton("OK", MessageBoxResult.OK, 75);
                    break;
                case MessageBoxButton.OKCancel:
                    AddButton("OK", MessageBoxResult.OK, 75);
                    AddButton("Cancel", MessageBoxResult.Cancel, 100, isCancel: true);
                    break;
                case MessageBoxButton.YesNo:
                    AddButton("Yes", MessageBoxResult.Yes, 75);
                    AddButton("No", MessageBoxResult.No, 75);
                    break;
                case MessageBoxButton.YesNoCancel:
                    AddButton("Yes", MessageBoxResult.Yes, 75);
                    AddButton("No", MessageBoxResult.No, 75);
                    AddButton("Cancel", MessageBoxResult.Cancel, 100, isCancel: true);
                    break;
                default:
                    throw new ArgumentException("Unknown button value", "buttons");
            }
        }

        void AddButton(string text, MessageBoxResult result, int width, bool isCancel = false)
        {
            var button = new Button() { Content = text, IsCancel = isCancel };
            button.Click += (o, args) => { Result = result; DialogResult = true; };
            button.Style = (Style)FindResource("ButtonStyle");
            button.Width = 75;
            ButtonContainer.Children.Add(button);
        }

        MessageBoxResult Result = MessageBoxResult.None;

        public static MessageBoxResult Show(string caption, string message,
                                            MessageBoxButton buttons)
        {
            var dialog = new OwnMessageBox() { Title = caption };
            dialog.MessageContainer.Text = message;
            dialog.AddButtons(buttons);
            dialog.ShowDialog();
            
            return dialog.Result;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
    }
}

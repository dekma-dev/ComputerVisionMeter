using System;
using System.Collections.Generic;
using System.Configuration;
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
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class AuthWindow : Window
    {
        private static string login = string.Empty;
        private static string password = string.Empty;
        public AuthWindow() { InitializeComponent(); LoginTextBox.Focus(); } 

        private void AuthButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                login = LoginTextBox.Text;
                password = hiddenPasswordTextBox.Password.ToString() != "" ? hiddenPasswordTextBox.Password.ToString() : passwordTextBox.Text;

                if (login == Properties.Settings.Default.Login && password == Properties.Settings.Default.Password)
                {
                    //Properties.Settings.Default.Save();
                    MainWindow main = new MainWindow();
                    main.Show();
                    this.Close();
                }
                else throw new Exception("Неверный логин или пароль");
            }
            catch (Exception error) 
            {
                OwnMessageBox.Show("Ошибка!", error.Message, MessageBoxButton.OK);
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (checkBox.IsChecked == true)
            {
                hiddenPasswordTextBox.Visibility = Visibility.Collapsed;
                passwordTextBox.Visibility = Visibility.Visible;
            }
            else
            {
                hiddenPasswordTextBox.Visibility = Visibility.Visible;
                passwordTextBox.Visibility = Visibility.Collapsed;
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void passwordTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (hiddenPasswordTextBox.Visibility == Visibility.Collapsed)  hiddenPasswordTextBox.Password = passwordTextBox.Text;
        }

        private void hiddenPasswordTextBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (passwordTextBox.Visibility == Visibility.Collapsed) passwordTextBox.Text = hiddenPasswordTextBox.Password.ToString();
        }

        private void CloseWindowButton_Click(object sender, RoutedEventArgs e) => this.Close();
    }
}

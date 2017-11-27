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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using System.Net.Sockets;
using System.Windows.Threading;
using System.Threading;
using System.ComponentModel;
using System.IO;
using System.Media;


namespace chat
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static IPAddress address;
        private static IPEndPoint endpoint;
        private Socket sockclient;
        private List<string> names;
        private string yname, s, sname;
        private int nbyte;
        private byte[] bts;
        private BackgroundWorker backgroundworker1;
        private bool DoWork;
        private SoundPlayer sp;

        public MainWindow()
        {
            InitializeComponent();
            panel2.Visibility = System.Windows.Visibility.Hidden;
            backgroundworker1 = (BackgroundWorker)this.FindResource("backgroundworker1");
            textbox21.IsEnabled = false;
            window1.
            names = new List<string>();
            address = IPAddress.Parse("127.0.0.1");
            endpoint = new IPEndPoint(address, 21322);
            bts = new byte[220];
            DoWork = false;
            sp = new SoundPlayer();
            
            sp.SoundLocation = "home15.wav";
            sp.Load();
        }

        private void button11_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (textbox11.Text == "" || textbox11.Text.Contains('*') || textbox11.Text.Contains('#') || textbox11.Text.Contains(':')
                    || textbox11.Text.Contains('/') || textbox11.Text.Contains('&') || textbox11.Text.Contains(';') || textbox11.Text.Contains("\\"))
                {
                    MessageBox.Show("Имя введено не верно. Введите заного.");
                }
                else
                {

                    sockclient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                    sockclient.Connect(endpoint);

                    
                    p_Identificator id = new p_Identificator(textbox11.Text, textbox12.Text);
                    sockclient.Send(id.MakePocket());

                    yname = textbox11.Text;
                    this.Title = "Ваш ник:" + yname;
                    Directory.CreateDirectory(yname);

                    panel1.Visibility = System.Windows.Visibility.Hidden;
                    panel2.Visibility = System.Windows.Visibility.Visible;

                    DoWork = true;
                    backgroundworker1.RunWorkerAsync();
                }
            }
            catch (SocketException)
            {
                MessageBox.Show("Сервер не активен. Попробуйте позже", "Сервер не активен!", MessageBoxButton.OK, MessageBoxImage.Stop);
            }
        }

        private void ListBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            label21.Content = "Чат с " + listbox1.SelectedValue;
            textbox21.IsEnabled = true;
            richtextbox21.IsEnabled = true;
            richtextbox21.Document.Blocks.Clear();
            if (listbox1.SelectedItem == null)
                textbox21.IsEnabled = false;

            if (File.Exists(yname + "\\" + listbox1.SelectedValue + ".txt"))
            {
                StreamReader sr = new StreamReader(yname + "\\" + listbox1.SelectedValue + ".txt");

                sr.ReadLine();

                while ((s = sr.ReadLine()) != null)
                {
                    string s1 = s.Substring(0, s.IndexOf(":"));
                    string s2 = s.Remove(0, s.IndexOf(":") + 1);

                    TextRange tr1 = new TextRange(richtextbox21.Document.ContentEnd, richtextbox21.Document.ContentEnd);
                    tr1.Text = Environment.NewLine + s1 + ": ";
                    if (s1 == "You")
                    {
                        tr1.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Blue);
                    }
                    else
                    {
                        tr1.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Green);
                    }

                    TextRange tr2 = new TextRange(richtextbox21.Document.ContentEnd, richtextbox21.Document.ContentEnd);
                    tr2.Text = s2;
                    tr2.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Black);
                }
                sr.Close();
            }
        }

        private void button21_Click(object sender, RoutedEventArgs e)
        {
            if (textbox21.Text != "")
            {
                try
                {
                    p_Message sMessage = new p_Message(listbox1.SelectedValue, textbox21.Text);
                    sockclient.Send(sMessage.MakePocket());
                }
                catch (SocketException)
                {
                    MessageBox.Show("Cервер отключился. Пожалуйста дождитесь его переподключения", "Cервер Отключился", MessageBoxButton.OK, MessageBoxImage.Error);

                    panel1.Visibility = System.Windows.Visibility.Visible;
                    panel2.Visibility = System.Windows.Visibility.Hidden;

                    sockclient.Disconnect(true);

                    DoWork = false;

                }

                TextRange tr1 = new TextRange(richtextbox21.Document.ContentEnd, richtextbox21.Document.ContentEnd);
                tr1.Text = Environment.NewLine + "You: ";
                tr1.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Blue);

                TextRange tr2 = new TextRange(richtextbox21.Document.ContentEnd, richtextbox21.Document.ContentEnd);
                tr2.Text = textbox21.Text;
                tr2.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Black);

                File.AppendAllText(yname + "\\" + Convert.ToString(listbox1.SelectedValue) + ".txt", Environment.NewLine + "You: " + textbox21.Text);

                richtextbox21.ScrollToEnd();
                textbox21.Text = "";
            }
        }

        private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            while (DoWork == true)
            {
                try
                {
                    nbyte = sockclient.Receive(bts);
                }
                catch (SocketException)
                {
                    continue;
                }
                if (nbyte != 0)
                {
                    switch (int.Parse(ASCIIEncoding.GetEncoding(1251).GetString(bts, 0, 1)))
                    {
                        case 2:
                            {
                                p_Message rMessage = new p_Message(bts);
                                s = ASCIIEncoding.GetEncoding(1251).GetString(bts, 0, nbyte);
                                sname = rMessage.sender;
                                s = rMessage.message;

                                File.AppendAllText(yname + "\\" + sname + ".txt", Environment.NewLine + sname + ": " + s);

                                backgroundworker1.ReportProgress(1);
                                nbyte = 0;
                                break;
                            }
                        case 3:
                            {
                                p_UserList rUserList = new p_UserList(bts);
                                names.Clear();
                                names.AddRange(rUserList.names);
                                backgroundworker1.ReportProgress(3);
                                nbyte = 0;
                                break;
                            }
                    }
                }
                Thread.Sleep(10);
            }
        }

        private void BackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage == 1)
            {
                TextRange tr1 = new TextRange(richtextbox21.Document.ContentEnd, richtextbox21.Document.ContentEnd);
                tr1.Text = Environment.NewLine + sname + ": ";
                tr1.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Green);

                TextRange tr2 = new TextRange(richtextbox21.Document.ContentEnd, richtextbox21.Document.ContentEnd);
                tr2.Text = s;
                tr2.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Black);

                richtextbox21.ScrollToEnd();

                sp.Play();
            }

            if (e.ProgressPercentage == 3)
            {
                listbox1.Items.Clear();
                foreach (string i in names)
                {
                    if (i != yname)
                    {
                        if (!listbox1.Items.Contains(i))
                        {
                            listbox1.Items.Add(i);
                        }
                    }
                }
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (Directory.Exists(yname))
            {
                Directory.Delete(yname, true);
            }
        }

        private void panel2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                button21_Click(sender, e);
                e.Handled = true;
            }
        }

        private void textbox21_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift) && e.Key == Key.Enter)
            {
                textbox21.Text += Environment.NewLine;
                textbox21.SelectionStart = textbox21.Text.Length;
                e.Handled = true;
            }
        }

        private void panel1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                button11_Click(sender, e);
                e.Handled = true;
            }
        }
    }
}

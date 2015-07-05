using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Net;
using System.Net.Security;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Threading;
using System.IO;
using System.Web;
using mshtml;

namespace PixivCrawler
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        WebBrowser PixivBrowser2 = new WebBrowser();
        CookieContainer cookie = new CookieContainer();
        //WebClient PixivBrowser = new WebClient();

        public MainWindow()
        {
            deletecookie();
            InitializeComponent();
            //PixivBrowser = new WebBrowser();
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(ValidateServerCertificate);
            Login.IsEnabled = false;
            //PixivBrowser.Visibility = System.Windows.Visibility.Visible;
            PixivBrowser.LoadCompleted += Login_Method;
            PixivBrowser.Navigate("https://www.secure.pixiv.net/login.php");
            Uri PixivLogin = new Uri("https://www.secure.pixiv.net/login.php");
            SearchButton.IsEnabled = false;
        }

        private void deletecookie()
        {
            string[] theCookies = System.IO.Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.Cookies));

            foreach (string currentFile in theCookies)
            {
                try
                {
                    System.IO.File.Delete(currentFile);
                }
                catch (Exception ex)
                {
                    //MessageBox.Show(ex.Message);
                }
            }
        }
        private void TextBoxPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(sender as TextBox, e.Text);
        }

        private static bool IsTextAllowed(TextBox textBox, string text)
        {
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex("[^0-9.-]+"); //regex that matches disallowed text
            return !regex.IsMatch(text);
        }

        private void Login_Method(object sender, NavigationEventArgs e)
        {
            Login.IsEnabled = true;
        }

        public static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return false;
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            String ID = IDTextBox.Text;
            String PW = PasswordTextBox.Password;

            var Doc = PixivBrowser.Document as HTMLDocument;

            var LoginElementCollection = Doc.getElementsByTagName("input");


            foreach (IHTMLElement LoginElement in LoginElementCollection)
            {
                int i = 0;
                if (LoginElement.id == "login_pixiv_id")
                {
                    LoginElement.setAttribute("value", ID);
                }
                else if(LoginElement.id=="login_password")
                {
                    LoginElement.setAttribute("value", PW);
                    //break;
                }
                else if(LoginElement.getAttribute("name")=="skip")
                {
                    //LoginElement.setAttribute("checked", "unchecked");
                    LoginElement.click();
                    //MessageBox.Show(LoginElement.outerHTML);
                }
            }
            LoginElementCollection = Doc.getElementsByTagName("button");
            foreach (IHTMLElement LoginElement in LoginElementCollection)
            {
                if (LoginElement.id == "login_submit") LoginElement.click();
            }
            PixivBrowser.LoadCompleted += CheckLogin;
        }

        private void CheckLogin(object sender,NavigationEventArgs e)
        {
            if (PixivBrowser.Source == new Uri("https://www.secure.pixiv.net/login.php")) MessageBox.Show("Failed!");
            else
            {
                Login.IsEnabled = false;
                SearchButton.IsEnabled = true;
                PixivBrowser.LoadCompleted -= Login_Method;
                PixivBrowser.LoadCompleted -= CheckLogin;
                cookie = GetCookieContainer();
                MessageBox.Show("Success!");
            }
        }

        public void ImagePageNumberTextChange(int n)
        {
            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate()
            {
                SearchingNowTextPageNumber.Content = n.ToString() + "번째 페이지 탐방중";
            });
            this.Dispatcher.Invoke(
   (ThreadStart)(() => { }), DispatcherPriority.ApplicationIdle);
        }

        public void ImageCountNumberTextChange(int n)
        {
            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate()
            {
                    SearchingNowTextImageCount.Content = n.ToString() + "개 이미지 수집중";
                  
             });
            this.Dispatcher.Invoke(
       (ThreadStart)(() => { }), DispatcherPriority.ApplicationIdle);
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!((Int32.Parse(MainThread_Number.Text)) > 0 && (Int32.Parse(MainThread_Number.Text)) <= 10))
                {
                    MessageBox.Show("효율은 1부터 10 사이의 숫자여야합니다.");
                    return;
                }
            }
            catch(Exception ex)
            {
                return;
            }
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate{
                SearchButton.IsEnabled = false;
            }));
            this.Dispatcher.Invoke(
       (ThreadStart)(() => { }), DispatcherPriority.ApplicationIdle);
            //ImageList ImageClass = new ImageList(PixivBrowser, TagTextBox.Text,cookie);
            CollectImage ImageClass;
            try
            {
                if (RadioButton1.IsChecked == true) ImageClass = new CollectImage(Int32.Parse(MainThread_Number.Text), this, Int32.Parse(StartPageTextBox.Text), Int32.Parse(EndPageTextBox.Text), Int32.Parse(BookMarkNumberTextBox.Text)
                       , TagTextBox.Text, cookie);
                else ImageClass = new CollectImage(Int32.Parse(MainThread_Number.Text),this, Int32.Parse(ImageNumber.Text), Int32.Parse(BookMarkNumberTextBox.Text), TagTextBox.Text, cookie);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        public void SearchButtonMakeEnable()
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                SearchButton.IsEnabled = true;
                SearchingNowTextPageNumber.Content = "";
                SearchingNowTextImageCount.Content = "";
                MessageBox.Show("Complete!");
            }));
            this.Dispatcher.Invoke(
       (ThreadStart)(() => { }), DispatcherPriority.ApplicationIdle);
        }

        public void GetCookieLoad(object sender,NavigationEventArgs e)
        {
            cookie = GetCookieContainer();
            PixivBrowser.LoadCompleted -= GetCookieLoad;
            MessageBox.Show("Success!");
        }

        public CookieContainer GetCookieContainer()
        {
            CookieContainer container = new CookieContainer();

            var Doc = PixivBrowser.Document as HTMLDocument;

            foreach (string cookie in Doc.cookie.Split(';'))
            {
                string name = cookie.Split('=')[0];
                string value = cookie.Substring(name.Length + 1);
                string path = "/";
                string domain;
                if (name.Equals("login_ever") || name.Equals("GCSCU_89993436389_H3")) domain = ".www.pixiv.net"; //change to your domain name
                else domain = ".pixiv.net"; //change to your domain name
                container.Add(new Cookie(name.Trim(), value.Trim(), path, domain));
            }

            return container;
        }
    }
}

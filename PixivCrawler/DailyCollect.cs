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
    class DailyCollect
    {
        WebBrowser DailyBrowser = new WebBrowser();
        CookieContainer LoginCookie;
        String LoginCookieString;

        public DailyCollect(CookieContainer LoginCookie,String LoginCookieString)
        {
            this.LoginCookieString = LoginCookieString;
            DailyBrowser.Loaded += delegate
            {
                DailyBrowser.LoadCompleted += LoadComp;
                DailyBrowser.Navigate("http://www.pixiv.net/ranking.php?mode=daily");
            };
        }

        private void LoadComp(object sender,NavigationEventArgs e)
        {
            HTMLDocument DailyDoc = DailyBrowser.Document as HTMLDocument;

            DailyDoc.cookie = LoginCookieString;
        }

        public void Go()
        {
        }
    }
}

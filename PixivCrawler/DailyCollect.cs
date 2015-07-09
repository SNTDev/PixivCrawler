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
using System.Windows.Forms;
using mshtml;
using HtmlAgilityPack;
namespace PixivCrawler
{
    class DailyCollect
    {
        //WebBrowser DailyBrowser = new WebBrowser();
        CookieContainer LoginCookie;
        String LoginCookieString;
        System.Windows.Forms.WebBrowser DailyBrowser;

        public DailyCollect(CookieContainer LoginCookie,String LoginCookieString)
        {
            DailyBrowser = new System.Windows.Forms.WebBrowser();
            DailyBrowser.AllowNavigation = true;
            DailyBrowser.ScriptErrorsSuppressed = true;
            //DailyBrowser.BeginInit();
            this.LoginCookieString = LoginCookieString;
            //T.LoadCompleted += LoadComp;
            //DailyBrowser.Url = new Uri("http://www.pixiv.net/ranking.php?mode=daily");
            DailyBrowser.Navigate("https://www.secure.pixiv.net/login.php");
            DailyBrowser.Height = 1000;
            DailyBrowser.Width = 1000;
            DailyBrowser.DocumentCompleted += LoadComp;
        }

        private void LoadComp(object sender,EventArgs e)
        {
            DailyBrowser.Document.Cookie = LoginCookieString;
            DailyBrowser.DocumentCompleted -= LoadComp;
            DailyBrowser.DocumentCompleted += RefreshComp;
            DailyBrowser.Refresh();
        }

        private void RefreshComp(object sender,EventArgs e)
        {
            DailyBrowser.DocumentCompleted -= RefreshComp;
            Go();
        }

        public void Go()
        {
            //HtmlDocument Doc = DailyBrowser.Document;
            HTMLDocument msDoc = DailyBrowser.Document.DomDocument as HTMLDocument;
            int count = 0;

            /*foreach(HtmlElement Ele in Doc.All)
            {
                if(Ele.GetAttribute("className")=="ranking-item")
                {
                    count++;
                }
            }*/

            return;
        }
    }
}

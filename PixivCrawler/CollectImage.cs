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
using System.Diagnostics;
using System.IO;
using System.Web;
using mshtml;

namespace PixivCrawler
{
    /// <summary>
    /// ImageList.xaml에 대한 상호 작용 논리
    /// </summary>
    public class CollectImage
    {
        Image[,] SearchedItem = new Image[4, 5];
        WebBrowser PixivBrowser;
        String URL, Tag;
        CookieContainer cookie;
        String FolderPath;
        int Count = 0;
        int MaxCount = 0;
        int PageNumber = 1;
        int BookMark = 0;
        int MainThreadCountNumber = 0;
        int MainThreadCounter = 0;
        bool StopSw=false;
        public CollectImage(int MainThreadCountNumber,MainWindow Main,int StartPage,int EndPage,int BookMark,String Tag, CookieContainer cookie,String Path)
        {
            this.Tag = Tag;
            this.cookie = cookie;
            this.BookMark = BookMark;
            this.MainThreadCountNumber = MainThreadCountNumber;
            FolderPath = Path + System.DateTime.Now.ToString("yyyy-MM-dd") + "-" + Tag + "-" + "북마크 " + BookMark + "이상";
            //FolderPath = FolderPath;
            MaxCount = -1;

            if (!Directory.Exists(FolderPath)) Directory.CreateDirectory(FolderPath);

            for(int i=StartPage;i<=EndPage;i++)
            {
                while(MainThreadCounter>=MainThreadCountNumber)
                {

                }
                Main.ImagePageNumberTextChange(i);
                Main.ImageCountNumberTextChange(Count);
                MainThreadCounter++;
                Thread Th = new Thread(new ParameterizedThreadStart(Start_Page_Thread));
                Th.Start(i);
                if (StopSw) break;
            }
            Main.SearchButtonMakeEnable();
        }

        public void Stop_Collect()
        {
            StopSw=true;
        }

        public CollectImage(int MainThreadCountNumber,MainWindow Main, int ImageNumber, int BookMark, String Tag, CookieContainer cookie,String Path)
        {
            this.Tag = Tag;
            this.cookie = cookie;
            this.BookMark = BookMark;
            FolderPath = Path + System.DateTime.Now.ToString("yyyy-MM-dd") + "-" + Tag + "-" + "북마크 " + BookMark + "이상";
            FolderPath = FolderPath;
            MaxCount = ImageNumber;

            if (!Directory.Exists(FolderPath)) Directory.CreateDirectory(FolderPath);

            for (int i = 1;; i++)
            {
                while (MainThreadCounter >= MainThreadCountNumber)
                {

                }
                Main.ImagePageNumberTextChange(i);
                Main.ImageCountNumberTextChange(Count);
                MainThreadCounter++;
                Thread Th = new Thread(new ParameterizedThreadStart(Start_Page_Thread));
                Th.Start(i);
                if (MaxCount > 0 && Count >= MaxCount) break;
                if (StopSw) break;
            }

            Main.SearchButtonMakeEnable();
        }

        private void Start_Page_Thread(object O)
        {
            int PageN = (int)O;
            try
            {
                Get_SearchList(PageN);
                MainThreadCounter--;
            }
            catch(Exception ex)
            {
                MainThreadCounter--;
            }
        }
            
        private void __Get_Image(int Count)
        {
            var Doc = PixivBrowser.Document as HTMLDocument;
            var ElementCollection = Doc.getElementsByTagName("img");
            foreach (IHTMLElement Element in ElementCollection)
            {
                if (Element.parentElement.className == "_layout-thumbnail ui-modal-trigger")
                {
                    var Img = new BitmapImage();
                    Img.BeginInit();
                    Img.UriSource = new Uri(Element.getAttribute("src"), UriKind.RelativeOrAbsolute);
                    Img.CacheOption = BitmapCacheOption.OnLoad;
                    Img.EndInit();
                    SearchedItem[Count / 5, Count % 5].Source = Img;
                }
            }
        }

        private void _Get_Image_Manga(String URL,int count,String Title)
        {
            try
            {
                HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(URL);
                string session = cookie.GetCookies(new Uri("http://www.pixiv.net"))["PHPSESSID"].Value;
                //Cookie C = new Cookie("PHPSESSID", session, "/", ".pixiv.net");
                myRequest.CookieContainer = cookie;
                //myRequest.Method = "POST";
                //myRequest.UserAgent = "Mozilla/5.0 (Windows; U; Windows NT 6.0; zh-CN; rv:1.9.0.6) Gecko/2009011913 Firefox/3.0.6";
                //myRequest.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
                /*myRequest.Headers.Add("Accept-Language", "zh-cn,zh;q=0.7,ja;q=0.3");
                myRequest.Headers.Add("Accept-Encoding", "gzip,deflate");
                myRequest.Headers.Add("Accept-Charset", "gb18030,utf-8;q=0.7,*;q=0.7");
                myRequest.Referer = "http://www.pixiv.net/index.php";*/
                myRequest.Referer = "http://www.pixiv.net/index.php";

                HttpWebResponse myResponse = (HttpWebResponse)myRequest.GetResponse();

                StreamReader reader = new StreamReader(myResponse.GetResponseStream(), Encoding.UTF8);

                String content = reader.ReadToEnd();

                int StIdx = content.IndexOf("data-src=");
                int EdIdx;
                count = 0;
                while(StIdx>=0)
                {
                    StIdx = content.IndexOf("\"",StIdx);
                    EdIdx = content.IndexOf("\"", StIdx + 1);
                    StIdx++;
                    String Src = content.Substring(StIdx, EdIdx - StIdx);
                    myRequest = (HttpWebRequest)WebRequest.Create(Src);
                    myRequest.CookieContainer = cookie;
                    //myRequest.Referer = "http://www.pixiv.net/member_illust.php?mode=medium&illust_id=50284907";
                    //myRequest.UserAgent = "Mozilla/5.0 (Windows; U; Windows NT 6.0; zh-CN; rv:1.9.0.6) Gecko/2009011913 Firefox/3.0.6";
                    //myRequest.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
                    //myRequest.Headers.Add("Accept-Encoding", "gzip,deflate");
                    //myRequest.Headers.Add("Accept-Charset", "gb18030,utf-8;q=0.7,*;q=0.7");
                    myRequest.Referer = "http://www.pixiv.net/index.php";


                    myResponse = (HttpWebResponse)myRequest.GetResponse();
                    using (var ImageS = myResponse.GetResponseStream())
                    {
                        byte[] Buffer = new byte[32768];
                        int read = 0;


                        int chunk;
                        while ((chunk = ImageS.Read(Buffer, read, Buffer.Length - read)) > 0)
                        {
                            read += chunk;
                            if (read == Buffer.Length)
                            {
                                int nextByte = ImageS.ReadByte();


                                // End of stream? If so, we're done
                                if (nextByte == -1)
                                {
                                    break;
                                }


                                // Nope. Resize the buffer, put in the byte we've just
                                // read, and continue
                                byte[] newBuffer = new byte[Buffer.Length * 2];
                                Array.Copy(Buffer, newBuffer, Buffer.Length);
                                newBuffer[read] = (byte)nextByte;
                                Buffer = newBuffer;
                                read++;
                            }
                        }
                        //Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                        {
                            using (MemoryStream ImageMemoryS = new MemoryStream(Buffer))
                            {
                                //var Img = new BitmapImage();
                                FileStream FileS = new FileStream(FolderPath + "/" + Title + " " + ++count + ".jpg", FileMode.Create);
                                FileS.Write(Buffer, 0, Buffer.Length);
                                //Img.UriSource = new Uri((Src), UriKind.RelativeOrAbsolute);
                                /*Img.BeginInit();
                                Img.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                                Img.CacheOption = BitmapCacheOption.OnLoad;
                                Img.UriSource = null;
                                Img.StreamSource = ImageMemoryS;
                                Img.EndInit();
                                if (count < 20) SearchedItem[count / 5, count % 5].Source = Img;*/

                                //this.Dispatcher.Invoke(
                                //(ThreadStart)(() => { }), DispatcherPriority.ApplicationIdle);
                            }
                        }

                    }
                    StIdx = content.IndexOf("data-src=", StIdx);
                }
            }
            catch(Exception ex)
            {
                //MessageBox.Show(ex.ToString());
            }
        }

        private void _Get_Image(object O)
        {
            String URL = ((Tuple<String, int,String>)O).Item1;
            int count = ((Tuple<String, int,String>)O).Item2;
            String Title = ((Tuple<String, int, String>)O).Item3;
            try
            {
                HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create("http://www.pixiv.net" + URL);
                string session = cookie.GetCookies(new Uri("http://www.pixiv.net"))["PHPSESSID"].Value;
                //Cookie C = new Cookie("PHPSESSID", session, "/", ".pixiv.net");
                myRequest.CookieContainer = cookie;
                //myRequest.Method = "POST";
                //myRequest.UserAgent = "Mozilla/5.0 (Windows; U; Windows NT 6.0; zh-CN; rv:1.9.0.6) Gecko/2009011913 Firefox/3.0.6";
                //myRequest.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
                /*myRequest.Headers.Add("Accept-Language", "zh-cn,zh;q=0.7,ja;q=0.3");
                myRequest.Headers.Add("Accept-Encoding", "gzip,deflate");
                myRequest.Headers.Add("Accept-Charset", "gb18030,utf-8;q=0.7,*;q=0.7");
                myRequest.Referer = "http://www.pixiv.net/index.php";*/
                myRequest.Referer = "http://www.pixiv.net/index.php";

                HttpWebResponse myResponse = (HttpWebResponse)myRequest.GetResponse();

                StreamReader reader = new StreamReader(myResponse.GetResponseStream(), Encoding.UTF8);

                String content = reader.ReadToEnd();

                int S_value = content.IndexOf("<div class=\"wrapper\"><span class=\"close ui-modal-close\"><i class=\"_icon-12 size-2x _icon-close\">");

                if (S_value < 0)
                {
                    int StIdx = content.IndexOf("<div class=\"works_display\">");
                    StIdx=content.IndexOf("a href=",StIdx);
                    StIdx = content.IndexOf("\"", StIdx);
                    int EdIdx = content.IndexOf("\"", StIdx + 1);
                    StIdx++;
                    URL = "http://www.pixiv.net/" + content.Substring(StIdx, EdIdx - StIdx);
                    _Get_Image_Manga(URL,count,Title);
                    return;
                }
                else
                {

                    int StIdx = content.IndexOf("data-src=\"", S_value);
                    int EdIdx = content.IndexOf("\"", StIdx + 10);

                    String Src = content.Substring(StIdx + 10, EdIdx - StIdx - 10);
                    foreach (Cookie Cook in myResponse.Cookies)
                    {
                        myRequest.CookieContainer.Add(Cook);
                    }

                    myRequest = (HttpWebRequest)WebRequest.Create(Src);
                    myRequest.CookieContainer = cookie;
                    //myRequest.Referer = "http://www.pixiv.net/member_illust.php?mode=medium&illust_id=50284907";
                    //myRequest.UserAgent = "Mozilla/5.0 (Windows; U; Windows NT 6.0; zh-CN; rv:1.9.0.6) Gecko/2009011913 Firefox/3.0.6";
                    //myRequest.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
                    //myRequest.Headers.Add("Accept-Encoding", "gzip,deflate");
                    //myRequest.Headers.Add("Accept-Charset", "gb18030,utf-8;q=0.7,*;q=0.7");
                    myRequest.Referer = "http://www.pixiv.net/index.php";
                    myResponse = (HttpWebResponse)myRequest.GetResponse();
                    using (var ImageS = myResponse.GetResponseStream())
                    {
                        byte[] Buffer = new byte[32768];
                        int read = 0;


                        int chunk;
                        while ((chunk = ImageS.Read(Buffer, read, Buffer.Length - read)) > 0)
                        {
                            read += chunk;


                            // If we've reached the end of our buffer, check to see if there's
                            // any more information
                            if (read == Buffer.Length)
                            {
                                int nextByte = ImageS.ReadByte();


                                // End of stream? If so, we're done
                                if (nextByte == -1)
                                {
                                    break;
                                }


                                // Nope. Resize the buffer, put in the byte we've just
                                // read, and continue
                                byte[] newBuffer = new byte[Buffer.Length * 2];
                                Array.Copy(Buffer, newBuffer, Buffer.Length);
                                newBuffer[read] = (byte)nextByte;
                                Buffer = newBuffer;
                                read++;
                            }
                        }
                        //Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                        {
                            using (MemoryStream ImageMemoryS = new MemoryStream(Buffer))
                            {
                                //var Img = new BitmapImage();
                                FileStream FileS = new FileStream(FolderPath + "/" + Title + ".jpg", FileMode.Create);
                                FileS.Write(Buffer, 0, Buffer.Length);
                                //Img.UriSource = new Uri((Src), UriKind.RelativeOrAbsolute);
                                /*Img.BeginInit();
                                Img.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                                Img.CacheOption = BitmapCacheOption.OnLoad;
                                Img.UriSource = null;
                                Img.StreamSource = ImageMemoryS;
                                Img.EndInit();
                                if (count < 20) SearchedItem[count / 5, count % 5].Source = Img;*/

                                //this.Dispatcher.Invoke(
                                //(ThreadStart)(() => { }), DispatcherPriority.ApplicationIdle);
                            }
                        }

                    }
                }

            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());
                //MessageBox.Show(ex.Status.ToString());
            }
        }

        private void Get_SearchList(int PageNumber)
        {
            int ImageCount=0;
            URL = "http://www.pixiv.net/search.php?word=" + Tag + "&order=date_d&p=" + PageNumber.ToString();
            //URL = "http://www.pixiv.net/member_illust.php?mode=medium&illust_id=50311630";
            //URL = "http://www.pixiv.net/member_illust.php?mode=medium&illust_id=50311108";
            //URL = "http://i3.pixiv.net/img-original/img/2015/05/10/16/15/31/50306658_p0.jpg";
            //URL = "http://i1.pixiv.net/img-original/img/2015/05/08/00/00/01/50259632_p0.jpg";
            //PixivBrowser.LoadCompleted += _Get_SearchList;

            HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(URL);
            myRequest.CookieContainer = cookie;
            myRequest.Referer = "http://www.pixiv.net/index.php";
            HttpWebResponse myResponse = (HttpWebResponse)myRequest.GetResponse();
            StreamReader reader = new StreamReader(myResponse.GetResponseStream(), Encoding.UTF8);
            String content = reader.ReadToEnd();

            int StIdx = content.IndexOf("<li class=\"image-item \"");
            int EdIdx;
            while (StIdx > 0)
            {
                StIdx = content.IndexOf("<a href=\"", StIdx);
                if (StIdx < 0) break;
                StIdx = content.IndexOf("\"", StIdx);
                EdIdx = content.IndexOf("\"", StIdx + 1);
                StIdx++;
                String SubURL = content.Substring(StIdx, EdIdx - StIdx);
                StIdx = content.IndexOf("title=\"", StIdx);
                StIdx = content.IndexOf("\"", StIdx);
                EdIdx = content.IndexOf("\"", StIdx + 1);
                StIdx++;
                String Title = content.Substring(StIdx, EdIdx - StIdx);
                int StIdx1 = content.IndexOf("<li class=\"image-item \"", StIdx + 1);
                int BkStIdx = content.IndexOf("data-tooltip=\"",StIdx);
                BkStIdx = content.IndexOf("\"",BkStIdx);
                int BkEdIdx = content.IndexOf("件의", BkStIdx);
                int BkN=0;
                BkStIdx++;
                if (BkEdIdx < 0 || BkEdIdx > StIdx1) BkN = 0;
                else BkN = Int32.Parse(content.Substring(BkStIdx, BkEdIdx - BkStIdx).Replace(",",""));
                if (BkN > BookMark)
                {
                    Thread T = new Thread(new ParameterizedThreadStart(_Get_Image));
                    T.Start(Tuple.Create(SubURL, Count, Title));
                    Count++;
                }
                ImageCount++;
                if (MaxCount > 0 && Count >= MaxCount) break;
                StIdx = content.IndexOf("<li class=\"image-item \">", StIdx + 1);
            }
            if (ImageCount < 18) Stop_Collect();
        }

        private void _Get_SearchList(object sender, NavigationEventArgs e)
        {
            var Doc = PixivBrowser.Document as HTMLDocument;
            var ElementCollection = Doc.getElementsByTagName("li");
            PixivBrowser.LoadCompleted -= _Get_SearchList;
            foreach (IHTMLElement Element in ElementCollection)
            {
                //var ChildrenCollection = Element.children as HTMLElementCollection;

                if (Element.className != "image-item ") continue;
                dynamic ChildrenCollection = Element.children;

                foreach (IHTMLElement ChildrenElement in ChildrenCollection)
                {
                    if (ChildrenElement.className == "work  _work ")
                    {
                        //PixivBrowser.Navigate(ChildrenElement.getAttribute("href"));
                        //Thread T = new Thread(new ParameterizedThreadStart(Get_Image));
                        //T.Start(ChildrenElement.getAttribute("href"));
                        break;
                    }
                }
                break;
            }
        }

    }
}

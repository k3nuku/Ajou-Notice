/*
 *                  AJOU NOTICE - 1.0 - Preliminary Version
 * 
 * Module Name      : Web Access Core Module (Code/Common/WebCore.cs)
 * Author           : k3nuku
 * Created at       : 2014-09-07
 * Last Modified    : 2014-09-20
 * 
 * Current Status   : Develop-Completed [200]
 * 
 */

using System;
using System.Text;
using System.Net;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace Ajou_Notice.Code.Common
{
    class WebCore
    {
        private static string userAgent = "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; Trident/6.0)";
        private static string etcHeader_1 = "Accept-Language: ko";
        private static string etcHeader_2 = "DNT: 1";
        private static string etcHeader_3 = "Pragma: no-cache";
        private static string etcHeader_4 = "Accept-Encoding: gzip, deflate";
        private static string acceptType = "text/html, application/xhtml+xml, */*";
        private static string contentType = "application/x-www-form-urlencoded";
        private static string eClassURL = "https://eclass.ajou.ac.kr/";
        private static string b64sha1APIURL = "http://api.sokdak.kr/aimslogin_misc/";
        
        /// <summary>
        /// URL형식으로 Encoding 합니다.
        /// </summary>
        /// <param name="arg0">Encoding할 string</param>
        /// <returns>Encoding된 string</returns>
        public static string UrlEncoder(string arg0)
        {
            return System.Web.HttpUtility.UrlEncode(arg0);
        }

        /// <summary>
        /// URL형식으로 된 string을 원본 string으로 Decoding 합니다.
        /// </summary>
        /// <param name="arg0">Decode할 String</param>
        /// <returns>Decoded String</returns>
        public static string UrlDecoder(string arg0)
        {
            return System.Web.HttpUtility.UrlDecode(arg0);
        }

        /// <summary>
        /// 변수로 입력받은 CookieContainer의 Cookie개수를 반환합니다.
        /// </summary>
        /// <param name="cookieContainer">CookieContainer</param>
        /// <returns>쿠키 개수</returns>
        public static int CookieCounter(object cookieContainer)
        {
            return ((CookieContainer)cookieContainer).Count;
        }

        /// <summary>
        /// 해당 Site에 Web Access를 시도합니다.
        /// </summary>
        /// <param name="URL">Site URL</param>
        /// <param name="Method">Method</param>
        /// <param name="CookieCont">CookieContainer</param>
        /// <param name="referer">Referrer</param>
        /// <param name="postBody">POST Body</param>
        /// <returns>object Array[2]{ ResponseData as String, Cookies in CookieContainer }</returns>
        public static object[] SiteAccess(string URL, string Method = null, CookieContainer CookieCont = null, string referer = null, string postBody = null)
        {
            if (CookieCont == null)
                CookieCont = new CookieContainer();

            HttpWebRequest wReq = null;

            try
            {
                wReq = (HttpWebRequest)HttpWebRequest.Create(URL);
            }
            catch (UriFormatException)
            {
                return new object[2] { "잘못된 URL 형식 입니다.", null };
            }

            wReq.UserAgent = userAgent;
            wReq.Headers.Add(etcHeader_1);
            wReq.Headers.Add(etcHeader_2);
            wReq.Headers.Add(etcHeader_3);
            wReq.Accept = acceptType;
            wReq.CookieContainer = CookieCont;
            wReq.KeepAlive = true;

            if (referer != null)
                wReq.Referer = referer;

            if (Method != "POST" || Method == null)
                wReq.Method = "GET";
            else
            {
                wReq.Method = "POST";
                wReq.ContentType = contentType;

                if(postBody != null)
                {
                    byte[] postBody_barr = Encoding.UTF8.GetBytes(postBody);
                    wReq.ContentLength = postBody_barr.Length;

                    using(Stream wReqStream = wReq.GetRequestStream())
                    {
                        wReqStream.Write(postBody_barr, 0, postBody_barr.Length);
                    }
                }
                else
                {
                    wReq.ContentLength = 0;
                }
            }

            HttpWebResponse wRes = null;

            try
            {
                wRes = (HttpWebResponse)wReq.GetResponse();

                if (URL.StartsWith(String.Format("{0}eclass/x/login_post_proc.jsp", eClassURL))) // eClass 로그인 시 root도메인에 JSESSIONID 강제 설정
                {
                    char[] sep = { ';', ',' };
                    string SessionID = "";
                    String[] headers = wRes.Headers["Set-Cookie"].Split(sep);
                    for (int i = 0; i <= headers.Length - 1; i++)
                    {
                        if (headers[i].StartsWith("JSESSIONID"))
                        {
                            sep[0] = '=';
                            SessionID = headers[i].Split(sep)[1];
                            break;
                        }
                    }

                    CookieCont.Add(new Uri(eClassURL), new Cookie("JSESSIONID", SessionID));
                }
            }
            catch (WebException)
            {
                return new object[2] { "서버에 접근할 수 없습니다. 사이트 주소가 올바른지 혹은 인터넷 연결을 확인하세요.", null };
            }
            
            StreamReader wResSReader = new StreamReader(wRes.GetResponseStream());

            string siteResponse = wResSReader.ReadToEnd();

            return new object[2] { siteResponse, CookieCont };
        }

        /// <summary>
        /// Base64, SHA1로 해당 string을 암호화 합니다.
        /// </summary>
        /// <param name="pwd_tobe">암호화 할 string</param>
        /// <returns>암호화 된 string</returns>
        public static string GetEncryptedString_b64sha1(string pwd_tobe)
        {
            string pwd_encrypted = "";

            var th = new Thread(() =>
            {
                var br = new WebBrowser();
                br.DocumentCompleted += (object sender, WebBrowserDocumentCompletedEventArgs e) =>
                {
                    var wb1 = sender as WebBrowser;

                    if (wb1.Url == e.Url)
                    {
                        pwd_encrypted = wb1.Document.InvokeScript("b64_sha1", new string[1] { pwd_tobe }).ToString();
                        Application.ExitThread();
                    }
                };
                br.Navigate(b64sha1APIURL);
                Application.Run();
            });
            th.SetApartmentState(ApartmentState.STA);
            th.Start();

            while (true)
            {
                if (th.ThreadState == ThreadState.Stopped)
                    break;
                Thread.Sleep(300);
            }

            return pwd_encrypted;
        }
    }
}

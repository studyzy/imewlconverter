/*
 *   Copyright © 2009-2020 studyzy(深蓝,曾毅)

 *   This program "IME WL Converter(深蓝词库转换)" is free software: you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation, either version 3 of the License, or
 *   (at your option) any later version.

 *   This program is distributed in the hope that it will be useful,
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *   GNU General Public License for more details.

 *   You should have received a copy of the GNU General Public License
 *   along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System.IO;
using System.Net.Http;
using System.Text;

namespace Studyzy.IMEWLConverter.Helpers
{
    public static class HttpHelper
    {
        //#region --Html--
        public static string GetHtml(string url)
        {
            return GetHtml(url, Encoding.UTF8);
        }

        public static string GetHtml(string url, Encoding encoding)
        {
            var client = new HttpClient();
            var resp = client.GetStreamAsync(url).GetAwaiter().GetResult();
            return new StreamReader(resp, encoding).ReadToEnd();
        }

        //public string GetHtml(string URL, out string cookie)
        //{
        //    WebRequest wrt;
        //    wrt = WebRequest.Create(URL);
        //    wrt.Credentials = CredentialCache.DefaultCredentials;
        //    WebResponse wrp;

        //    wrp = wrt.GetResponse();

        //    string html = new StreamReader(wrp.GetResponseStream(), this.Encoding).ReadToEnd();
        //    cookie = wrp.Headers.Get("Set-Cookie");
        //    return html;
        //}
        //public string GetHtml(string url, CookieContainer cc)
        //{
        //    HttpWebRequest httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(url);
        //    httpWebRequest.Accept = "text/plain, */*";
        //    httpWebRequest.ContentType = "application/x-www-form-urlencoded";
        //    httpWebRequest.Referer = "https://dynamic.12306.cn/otsweb/order/querySingleAction.do?method=init";
        //    httpWebRequest.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; SV1; Maxthon; .NET CLR 1.1.4322)";
        //    httpWebRequest.Method = "GET";
        //    httpWebRequest.CookieContainer = cc;
        //    httpWebRequest.Headers.Add("x-requested-with","XMLHttpRequest");
        //    HttpWebResponse webResponse = (HttpWebResponse)httpWebRequest.GetResponse();

        //    string html = new StreamReader(webResponse.GetResponseStream(), this.Encoding).ReadToEnd();
        //    return html;
        //}
        //public string GetHtml(string url, string cookie)
        //{

        //    var cc = SetCookie(cookie);
        //    return GetHtml(url, cc);
        //}
        //private CookieContainer SetCookie(string cookie)
        //{
        //    CookieContainer cc = new CookieContainer();
        //    Cookie c = new Cookie();
        //    foreach (var s in cookie.Split(';'))
        //    {
        //        if (s != "")
        //        {
        //            cc.SetCookies(new Uri(this.HttpServer), s);
        //        }
        //    }
        //    return cc;
        //}

        //public string GetHtml(string URL, string postData, string cookie,string referer=null)
        //{
        //    byte[] byteRequest = Encoding.Default.GetBytes(postData);


        //    HttpWebRequest httpWebRequest;
        //    HttpWebResponse webResponse;
        //    if (referer == null)
        //    {
        //        referer = URL;
        //    }
        //    httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(URL);

        //    //httpWebRequest.ProtocolVersion = HttpVersion.Version10;
        //    httpWebRequest.CookieContainer  = SetCookie(cookie);;
        //    httpWebRequest.ServicePoint.Expect100Continue = false;
        //    httpWebRequest.ContentType = "application/x-www-form-urlencoded";
        //    httpWebRequest.Accept = "text/html, application/xhtml+xml, */*";
        //        //"text/html, application/xhtml+xml, image/gif, image/x-xbitmap, image/jpeg, image/pjpeg, application/x-shockwave-flash, application/vnd.ms-excel, application/vnd.ms-powerpoint, application/msword, */*";
        //    httpWebRequest.Referer = referer;
        //    httpWebRequest.UserAgent = "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Trident/5.0)";
        //    httpWebRequest.Method = "Post";
        //    httpWebRequest.ContentLength = byteRequest.Length;
        //    httpWebRequest.Headers.Set("DNT", "1");
        //    httpWebRequest.Headers.Set("Accept-Encoding", "gzip, deflate");
        //    httpWebRequest.Headers.Set("Accept-Language", "zh-CN");
        //    httpWebRequest.Headers.Set("Cache-Control", "no-cache");
        //    httpWebRequest.KeepAlive = true;

        //    httpWebRequest.AllowAutoRedirect = false;

        //    var sp = httpWebRequest.ServicePoint;
        //    var prop = sp.GetType().GetProperty("HttpBehaviour", BindingFlags.Instance | BindingFlags.NonPublic);
        //    prop.SetValue(sp, (byte)0, null);

        //    Stream stream;
        //    stream = httpWebRequest.GetRequestStream();
        //    stream.Write(byteRequest, 0, byteRequest.Length);
        //    stream.Close();
        //    webResponse = (HttpWebResponse)httpWebRequest.GetResponse();
        //    string html = new StreamReader(webResponse.GetResponseStream(), this.Encoding).ReadToEnd();
        //    return html;


        //}
        ////public  byte[] ReadFully(Stream stream)
        ////{
        ////    byte[] buffer = new byte[128];
        ////    using (MemoryStream ms = new MemoryStream())
        ////    {
        ////        while (true)
        ////        {
        ////            int read = stream.Read(buffer, 0, buffer.Length);
        ////            if (read <= 0)
        ////                return ms.ToArray();
        ////            ms.Write(buffer, 0, read);
        ////        }
        ////    }
        ////}

        //public Image GetImage(string URL, out string cookie)
        //{
        //    HttpWebRequest httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(URL);
        //    httpWebRequest.Accept = "*/*";

        //    httpWebRequest.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; SV1; Maxthon; .NET CLR 1.1.4322)";
        //    httpWebRequest.Method = "GET";
        //    HttpWebResponse webResponse = (HttpWebResponse)httpWebRequest.GetResponse();
        //    cookie = webResponse.Headers.Get("Set-Cookie");
        //    Stream getStream = webResponse.GetResponseStream();
        //    Image img = new Bitmap(getStream);

        //    getStream.Close();
        //    return img;
        //}

        //public Image GetImage(string URL, string cookie)
        //{
        //    HttpWebRequest httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(URL);
        //    httpWebRequest.Accept = "*/*";

        //    httpWebRequest.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; SV1; Maxthon; .NET CLR 1.1.4322)";
        //    httpWebRequest.Method = "GET";


        //    httpWebRequest.CookieContainer = SetCookie(cookie);

        //    HttpWebResponse webResponse = (HttpWebResponse)httpWebRequest.GetResponse();

        //    Stream getStream = webResponse.GetResponseStream();
        //    Image img = new Bitmap(getStream);

        //    getStream.Close();
        //    return img;
        //}
        //#endregion


        //public  string UrlEncode(string str)
        //{
        //    return HttpUtility.UrlEncode(str, this.Encoding);
        //}
    }
}

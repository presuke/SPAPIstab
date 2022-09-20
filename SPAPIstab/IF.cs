using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;

namespace SPAPIstab
{
    class IF
    {
        internal const String HOST_ADDRESS_LISENCE = "https://boy-reno-sedori.ssl-lolipop.jp";
        internal const String HOST_ADDRESS_APP = "https://main-iine-factory.ssl-lolipop.jp";
        internal const String HOST_ADDRESS_STEALTH = "https://renoneve.xsrv.jp";

        internal static void initializeProtocol()
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            }
            catch (Exception ex)
            {
                Log.outputError(ex);
            }
        }

        internal static String getECSetting()
        {
            String strRet = String.Empty;
            for (int intTryCnt = 0; intTryCnt < 5; intTryCnt++)
            {
                using (WebClient wc = new WebClient())
                {
                    try
                    {
                        strRet = wc.DownloadString(HOST_ADDRESS_STEALTH + "/stealth/ec_search.xml");
                        break;
                    }
                    catch (WebException wex)
                    {
                    }
                    catch (Exception ex)
                    {
                        Log.outputError(ex);
                        break;
                    }
                }
            }
            return strRet;
        }

        /// <summary>
        /// サーバにポストする
        /// </summary>
        /// <param name="postDataBytes"></param>
        internal static String post(String url, String postData, MethodBase methodBase, ref Exception error)
        {
            String strResponse = String.Empty;
            try
            {
                if (ServicePointManager.ServerCertificateValidationCallback == null)
                    ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(OnRemoteCertificateValidationCallback);

                //バイト型配列に変換
                byte[] postDataBytes = System.Text.Encoding.ASCII.GetBytes(postData);

                //WebRequestの作成
                WebRequest objReq = WebRequest.Create(url);
                objReq.Method = "POST";
                objReq.ContentType = "application/x-www-form-urlencoded";
                objReq.ContentLength = postDataBytes.Length;
                objReq.Timeout = 40000;

                //データをPOST送信するためのStreamを取得
                Stream objReqStream = objReq.GetRequestStream();
                objReqStream.Write(postDataBytes, 0, postDataBytes.Length);
                objReqStream.Close();

                //サーバーからの応答を受信するためのWebResponseを取得
                WebResponse objRes = objReq.GetResponse();
                Stream objResStream = objRes.GetResponseStream();
                StreamReader objReader = new StreamReader(objResStream);
                strResponse = objReader.ReadToEnd();
                strResponse = strResponse.Replace(((char)65279).ToString(), "");
                objReader.Close();
            }
            catch (Exception ex)
            {
                error = ex;

                if (postData.Length > 100)
                    postData = postData.Substring(0, 100);
                Log.outputError(ex);
                Log.outputError(new Exception("\ncurerntMethod:" + methodBase));
                Log.outputError(new Exception("\nurl:" + url));
                Log.outputError(new Exception("\npostData:" + postData));

            }
            return strResponse;
        }

        internal static String postProxy(String urlProxy, String url, String postData, MethodBase methodBase, ref Exception error)
        {
            String strResponse = String.Empty;
            try
            {
                using (WebClient wc = new WebClient())
                {
                    String strUrl = urlProxy + "?url=" + Uri.EscapeDataString(url);
                    strResponse = wc.DownloadString(strUrl);
                }
            }
            catch (Exception ex)
            {
                error = ex;

                Log.outputError(ex);
                Log.outputError(new Exception("\ncurerntMethod:" + methodBase));
                Log.outputError(new Exception("\nurl:" + url));
                Log.outputError(new Exception("\npostData:" + postData));

            }
            return strResponse;
        }

        internal static String getEscapeLongDataString(string stringToEscape)
        {
            var sb = new StringBuilder();
            var length = stringToEscape.Length;

            // Uri.c_MaxUriBufferSize以上だとエラーになるため
            // Uri.c_MaxUriBufferSize - 1を上限とする
            var limit = 0xFFF0 - 1;

            // limitごとに区切って処理
            for (int i = 0; i < length; i += limit)
            {
                sb.Append(Uri.EscapeDataString(stringToEscape.Substring(i, Math.Min(limit, length - i))));
            }

            return sb.ToString();
        }

        private static bool OnRemoteCertificateValidationCallback(Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;	// 無条件でオレオレ証明を信用する。危険！(senderのURIとか調
        }
    }
}

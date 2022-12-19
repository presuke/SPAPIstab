using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using RestSharp;

namespace SPAPIstab
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        internal static String UserID = "testUser";
        internal static String appName = "testApps";
        private const String PATH_API_SETTING = "api.xml";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (File.Exists(PATH_API_SETTING))
                {
                    XmlDocument objDom = new XmlDocument();
                    objDom.Load(PATH_API_SETTING);
                    XmlNode objRoot = objDom.SelectSingleNode("//root");
                    if(objRoot != null)
                    {
                        SellerId.Text = objRoot.Attributes["sellerID"].Value;
                        RefleshToken.Text = objRoot.Attributes["refleshToken"].Value;
                    }
                }
            }
            catch(Exception ex)
            {

            }
        }

        private void btnTest_Click(object sender, RoutedEventArgs e)
        {
            btnTest.IsEnabled = false;

            SellerId.Text = SellerId.Text.Replace("　", "").Replace(" ", "").Replace("\t", "").Replace("\r", "").Replace("\n", "");
            RefleshToken.Text = RefleshToken.Text.Replace("　", "").Replace(" ", "").Replace("\t", "").Replace("\r", "").Replace("\n", "");

            String strRet = SPAPI.testSPAPI(SellerId.Text, RefleshToken.Text, SPAPI.strAppId, SPAPI.Region.JP);
            if (strRet == SPAPI.RETURN_CODE_SUCCESS)
            {
                saveApiKey();
                MessageBox.Show("この設定でアクセスできます.", "通知", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(this, "このAPIキーでは、アクセスできません.\n------応答結果------\n" + strRet, "通知", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }

            btnTest.IsEnabled = true;
        }

        private void saveApiKey()
        {
            try
            {
                XmlDocument objDom = new XmlDocument();
                XmlElement objRoot = objDom.CreateElement("root");
                objRoot.SetAttribute("sellerID", SellerId.Text);
                objRoot.SetAttribute("refleshToken", RefleshToken.Text);
                objDom.AppendChild(objRoot);
                objDom.Save(PATH_API_SETTING);
            }
            catch(Exception ex)
            {

            }
        }

        private void btnCall_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                txtResponse.Text = "calling api..";
                ComboBoxItem cmb = (ComboBoxItem)cmbAction.SelectedItem;
                String _SellerID = SellerId.Text;
                String _RefleshToken = RefleshToken.Text;
                String strAction = cmb.Content + "";
                String _txtAsins = txtAsins.Text;
                String _txtAsin = txtAsin.Text;
                String _txtSKU = txtSKU.Text;
                String _txtJans = txtJans.Text;
                String _txtKeyword = txtKeywords.Text;
                Boolean _chkNew = chkNew.IsChecked == true;
                SPAPI.ItemCondition condition = _chkNew ? SPAPI.ItemCondition.NEW : SPAPI.ItemCondition.USED;
                String strText = txtResponse.Text;

                Dictionary<String, DataModel.ItemAttribute> dctReq = new Dictionary<String, DataModel.ItemAttribute>();
                Dictionary<String, String> dctRes = new Dictionary<string, string>();
                Task.Run(async () =>
                {
                    switch (strAction)
                    {

                        case "getItemOffers":
                            DataModel.ItemAttribute item1 = new DataModel.ItemAttribute();
                            item1.asin = _txtAsin;
                            dctReq.Add(item1.asin, item1);
                            dctRes = SPAPI.getItemOffers(_SellerID, _RefleshToken, SPAPI.strAppId, SPAPI.Region.JP, dctReq, condition);
                            SPAPI.getItemOffersToDictionary(dctRes, ref dctReq, condition);
                            break;

                        case "getItemsOffersBatch":
                            foreach (String strAsin in _txtAsins.Split(','))
                            {
                                DataModel.ItemAttribute item = new DataModel.ItemAttribute();
                                item.asin = strAsin;
                                dctReq.Add(item.asin, item);
                            }
                            dctRes = SPAPI.getItemsOffersBatch(_SellerID, _RefleshToken, SPAPI.strAppId, SPAPI.Region.JP, dctReq, condition);
                            SPAPI.getItemOffersBatchToDictionary(dctRes, ref dctReq, condition);
                            break;

                        case "getListingOffers":
                            DataModel.ItemAttribute item2 = new DataModel.ItemAttribute();
                            item2.sku = _txtSKU;
                            dctReq.Add(item2.sku, item2);
                            dctRes = SPAPI.getListingOffers(_SellerID, _RefleshToken, SPAPI.strAppId, SPAPI.Region.JP, dctReq, condition);
                            SPAPI.getListingOffersToDictionary(dctRes, ref dctReq);
                            break;

                        case "getCatalogItem":
                            foreach (String strAsin in _txtAsins.Split(','))
                            {
                                DataModel.ItemAttribute item = new DataModel.ItemAttribute();
                                item.asin = strAsin;
                                dctReq.Add(item.asin, item);
                            }
                            dctRes = SPAPI.getCatalogItem(_SellerID, _RefleshToken, SPAPI.strAppId, SPAPI.Region.JP, dctReq);
                            SPAPI.getCatalogItemToDictionary(dctRes, ref dctReq);
                            break;

                        case "getPricing":
                            foreach (String strAsin in _txtAsins.Split(','))
                            {
                                DataModel.ItemAttribute item = new DataModel.ItemAttribute();
                                item.asin = strAsin;
                                dctReq.Add(item.asin, item);
                            }
                            dctRes = SPAPI.getPricing(_SellerID, _RefleshToken, SPAPI.strAppId, SPAPI.Region.JP, dctReq, SPAPI.KindGetPricing.Asin);
                            SPAPI.getPricingToDictionary(dctRes, ref dctReq);
                            break;

                        case "listCatalogItems_JAN":
                            foreach (String strJan in _txtJans.Split(','))
                            {
                                DataModel.ItemAttribute item = new DataModel.ItemAttribute();
                                item.ean = strJan;
                                dctReq.Add(item.ean, item);
                            }
                            dctRes = SPAPI.listCatalogItems(_SellerID, _RefleshToken, SPAPI.strAppId, SPAPI.Region.JP, dctReq, SPAPI.KindListCatalogItems.ean);
                            SPAPI.listCatalogItemsToDictionary(dctRes, ref dctReq);
                            break;

                        case "listCatalogItems_KEYWORD":
                            foreach (String strJan in _txtJans.Split(','))
                            {
                                DataModel.ItemAttribute item = new DataModel.ItemAttribute();
                                item.ean = strJan;
                                dctReq.Add(item.ean, item);
                            }
                            String strRes = SPAPI.listCatalogItemsKeyword(_SellerID, _RefleshToken, SPAPI.strAppId, SPAPI.Region.JP, _txtKeyword);
                            this.Dispatcher.Invoke((Action)(() =>
                            {
                                txtResponse.Text = strRes;
                            }));
                            break;

                        case "getCompetitivePricing":
                            foreach (String strAsin in _txtAsins.Split(','))
                            {
                                DataModel.ItemAttribute item = new DataModel.ItemAttribute();
                                item.asin = strAsin;
                                dctReq.Add(item.asin, item);
                            }
                            dctRes = SPAPI.getCompetitivePricing(_SellerID, _RefleshToken, SPAPI.strAppId, SPAPI.Region.JP, dctReq, SPAPI.KindGetCompetitivePricing.Asin);
                            SPAPI.getCompetitivePricingToDictionary(dctRes, ref dctReq);
                            break;

                        case "getMyFeesEstimateForASIN":
                            foreach (String strAsin in _txtAsins.Split(','))
                            {
                                DataModel.ItemAttribute item = new DataModel.ItemAttribute();
                                item.asin = strAsin;
                                dctReq.Add(item.asin, item);
                            }
                            dctRes = SPAPI.getMyFeesEstimateForASIN(_SellerID, _RefleshToken, SPAPI.strAppId, SPAPI.Region.JP, dctReq);
                            SPAPI.getMyFeesEstimateForASINToDictionary(dctRes, ref dctReq);
                            break;

                        case "submitFeed":
                            strText = SPAPI.createFeedDocument(_SellerID, _RefleshToken, SPAPI.strAppId, SPAPI.Region.JP, "{\"contentType\":\"text/xml; charset=UTF-8\"}");
                            XmlDocument objDom = new XmlDocument();
                            String strDocumentId = "";
                            String strUrl = "";
                            String strKey = "";
                            String strIV = "";
                            objDom.LoadXml(strText);

                            if (objDom.SelectSingleNode("//payload/feedDocumentId") != null)
                                strDocumentId = objDom.SelectSingleNode("//payload/feedDocumentId").InnerText;

                            if (objDom.SelectSingleNode("//payload/url") != null)
                                strUrl = objDom.SelectSingleNode("//payload/url").InnerText;

                            if (objDom.SelectSingleNode("//payload/encryptionDetails/key") != null)
                                strKey = objDom.SelectSingleNode("//payload/encryptionDetails/key").InnerText;

                            if (objDom.SelectSingleNode("//payload/encryptionDetails/initializationVector") != null)
                                strIV = objDom.SelectSingleNode("//payload/encryptionDetails/initializationVector").InnerText;


                            if (!String.IsNullOrEmpty(strDocumentId) &&
                               !String.IsNullOrEmpty(strUrl))
                            {
                                //コンテンツアップロード
                                String content = "";
                                using (StreamReader sr = new StreamReader(new FileStream("testFeed.txt", FileMode.Open)))
                                {
                                    content = sr.ReadToEnd();
                                }

                                //コンテンツを暗号化
                                Byte[] key = Convert.FromBase64String(strKey);
                                Byte[] iv = Convert.FromBase64String(strIV);
                                Byte[] bytes = EncryptStringToBytes_Aes(content, key, iv);


                                var contentType = "text/plain; charset=utf-8"; // this should be the same as what was used in Step #1 (in the CreateFeedDocument API request)

                                RestClient restClient = new RestClient(strUrl);
                                IRestRequest restRequest = new RestRequest(Method.PUT);
                                restRequest.AddParameter(contentType, bytes, ParameterType.RequestBody);

                                IRestResponse response = restClient.Execute(restRequest);


                                String strA = SPAPI.createFeed(_SellerID, _RefleshToken, SPAPI.strAppId, new SPAPI.Region[] { SPAPI.Region.JP }, "POST_PRODUCT_DATA", strDocumentId);
                                String strFeedRequest = "{\"feedType\":\"\",";
                                strFeedRequest += "{\"marketplaceIds\":[\"A1VC38T7YXB528\"],";
                                strFeedRequest += "\"inputFeedDocumentId\":\"" + strDocumentId +"\"}";

                            }

                            this.Dispatcher.Invoke((Action)(() =>
                            {
                                txtResponse.Text = strText;
                            }));
                            break;

                        default:
                            break;
                    }

                    if (dctRes.Count > 0)
                    {
                        String strText = "";
                        foreach (String strXml in dctRes.Values)
                        {
                            strText += strXml;
                            strText += "\r\n---\r\n";
                        }
                        this.Dispatcher.Invoke((Action)(() =>
                        {
                            txtResponse.Text = strText;
                        }));
                    }
                });
            }
            catch(Exception ex)
            {

            }
        }

        private byte[] EncryptStringToBytes_Aes(string plainText, byte[] key, byte[] initializationVector)
        {
            // Check arguments.
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (key == null || key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (initializationVector == null || initializationVector.Length <= 0)
                throw new ArgumentNullException("initializationVector");
            byte[] encrypted;

            // Create an Aes object
            // with the specified key and IV.
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = initializationVector;

                // Create an encryptor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt, Encoding.UTF8))
                        {
                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }

            // Return the encrypted bytes from the memory stream.
            return encrypted;
        }

        private string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] Key, byte[] initializationVector, string compressionAlgorithm)
        {
            // Validate Compression Algorithm
            var isGzip = string.Equals(compressionAlgorithm, "GZIP", StringComparison.OrdinalIgnoreCase);
            var compressionAlgorithmValid = compressionAlgorithm == null || isGzip;

            if (!compressionAlgorithmValid)
            {
                throw new InvalidOperationException($"Unexpected CompressionAlgorithm encounted. compressionAlgorithm = {compressionAlgorithm}");
            }

            // Check arguments.
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (initializationVector == null || initializationVector.Length <= 0)
                throw new ArgumentNullException("IV");

            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;

            // Create an Aes object
            // with the specified key and IV.
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = initializationVector;

                // Create a decryptor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        if (isGzip)
                        {
                            using (var decompressedFileStream = new MemoryStream())
                            {

                                using (GZipStream decompressionStream = new GZipStream(csDecrypt, CompressionMode.Decompress))
                                {
                                    decompressionStream.CopyTo(decompressedFileStream);
                                    decompressedFileStream.Position = 0;

                                    using (var writer = new StreamReader(decompressedFileStream))
                                    {
                                        plaintext = writer.ReadToEnd();
                                    }
                                }

                            }
                        }
                        else
                        {
                            using (StreamReader srDecrypt = new StreamReader(csDecrypt, Encoding.UTF8))
                            {
                                // Read the decrypted bytes from the decrypting stream
                                // and place them in a string.
                                plaintext = srDecrypt.ReadToEnd();
                            }
                        }
                    }
                }
            }

            return plaintext;
        }

        private void cmbAction_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                ComboBoxItem cmb = (ComboBoxItem)cmbAction.SelectedItem;
                switch (cmb.Content) {
                    case "getItemOffers":
                        lblAsins.IsEnabled = false;
                        lblAsin.IsEnabled = true;
                        lblSKU.IsEnabled = false;
                        lblKeyword.IsEnabled = false;
                        lblJans.IsEnabled = false;
                        txtAsins.IsEnabled = false;
                        txtAsin.IsEnabled = true;
                        txtSKU.IsEnabled = false;
                        txtKeywords.IsEnabled = false;
                        txtJans.IsEnabled = false;
                        chkNew.IsEnabled = true;
                        break;

                    case "getItemsOffersBatch":
                        lblAsins.IsEnabled = true;
                        lblAsin.IsEnabled = false;
                        lblSKU.IsEnabled = false;
                        lblKeyword.IsEnabled = false;
                        lblJans.IsEnabled = false;
                        txtAsins.IsEnabled = true;
                        txtAsin.IsEnabled = false;
                        txtSKU.IsEnabled = false;
                        txtKeywords.IsEnabled = false;
                        txtJans.IsEnabled = false;
                        chkNew.IsEnabled = true;
                        break;

                    case "getListingOffers":
                        lblAsins.IsEnabled = false;
                        lblAsin.IsEnabled = false;
                        lblSKU.IsEnabled = true;
                        lblKeyword.IsEnabled = false;
                        lblJans.IsEnabled = false;
                        txtAsins.IsEnabled = false;
                        txtAsin.IsEnabled = false;
                        txtSKU.IsEnabled = true;
                        txtKeywords.IsEnabled = false;
                        txtJans.IsEnabled = false;
                        chkNew.IsEnabled = true;
                        break;

                    case "getCatalogItem":
                        lblAsins.IsEnabled = true;
                        lblAsin.IsEnabled = false;
                        lblSKU.IsEnabled = false;
                        lblKeyword.IsEnabled = false;
                        lblJans.IsEnabled = false;
                        txtAsins.IsEnabled = true;
                        txtAsin.IsEnabled = false;
                        txtSKU.IsEnabled = true;
                        txtKeywords.IsEnabled = false;
                        txtJans.IsEnabled = false;
                        chkNew.IsEnabled = false;
                        break;

                    case "listCatalogItems_JAN":
                        lblAsins.IsEnabled = false;
                        lblAsin.IsEnabled = false;
                        lblSKU.IsEnabled = false;
                        lblKeyword.IsEnabled = false;
                        lblJans.IsEnabled = true;
                        txtAsins.IsEnabled = false;
                        txtAsin.IsEnabled = false;
                        txtSKU.IsEnabled = true;
                        txtKeywords.IsEnabled = false;
                        txtJans.IsEnabled = true;
                        chkNew.IsEnabled = false;
                        break;

                    case "listCatalogItems_KEYWORD":
                        lblAsins.IsEnabled = false;
                        lblAsin.IsEnabled = false;
                        lblSKU.IsEnabled = false;
                        lblKeyword.IsEnabled = true;
                        lblJans.IsEnabled = false;
                        txtAsins.IsEnabled = false;
                        txtAsin.IsEnabled = false;
                        txtSKU.IsEnabled = true;
                        txtKeywords.IsEnabled = true;
                        txtJans.IsEnabled = false;
                        chkNew.IsEnabled = false;
                        break;

                    case "getPricing":
                    case "getCompetitivePricing":
                    case "getMyFeesEstimateForASIN":
                        lblAsins.IsEnabled = true;
                        lblAsin.IsEnabled = false;
                        lblSKU.IsEnabled = false;
                        lblKeyword.IsEnabled = false;
                        lblJans.IsEnabled = false;
                        txtAsins.IsEnabled = true;
                        txtAsin.IsEnabled = false;
                        txtSKU.IsEnabled = true;
                        txtKeywords.IsEnabled = false;
                        txtJans.IsEnabled = false;
                        chkNew.IsEnabled = false;
                        break;

                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void lnkGetApiKey_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ProcessStartInfo pi = new ProcessStartInfo()
            {
                FileName = "https://sellercentral.amazon.co.jp/apps/store/dp/" + SPAPI.strAppId,
                UseShellExecute = true,
            };
            Process.Start(pi);
        }
    }
}

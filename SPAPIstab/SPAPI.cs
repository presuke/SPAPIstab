using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Xml;
using Newtonsoft.Json;

namespace SPAPIstab
{
    class SPAPI
    {
        internal const String strAppId = "amzn1.sellerapps.app.b8e54650-144f-4372-baf3-8db8d33115b8";
        private const String strEndPoint = "https://main-trademark.ssl-lolipop.jp/spapiphp/res.php";

        private static Dictionary<Region, String> dctRegion_region = new Dictionary<Region, string>() { { Region.JP, "us-west-2" } };
        private static Dictionary<Region, String> dctRegion_endpoint = new Dictionary<Region, string>() { { Region.JP, "https://sellingpartnerapi-fe.amazon.com" } };
        private static Dictionary<Region, String> dctRegion_marketplace_id = new Dictionary<Region, string>() { { Region.JP, "A1VC38T7YXB528" } };

        internal static String RETURN_CODE_SUCCESS = "Success";

        const String SELLERID_AMAZON = "AN1VRQENFRJN5";

        internal enum Region
        {
            JP,
            US
        }

        internal enum KindListCatalogItems
        {
            seller_sku,
            upc,
            ean,
            isbn,
            jan
        }

        internal enum KindGetPricing
        {
            Asin,
            Sku
        }

        internal enum KindGetCompetitivePricing
        {
            Asin,
            Sku
        }

        internal enum ItemCondition
        {
            NEW,
            USED
        }

        private static String getResponse(Dictionary<String, String> dct, MethodBase calledMethod, int maxRetry, int intervalMillSec)
        {
            String strRet = String.Empty;
            try
            {
                XmlDataDocument objDom = new XmlDataDocument();
                XmlElement objRoot = objDom.CreateElement("root");
                foreach (String strKey in dct.Keys)
                {
                    XmlElement objElement = objDom.CreateElement(strKey);
                    objElement.InnerText = dct[strKey];
                    objRoot.AppendChild(objElement);
                }

                String strURL = strEndPoint;
                String strParam = "xml=" + IF.getEscapeLongDataString(objRoot.OuterXml);

                //レスポンスフォーマットを設定（オプション内に"format"という名前で指定されていたら、それをフォーマットにする)
                String strFormat = String.Empty;
                if (dct.Keys.Contains("format"))
                    strFormat = dct["format"];

                for (int intTryCnt = 1; intTryCnt <= maxRetry; intTryCnt++)
                {
                    Exception exPost = null;
                    String strResponse = IF.post(strURL, strParam, calledMethod, ref exPost);

                    if (exPost == null)
                    {
                        try
                        {
                            //レスポンスフォーマットに応じて整合性をチェックする(どのみちXMLで返す)
                            XmlDocument objDomRet = new XmlDocument();
                            switch (strFormat)
                            {
                                case "json":
                                    try
                                    {
                                        var objXml = JsonConvert.DeserializeXmlNode(strResponse);
                                        strRet = objXml.OuterXml;
                                    }
                                    catch (Exception ex)
                                    {
                                        try
                                        {
                                            var objXml = JsonConvert.DeserializeXmlNode("{\"root\":" + strResponse + "}");
                                            strRet = objXml.OuterXml;
                                        }
                                        catch (Exception ex2)
                                        {
                                        }
                                    }
                                    break;

                                default:
                                    objDomRet.LoadXml(strResponse);
                                    strRet = strResponse;
                                    break;
                            }
                            break;
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                    else
                    {
                        System.Threading.Thread.Sleep(intervalMillSec);
                    }

                    //試行回数上限に達した場合
                    if (intTryCnt == maxRetry)
                    {
                    }

                    strRet = intTryCnt + ":" + strResponse;
                }
            }
            catch (Exception ex)
            {
                Log.outputError(ex);
            }
            return strRet;
        }

        private static String getXmlInnerText(XmlNode node)
        {
            String strRet = String.Empty;
            try
            {
                if (node != null)
                {
                    if (!String.IsNullOrEmpty(node.Value))
                    {
                        strRet = node.Value;
                    }

                    if (String.IsNullOrEmpty(strRet))
                    {
                        if (node != null)
                        {
                            strRet = node.InnerText;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.outputError(ex);
            }
            return strRet;
        }

        internal static String testSPAPI(String selling_partner_id, String refleshToken, String appId, Region region)
        {
            String strRet = String.Empty;

            try
            {
                Dictionary<String, String> dctParam = new Dictionary<string, string>();
                dctParam.Add("action", "listCatalogItems_v202204");
                dctParam.Add("UserId", MainWindow.UserID);
                dctParam.Add("appId", appId);
                dctParam.Add("appName", MainWindow.appName);
                dctParam.Add("region", dctRegion_region[region]);
                dctParam.Add("endpoint", dctRegion_endpoint[region]);
                dctParam.Add("marketplaceIds", dctRegion_marketplace_id[region]);
                dctParam.Add("selling_partner_id", selling_partner_id);
                dctParam.Add("refleshToken", refleshToken);
                dctParam.Add("format", "json");
                dctParam.Add("keywords", "a");

                String strURL = strEndPoint;
                String strXml = getResponse(dctParam, MethodBase.GetCurrentMethod(), 2, 1000);

                try
                {
                    XmlDocument objDom = new XmlDocument();
                    objDom.LoadXml(strXml);
                    strRet = RETURN_CODE_SUCCESS;
                }
                catch (Exception ex)
                {
                    strRet = "エラーが発生しました。\n" + strXml;
                }
            }
            catch (Exception ex)
            {
                strRet = ex.Message;
            }
            return strRet;
        }

        internal static Dictionary<String, String> getItemOffers(String selling_partner_id, String refleshToken, String appId, Region region, Dictionary<String, DataModel.ItemAttribute> dct, ItemCondition item_condition)
        {
            Dictionary<String, String> dctRet = new Dictionary<string, string>();

            foreach (String strAsin in dct.Keys)
            {
                if (!dctRet.Keys.Contains(strAsin))
                {
                    dctRet.Add(strAsin, String.Empty);

                    try
                    {
                        Dictionary<String, String> dctParam = new Dictionary<string, string>();
                        dctParam.Add("action", "getItemOffers");
                        dctParam.Add("UserId", MainWindow.UserID);
                        dctParam.Add("appId", appId);
                        dctParam.Add("appName", MainWindow.appName);
                        dctParam.Add("region", dctRegion_region[region]);
                        dctParam.Add("endpoint", dctRegion_endpoint[region]);
                        dctParam.Add("marketplace_id", dctRegion_marketplace_id[region]);
                        dctParam.Add("selling_partner_id", selling_partner_id);
                        dctParam.Add("refleshToken", refleshToken);
                        dctParam.Add("item_condition", item_condition.ToString());
                        dctParam.Add("asin", strAsin);
                        dctParam.Add("format", "json");

                        String strURL = strEndPoint;
                        String strResponse = getResponse(dctParam, MethodBase.GetCurrentMethod(), 5, 1000);
                        dctRet[strAsin] = strResponse;
                    }
                    catch (Exception ex)
                    {
                        dctRet[strAsin] = ex.Message;
                    }
                }
            }
            return dctRet;
        }

        internal static Dictionary<String, String> getCatalogItem(String selling_partner_id, String refleshToken, String appId, Region region, Dictionary<String, DataModel.ItemAttribute> dct)
        {
            Dictionary<String, String> dctRet = new Dictionary<string, string>();

            foreach (String strAsin in dct.Keys)
            {
                if (!dctRet.Keys.Contains(strAsin))
                {
                    dctRet.Add(strAsin, String.Empty);

                    try
                    {
                        Dictionary<String, String> dctParam = new Dictionary<string, string>();
                        dctParam.Add("action", "getCatalogItem_v202204");
                        dctParam.Add("UserId", MainWindow.UserID);
                        dctParam.Add("appId", appId);
                        dctParam.Add("appName", MainWindow.appName);
                        dctParam.Add("region", dctRegion_region[region]);
                        dctParam.Add("endpoint", dctRegion_endpoint[region]);
                        dctParam.Add("marketplaceIds", dctRegion_marketplace_id[region]);
                        dctParam.Add("selling_partner_id", selling_partner_id);
                        dctParam.Add("refleshToken", refleshToken);
                        dctParam.Add("asin", strAsin);
                        dctParam.Add("includedData", "attributes,identifiers,images,productTypes,salesRanks,summaries,variations");
                        dctParam.Add("format", "json");

                        String strURL = strEndPoint;
                        String strResponse = getResponse(dctParam, MethodBase.GetCurrentMethod(), 5, 1000);
                        dctRet[strAsin] = strResponse;
                    }
                    catch (Exception ex)
                    {
                        dctRet[strAsin] = ex.Message;
                    }
                }
            }
            return dctRet;
        }

        internal static Dictionary<String, String> listCatalogItems(String selling_partner_id, String refleshToken, String appId, Region region, Dictionary<String, DataModel.ItemAttribute> dct, KindListCatalogItems kind)
        {
            //kind
            //PHP side script
            //$query = getTextContent($dom->query);
            //$query_context_id = getTextContent($dom->query_context_id);
            //$seller_sku = getTextContent($dom->seller_sku);
            //$upc = getTextContent($dom->upc);
            //$ean = getTextContent($dom->ean);
            //$isbn = getTextContent($dom->isbn);
            //$jan = getTextContent($dom->jan);
            //$result = $apiInstance->listCatalogItems($marketplace_id, $query, $query_context_id, $seller_sku, $upc, $ean, $isbn, $jan);

            Dictionary<String, String> dctRet = new Dictionary<string, string>();

            foreach (String strKey in dct.Keys)
            {
                if (!dctRet.Keys.Contains(strKey))
                {
                    dctRet.Add(strKey, String.Empty);

                    try
                    {
                        Dictionary<String, String> dctParam = new Dictionary<string, string>();
                        dctParam.Add("action", "listCatalogItems_v202204");
                        dctParam.Add("UserId", MainWindow.UserID);
                        dctParam.Add("appId", appId);
                        dctParam.Add("appName", MainWindow.appName);
                        dctParam.Add("region", dctRegion_region[region]);
                        dctParam.Add("endpoint", dctRegion_endpoint[region]);
                        dctParam.Add("marketplaceIds", dctRegion_marketplace_id[region]);
                        dctParam.Add("selling_partner_id", selling_partner_id);
                        dctParam.Add("refleshToken", refleshToken);
                        dctParam.Add("format", "json");
                        dctParam.Add("identifiersType", "EAN");
                        dctParam.Add("identifiers", strKey);

                        String strURL = strEndPoint;
                        String strResponse = getResponse(dctParam, MethodBase.GetCurrentMethod(), 5, 1000);
                        dctRet[strKey] = strResponse;
                    }
                    catch (Exception ex)
                    {
                        dctRet[strKey] = ex.Message;
                    }
                }
            }
            return dctRet;
        }

        internal static String listCatalogItemsKeyword(String selling_partner_id, String refleshToken, String appId, Region region, String keywords)
        {
            String strRet = String.Empty;

            try
            {
                Dictionary<String, String> dctParam = new Dictionary<string, string>();
                dctParam.Add("action", "listCatalogItems_v202204");
                dctParam.Add("UserId", MainWindow.UserID);
                dctParam.Add("appId", appId);
                dctParam.Add("appName", MainWindow.appName);
                dctParam.Add("region", dctRegion_region[region]);
                dctParam.Add("endpoint", dctRegion_endpoint[region]);
                dctParam.Add("marketplaceIds", dctRegion_marketplace_id[region]);
                dctParam.Add("selling_partner_id", selling_partner_id);
                dctParam.Add("refleshToken", refleshToken);
                dctParam.Add("format", "json");
                dctParam.Add("keywords", keywords);

                String strURL = strEndPoint;
                String strXml = getResponse(dctParam, MethodBase.GetCurrentMethod(), 2, 1000);

                try
                {
                    XmlDocument objDom = new XmlDocument();
                    objDom.LoadXml(strXml);
                    strRet = strXml;
                }
                catch (Exception ex)
                {
                    strRet = "エラーが発生しました。\n" + strXml;
                }
            }
            catch (Exception ex)
            {
                strRet = ex.Message;
            }
            return strRet;
        }

        internal static Dictionary<String, String> getListingOffers(String selling_partner_id, String refleshToken, String appId, Region region, Dictionary<String, DataModel.ItemAttribute> dct, ItemCondition item_condition)
        {
            Dictionary<String, String> dctRet = new Dictionary<string, string>();

            foreach (String strSku in dct.Keys)
            {
                if (!dctRet.Keys.Contains(strSku))
                {
                    dctRet.Add(strSku, String.Empty);

                    try
                    {
                        Dictionary<String, String> dctParam = new Dictionary<string, string>();
                        dctParam.Add("action", "getListingOffers");
                        dctParam.Add("UserId", MainWindow.UserID);
                        dctParam.Add("appId", appId);
                        dctParam.Add("appName", MainWindow.appName);
                        dctParam.Add("region", dctRegion_region[region]);
                        dctParam.Add("endpoint", dctRegion_endpoint[region]);
                        dctParam.Add("marketplace_id", dctRegion_marketplace_id[region]);
                        dctParam.Add("selling_partner_id", selling_partner_id);
                        dctParam.Add("refleshToken", refleshToken);
                        dctParam.Add("seller_sku", strSku);
                        dctParam.Add("item_condition", item_condition.ToString());
                        dctParam.Add("format", "json");

                        String strURL = strEndPoint;
                        String strResponse = getResponse(dctParam, MethodBase.GetCurrentMethod(), 5, 1000);
                        dctRet[strSku] = strResponse;
                    }
                    catch (Exception ex)
                    {
                        dctRet[strSku] = ex.Message;
                    }
                }
            }
            return dctRet;
        }

        internal static Dictionary<String, String> getPricing(String selling_partner_id, String refleshToken, String appId, Region region, Dictionary<String, DataModel.ItemAttribute> dct, KindGetPricing kind)
        {
            //$asins = getTextContent($dom->asins);
            //if ($asins != null)
            //	$asins = explode(",", $asins);
            //$skus = getTextContent($dom->skus);
            //if ($skus != null)
            //	$skus = explode(",", $skus);

            Dictionary<KindGetPricing, String> DCT_ITEM_NAME = new Dictionary<KindGetPricing, string>() {
                { KindGetPricing.Asin, "asins" },
                { KindGetPricing.Sku, "skus" }
            };

            Dictionary<String, String> dctRet = new Dictionary<String, String>();
            String strKey = String.Join(",", dct.Keys.ToList<String>());
            try
            {
                Dictionary<String, String> dctParam = new Dictionary<string, string>();
                dctParam.Add("action", "getPricing");
                dctParam.Add("UserId", MainWindow.UserID);
                dctParam.Add("appId", appId);
                dctParam.Add("appName", MainWindow.appName);
                dctParam.Add("region", dctRegion_region[region]);
                dctParam.Add("endpoint", dctRegion_endpoint[region]);
                dctParam.Add("marketplace_id", dctRegion_marketplace_id[region]);
                dctParam.Add("selling_partner_id", selling_partner_id);
                dctParam.Add("refleshToken", refleshToken);
                dctParam.Add("item_type", kind.ToString());
                dctParam.Add(DCT_ITEM_NAME[kind], strKey);
                dctParam.Add("format", "json");

                String strURL = strEndPoint;
                String strResponse = getResponse(dctParam, MethodBase.GetCurrentMethod(), 5, 1000);
                dctRet[strKey] = strResponse;
            }
            catch (Exception ex)
            {
                dctRet[strKey] = ex.Message;
            }
            return dctRet;
        }

        internal static Dictionary<String, String> getCompetitivePricing(String selling_partner_id, String refleshToken, String appId, Region region, Dictionary<String, DataModel.ItemAttribute> dct, KindGetCompetitivePricing kind)
        {
            //$asins = getTextContent($dom->asins);
            //if ($asins != null)
            //	$asins = explode(",", $asins);
            //$skus = getTextContent($dom->skus);
            //if ($skus != null)
            //	$skus = explode(",", $skus);

            Dictionary<KindGetCompetitivePricing, String> DCT_ITEM_NAME = new Dictionary<KindGetCompetitivePricing, string>() {
                { KindGetCompetitivePricing.Asin, "asins" },
                { KindGetCompetitivePricing.Sku, "skus" }
            };

            Dictionary<String, String> dctRet = new Dictionary<String, String>();
            String strKey = String.Join(",", dct.Keys.ToList<String>());
            try
            {
                Dictionary<String, String> dctParam = new Dictionary<string, string>();
                dctParam.Add("action", "getCompetitivePricing");
                dctParam.Add("UserId", MainWindow.UserID);
                dctParam.Add("appId", appId);
                dctParam.Add("appName", MainWindow.appName);
                dctParam.Add("region", dctRegion_region[region]);
                dctParam.Add("endpoint", dctRegion_endpoint[region]);
                dctParam.Add("marketplace_id", dctRegion_marketplace_id[region]);
                dctParam.Add("selling_partner_id", selling_partner_id);
                dctParam.Add("refleshToken", refleshToken);
                dctParam.Add("item_type", kind.ToString());
                dctParam.Add(DCT_ITEM_NAME[kind], strKey);
                dctParam.Add("format", "json");

                String strURL = strEndPoint;
                String strResponse = getResponse(dctParam, MethodBase.GetCurrentMethod(), 5, 1000);
                dctRet[strKey] = strResponse;
            }
            catch (Exception ex)
            {
                dctRet[strKey] = ex.Message;
            }
            return dctRet;
        }

        internal static Dictionary<String, String> getMyFeesEstimateForASIN(String selling_partner_id, String refleshToken, String appId, Region region, Dictionary<String, DataModel.ItemAttribute> dct)
        {
            Dictionary<String, String> dctRet = new Dictionary<string, string>();

            foreach (String strAsin in dct.Keys)
            {
                if (!dctRet.Keys.Contains(strAsin))
                {
                    dctRet.Add(strAsin, String.Empty);
                    try
                    {
                        Dictionary<String, String> dctParam = new Dictionary<string, string>();
                        dctParam.Add("action", "getMyFeesEstimateForASIN");
                        dctParam.Add("UserId", MainWindow.UserID);
                        dctParam.Add("appId", appId);
                        dctParam.Add("appName", MainWindow.appName);
                        dctParam.Add("region", dctRegion_region[region]);
                        dctParam.Add("endpoint", dctRegion_endpoint[region]);
                        dctParam.Add("marketplace_id", dctRegion_marketplace_id[region]);
                        dctParam.Add("selling_partner_id", selling_partner_id);
                        dctParam.Add("refleshToken", refleshToken);
                        dctParam.Add("asin", strAsin);
                        dctParam.Add("format", "json");

                        String strURL = strEndPoint;
                        String strResponse = getResponse(dctParam, MethodBase.GetCurrentMethod(), 5, 1000);

                        if (!strResponse.Contains("ClientError"))
                            Console.WriteLine("");

                        dctRet[strAsin] = strResponse;
                    }
                    catch (Exception ex)
                    {
                        dctRet[strAsin] = ex.Message;
                    }
                }
            }
            return dctRet;
        }

        internal static Dictionary<String, String> getMyFeesEstimateForSKU(String selling_partner_id, String refleshToken, String appId, Region region, Dictionary<String, DataModel.ItemAttribute> dct)
        {
            Dictionary<String, String> dctRet = new Dictionary<string, string>();

            foreach (String strSku in dct.Keys)
            {
                if (!dctRet.Keys.Contains(strSku))
                {
                    dctRet.Add(strSku, String.Empty);
                    try
                    {
                        Dictionary<String, String> dctParam = new Dictionary<string, string>();
                        dctParam.Add("action", "getMyFeesEstimateForSKU");
                        dctParam.Add("UserId", MainWindow.UserID);
                        dctParam.Add("appId", appId);
                        dctParam.Add("appName", MainWindow.appName);
                        dctParam.Add("region", dctRegion_region[region]);
                        dctParam.Add("endpoint", dctRegion_endpoint[region]);
                        dctParam.Add("marketplace_id", dctRegion_marketplace_id[region]);
                        dctParam.Add("selling_partner_id", selling_partner_id);
                        dctParam.Add("refleshToken", refleshToken);
                        dctParam.Add("sku", strSku);
                        dctParam.Add("format", "json");

                        String strURL = strEndPoint;
                        String strResponse = getResponse(dctParam, MethodBase.GetCurrentMethod(), 5, 1000);
                        dctRet[strSku] = strResponse;
                    }
                    catch (Exception ex)
                    {
                        dctRet[strSku] = ex.Message;
                    }
                }
            }
            return dctRet;
        }

        internal static String getOrder(String selling_partner_id, String refleshToken, String appId, Region region, Dictionary<String, DataModel.ItemAttribute> dct)
        {
            String strRet = String.Empty;

            try
            {
                Dictionary<String, String> dctParam = new Dictionary<string, string>();
                dctParam.Add("action", "getOrder");
                dctParam.Add("UserId", MainWindow.UserID);
                dctParam.Add("appId", appId);
                dctParam.Add("appName", MainWindow.appName);
                dctParam.Add("region", dctRegion_region[region]);
                dctParam.Add("endpoint", dctRegion_endpoint[region]);
                dctParam.Add("marketplace_id", dctRegion_marketplace_id[region]);
                dctParam.Add("selling_partner_id", selling_partner_id);
                dctParam.Add("refleshToken", refleshToken);
                dctParam.Add("format", "json");

                String strURL = strEndPoint;
                String strResponse = getResponse(dctParam, MethodBase.GetCurrentMethod(), 5, 1000);
            }
            catch (Exception ex)
            {
                strRet = ex.Message;
            }
            return strRet;
        }

        internal static String getOrders(String selling_partner_id, String refleshToken, String appId, Region region, Dictionary<String, DataModel.ItemAttribute> dct)
        {
            String strRet = String.Empty;

            try
            {
                Dictionary<String, String> dctParam = new Dictionary<string, string>();
                dctParam.Add("action", "getOrders");
                dctParam.Add("UserId", MainWindow.UserID);
                dctParam.Add("appId", appId);
                dctParam.Add("appName", MainWindow.appName);
                dctParam.Add("region", dctRegion_region[region]);
                dctParam.Add("endpoint", dctRegion_endpoint[region]);
                dctParam.Add("marketplace_id", dctRegion_marketplace_id[region]);
                dctParam.Add("selling_partner_id", selling_partner_id);
                dctParam.Add("refleshToken", refleshToken);
                dctParam.Add("format", "json");

                String strURL = strEndPoint;
                String strResponse = getResponse(dctParam, MethodBase.GetCurrentMethod(), 5, 1000);
            }
            catch (Exception ex)
            {
                strRet = ex.Message;
            }
            return strRet;
        }

        internal static String getOrderItems(String selling_partner_id, String refleshToken, String appId, Region region, Dictionary<String, DataModel.ItemAttribute> dct)
        {
            String strRet = String.Empty;

            try
            {
                Dictionary<String, String> dctParam = new Dictionary<string, string>();
                dctParam.Add("action", "getOrderItems");
                dctParam.Add("UserId", MainWindow.UserID);
                dctParam.Add("appId", appId);
                dctParam.Add("appName", MainWindow.appName);
                dctParam.Add("region", dctRegion_region[region]);
                dctParam.Add("endpoint", dctRegion_endpoint[region]);
                dctParam.Add("marketplace_id", dctRegion_marketplace_id[region]);
                dctParam.Add("selling_partner_id", selling_partner_id);
                dctParam.Add("refleshToken", refleshToken);
                dctParam.Add("format", "json");

                String strURL = strEndPoint;
                String strResponse = getResponse(dctParam, MethodBase.GetCurrentMethod(), 5, 1000);
            }
            catch (Exception ex)
            {
                strRet = ex.Message;
            }
            return strRet;
        }

        internal static String getOrderItemsBuyerInfo(String selling_partner_id, String refleshToken, String appId, Region region, Dictionary<String, DataModel.ItemAttribute> dct)
        {
            String strRet = String.Empty;

            try
            {
                Dictionary<String, String> dctParam = new Dictionary<string, string>();
                dctParam.Add("action", "getOrderItemsBuyerInfo");
                dctParam.Add("UserId", MainWindow.UserID);
                dctParam.Add("appId", appId);
                dctParam.Add("appName", MainWindow.appName);
                dctParam.Add("region", dctRegion_region[region]);
                dctParam.Add("endpoint", dctRegion_endpoint[region]);
                dctParam.Add("marketplace_id", dctRegion_marketplace_id[region]);
                dctParam.Add("selling_partner_id", selling_partner_id);
                dctParam.Add("refleshToken", refleshToken);
                dctParam.Add("format", "json");

                String strURL = strEndPoint;
                String strResponse = getResponse(dctParam, MethodBase.GetCurrentMethod(), 5, 1000);
            }
            catch (Exception ex)
            {
                strRet = ex.Message;
            }
            return strRet;
        }

        internal static String getOrderAddress(String selling_partner_id, String refleshToken, String appId, Region region, Dictionary<String, DataModel.ItemAttribute> dct)
        {
            String strRet = String.Empty;

            try
            {
                Dictionary<String, String> dctParam = new Dictionary<string, string>();
                dctParam.Add("action", "getOrderAddress");
                dctParam.Add("UserId", MainWindow.UserID);
                dctParam.Add("appId", appId);
                dctParam.Add("appName", MainWindow.appName);
                dctParam.Add("region", dctRegion_region[region]);
                dctParam.Add("endpoint", dctRegion_endpoint[region]);
                dctParam.Add("marketplace_id", dctRegion_marketplace_id[region]);
                dctParam.Add("selling_partner_id", selling_partner_id);
                dctParam.Add("refleshToken", refleshToken);
                dctParam.Add("format", "json");

                String strURL = strEndPoint;
                String strResponse = getResponse(dctParam, MethodBase.GetCurrentMethod(), 5, 1000);
            }
            catch (Exception ex)
            {
                strRet = ex.Message;
            }
            return strRet;
        }

        internal static String getOrderBuyerInfo(String selling_partner_id, String refleshToken, String appId, Region region, Dictionary<String, DataModel.ItemAttribute> dct)
        {
            String strRet = String.Empty;

            try
            {
                Dictionary<String, String> dctParam = new Dictionary<string, string>();
                dctParam.Add("action", "getOrderBuyerInfo");
                dctParam.Add("UserId", MainWindow.UserID);
                dctParam.Add("appId", appId);
                dctParam.Add("appName", MainWindow.appName);
                dctParam.Add("region", dctRegion_region[region]);
                dctParam.Add("endpoint", dctRegion_endpoint[region]);
                dctParam.Add("marketplace_id", dctRegion_marketplace_id[region]);
                dctParam.Add("selling_partner_id", selling_partner_id);
                dctParam.Add("refleshToken", refleshToken);
                dctParam.Add("format", "json");

                String strURL = strEndPoint;
                String strResponse = getResponse(dctParam, MethodBase.GetCurrentMethod(), 5, 1000);
            }
            catch (Exception ex)
            {
                strRet = ex.Message;
            }
            return strRet;
        }

        internal static String listFinancialEventGroups(String selling_partner_id, String refleshToken, String appId, Region region, Dictionary<String, DataModel.ItemAttribute> dct)
        {
            String strRet = String.Empty;

            try
            {
                Dictionary<String, String> dctParam = new Dictionary<string, string>();
                dctParam.Add("action", "listFinancialEventGroups");
                dctParam.Add("UserId", MainWindow.UserID);
                dctParam.Add("appId", appId);
                dctParam.Add("appName", MainWindow.appName);
                dctParam.Add("region", dctRegion_region[region]);
                dctParam.Add("endpoint", dctRegion_endpoint[region]);
                dctParam.Add("marketplace_id", dctRegion_marketplace_id[region]);
                dctParam.Add("selling_partner_id", selling_partner_id);
                dctParam.Add("refleshToken", refleshToken);
                dctParam.Add("format", "json");

                String strURL = strEndPoint;
                String strResponse = getResponse(dctParam, MethodBase.GetCurrentMethod(), 5, 1000);
            }
            catch (Exception ex)
            {
                strRet = ex.Message;
            }
            return strRet;
        }

        internal static String listFinancialEventsByGroupId(String selling_partner_id, String refleshToken, String appId, Region region, Dictionary<String, DataModel.ItemAttribute> dct)
        {
            String strRet = String.Empty;

            try
            {
                Dictionary<String, String> dctParam = new Dictionary<string, string>();
                dctParam.Add("action", "listFinancialEventsByGroupId");
                dctParam.Add("UserId", MainWindow.UserID);
                dctParam.Add("appId", appId);
                dctParam.Add("appName", MainWindow.appName);
                dctParam.Add("region", dctRegion_region[region]);
                dctParam.Add("endpoint", dctRegion_endpoint[region]);
                dctParam.Add("marketplace_id", dctRegion_marketplace_id[region]);
                dctParam.Add("selling_partner_id", selling_partner_id);
                dctParam.Add("refleshToken", refleshToken);
                dctParam.Add("format", "json");

                String strURL = strEndPoint;
                String strResponse = getResponse(dctParam, MethodBase.GetCurrentMethod(), 5, 1000);
            }
            catch (Exception ex)
            {
                strRet = ex.Message;
            }
            return strRet;
        }

        internal static String listFinancialEventsByOrderId(String selling_partner_id, String refleshToken, String appId, Region region, Dictionary<String, DataModel.ItemAttribute> dct)
        {
            String strRet = String.Empty;

            try
            {
                Dictionary<String, String> dctParam = new Dictionary<string, string>();
                dctParam.Add("action", "listFinancialEventsByOrderId");
                dctParam.Add("UserId", MainWindow.UserID);
                dctParam.Add("appId", appId);
                dctParam.Add("appName", MainWindow.appName);
                dctParam.Add("region", dctRegion_region[region]);
                dctParam.Add("endpoint", dctRegion_endpoint[region]);
                dctParam.Add("marketplace_id", dctRegion_marketplace_id[region]);
                dctParam.Add("selling_partner_id", selling_partner_id);
                dctParam.Add("refleshToken", refleshToken);
                dctParam.Add("format", "json");

                String strURL = strEndPoint;
                String strResponse = getResponse(dctParam, MethodBase.GetCurrentMethod(), 5, 1000);
            }
            catch (Exception ex)
            {
                strRet = ex.Message;
            }
            return strRet;
        }

        internal static String listFinancialEvents(String selling_partner_id, String refleshToken, String appId, Region region, Dictionary<String, DataModel.ItemAttribute> dct)
        {
            String strRet = String.Empty;

            try
            {
                Dictionary<String, String> dctParam = new Dictionary<string, string>();
                dctParam.Add("action", "listFinancialEvents");
                dctParam.Add("UserId", MainWindow.UserID);
                dctParam.Add("appId", appId);
                dctParam.Add("appName", MainWindow.appName);
                dctParam.Add("region", dctRegion_region[region]);
                dctParam.Add("endpoint", dctRegion_endpoint[region]);
                dctParam.Add("marketplace_id", dctRegion_marketplace_id[region]);
                dctParam.Add("selling_partner_id", selling_partner_id);
                dctParam.Add("refleshToken", refleshToken);
                dctParam.Add("format", "json");

                String strURL = strEndPoint;
                String strResponse = getResponse(dctParam, MethodBase.GetCurrentMethod(), 5, 1000);
            }
            catch (Exception ex)
            {
                strRet = ex.Message;
            }
            return strRet;
        }

        internal static String createFeed(String selling_partner_id, String refleshToken, String appId, Region region, String body)
        {
            String strRet = String.Empty;

            try
            {
                Dictionary<String, String> dctParam = new Dictionary<string, string>();
                dctParam.Add("action", "createFeed");
                dctParam.Add("UserId", MainWindow.UserID);
                dctParam.Add("appId", appId);
                dctParam.Add("appName", MainWindow.appName);
                dctParam.Add("region", dctRegion_region[region]);
                dctParam.Add("endpoint", dctRegion_endpoint[region]);
                dctParam.Add("body", body);
                dctParam.Add("selling_partner_id", selling_partner_id);
                dctParam.Add("refleshToken", refleshToken);
                dctParam.Add("format", "json");

                String strURL = strEndPoint;
                String strResponse = getResponse(dctParam, MethodBase.GetCurrentMethod(), 5, 1000);
            }
            catch (Exception ex)
            {
                strRet = ex.Message;
            }
            return strRet;
        }

        internal static String createFeedDocument(String selling_partner_id, String refleshToken, String appId, Region region, String body)
        {
            String strRet = String.Empty;

            try
            {
                Dictionary<String, String> dctParam = new Dictionary<string, string>();
                dctParam.Add("action", "createFeedDocument");
                dctParam.Add("UserId", MainWindow.UserID);
                dctParam.Add("appId", appId);
                dctParam.Add("appName", MainWindow.appName);
                dctParam.Add("region", dctRegion_region[region]);
                dctParam.Add("endpoint", dctRegion_endpoint[region]);
                dctParam.Add("body", body);
                dctParam.Add("selling_partner_id", selling_partner_id);
                dctParam.Add("refleshToken", refleshToken);
                dctParam.Add("format", "json");

                String strURL = strEndPoint;
                String strResponse = getResponse(dctParam, MethodBase.GetCurrentMethod(), 5, 1000);
            }
            catch (Exception ex)
            {
                strRet = ex.Message;
            }
            return strRet;
        }

        internal static void getItemOffersToDictionary(Dictionary<String, String> response, ref Dictionary<String, DataModel.ItemAttribute> dct, ItemCondition condition)
        {
            try
            {
                foreach (String strKey in response.Keys)
                {
                    try
                    {
                        if (dct.Keys.Contains(strKey))
                        {
                            String strXml = response[strKey];
                            if (!String.IsNullOrEmpty(strXml))
                            {
                                DataModel.ItemAttribute item = dct[strKey];

                                XmlDocument objDom = new XmlDocument();
                                objDom.LoadXml(strXml);

                                XmlNodeList objLowestPrices = objDom.SelectNodes(".//payload/Summary/LowestPrices");
                                XmlNodeList objBuyBoxPrices = objDom.SelectNodes(".//payload/Summary/BuyBoxPrices");
                                XmlNodeList objNumberOfOffers = objDom.SelectNodes(".//payload/Summary/NumberOfOffers");
                                XmlNodeList objBuyBoxEligibleOffers = objDom.SelectNodes(".//payload/Summary/BuyBoxEligibleOffers");
                                XmlNodeList objOffers = objDom.SelectNodes(".//payload/Offers");

                                //最安値
                                foreach (XmlNode objLowestPrice in objLowestPrices)
                                {
                                    XmlNode objCondition = objLowestPrice.SelectSingleNode("./condition");
                                    XmlNode objFulfillmentChannel = objLowestPrice.SelectSingleNode("./fulfillmentChannel");
                                    int intListingPrice = 0;
                                    int intShippingPrice = 0;
                                    int intPoint = 0;

                                    int.TryParse(getXmlInnerText(objLowestPrice.SelectSingleNode("./ListingPrice/Amount")), out intListingPrice);
                                    int.TryParse(getXmlInnerText(objLowestPrice.SelectSingleNode("./Shipping/Amount")), out intShippingPrice);
                                    int.TryParse(getXmlInnerText(objLowestPrice.SelectSingleNode("./Points/Amount")), out intPoint);
                                    switch (getXmlInnerText(objCondition) + "_" + getXmlInnerText(objFulfillmentChannel))
                                    {
                                        case "new_Amazon":
                                            item.pna = intListingPrice + intShippingPrice;
                                            item.pna_s = intShippingPrice;
                                            item.pna_p = intPoint;
                                            if (item.pn == 0)
                                            {
                                                item.pn = intListingPrice + intShippingPrice;
                                                item.pn_s = intShippingPrice;
                                                item.pn_p = intPoint;
                                            }
                                            break;

                                        case "new_Merchant":
                                            item.pnm = intListingPrice + intShippingPrice;
                                            item.pnm_s = intShippingPrice;
                                            item.pnm_p = intPoint;
                                            if (item.pn == 0)
                                            {
                                                item.pn = intListingPrice + intShippingPrice;
                                                item.pn_s = intShippingPrice;
                                                item.pn_p = intPoint;
                                            }
                                            break;

                                        case "used_Amazon":
                                            item.pua = intListingPrice + intShippingPrice;
                                            item.pua_s = intShippingPrice;
                                            item.pua_p = intPoint;
                                            if (item.pu == 0)
                                            {
                                                item.pu = intListingPrice + intShippingPrice;
                                                item.pu_s = intShippingPrice;
                                                item.pu_p = intPoint;
                                            }
                                            break;

                                        case "used_Merchant":
                                            item.pum = intListingPrice + intShippingPrice;
                                            item.pum_s = intShippingPrice;
                                            item.pum_p = intPoint;
                                            if (item.pu == 0)
                                            {
                                                item.pu = intListingPrice + intShippingPrice;
                                                item.pu_s = intShippingPrice;
                                                item.pu_p = intPoint;
                                            }
                                            break;
                                    }
                                }

                                //カート価格
                                foreach (XmlNode objBuyBoxPrice in objBuyBoxPrices)
                                {
                                    XmlNode objCondition = objBuyBoxPrice.SelectSingleNode("./condition");
                                    int intListingPrice = 0;
                                    int intShippingPrice = 0;
                                    int intPoint = 0;

                                    int.TryParse(getXmlInnerText(objBuyBoxPrice.SelectSingleNode("./ListingPrice/Amount")), out intListingPrice);
                                    int.TryParse(getXmlInnerText(objBuyBoxPrice.SelectSingleNode("./Shipping/Amount")), out intShippingPrice);
                                    int.TryParse(getXmlInnerText(objBuyBoxPrice.SelectSingleNode("./Points/Amount")), out intPoint);
                                    switch (getXmlInnerText(objCondition))
                                    {
                                        case "new":
                                            item.pc = intListingPrice + intShippingPrice;
                                            item.pc_s = intShippingPrice;
                                            item.pc_p = intPoint;
                                            if (item.pn == 0)
                                            {
                                                item.pn = intListingPrice + intShippingPrice;
                                                item.pn_s = intShippingPrice;
                                                item.pn_p = intPoint;
                                            }
                                            break;

                                        case "used":
                                            break;
                                    }
                                }

                                //出品者数
                                foreach (XmlNode objNumberOfOffer in objNumberOfOffers)
                                {
                                    XmlNode objCondition = objNumberOfOffer.SelectSingleNode("./condition");
                                    XmlNode objFulfillmentChannel = objNumberOfOffer.SelectSingleNode("./fulfillmentChannel");
                                    int intQ = 0;

                                    int.TryParse(getXmlInnerText(objNumberOfOffer.SelectSingleNode("./OfferCount")), out intQ);
                                    switch (getXmlInnerText(objCondition) + "_" + getXmlInnerText(objFulfillmentChannel))
                                    {
                                        case "new_Amazon":
                                            item.qna = intQ;
                                            break;

                                        case "new_Merchant":
                                            item.qnm = intQ;
                                            break;

                                        case "used_Amazon":
                                            item.qua = intQ;
                                            break;

                                        case "used_Merchant":
                                            item.qum = intQ;
                                            break;
                                    }
                                }

                                //カート出品者数
                                foreach (XmlNode objBuyBoxEligibleOffer in objBuyBoxEligibleOffers)
                                {
                                    XmlNode objCondition = objBuyBoxEligibleOffer.SelectSingleNode("./condition");
                                    XmlNode objFulfillmentChannel = objBuyBoxEligibleOffer.SelectSingleNode("./fulfillmentChannel");
                                    int intQ = 0;

                                    int.TryParse(getXmlInnerText(objBuyBoxEligibleOffer.SelectSingleNode("./OfferCount")), out intQ);
                                    switch (getXmlInnerText(objCondition) + "_" + getXmlInnerText(objFulfillmentChannel))
                                    {
                                        case "new_Amazon":
                                            item.qca = intQ;
                                            break;

                                        case "new_Merchant":
                                            item.qcm = intQ;
                                            break;

                                        case "used_Amazon":
                                            break;

                                        case "used_Merchant":
                                            break;
                                    }
                                }

                                //価格リスト
                                if (item.lpn == null)
                                    item.lpn = new List<DataModel.Offer>();

                                if (item.lpu == null)
                                    item.lpu = new List<DataModel.Offer>();

                                foreach (XmlNode objOffer in objOffers)
                                {
                                    DataModel.Offer offer = new DataModel.Offer();
                                    int.TryParse(getXmlInnerText(objOffer.SelectSingleNode("./Shipping/Amount")), out offer.Shipping);
                                    int.TryParse(getXmlInnerText(objOffer.SelectSingleNode("./ListingPrice/Amount")), out offer.ListingPrice);
                                    int.TryParse(getXmlInnerText(objOffer.SelectSingleNode("./ShippingTime/maximumHours")), out offer.maximumHours);
                                    int.TryParse(getXmlInnerText(objOffer.SelectSingleNode("./ShippingTime/minimumHours")), out offer.minimumHours);
                                    offer.availabilityType = getXmlInnerText(objOffer.SelectSingleNode("./ShippingTime/availabilityType"));
                                    int.TryParse(getXmlInnerText(objOffer.SelectSingleNode("./SellerFeedbackRating/FeedbackCount")), out offer.FeedbackCount);
                                    int.TryParse(getXmlInnerText(objOffer.SelectSingleNode("./SellerFeedbackRating/SellerFeedbackRating")), out offer.SellerFeedbackRating);
                                    offer.SubCondition = getXmlInnerText(objOffer.SelectSingleNode("./SubCondition"));
                                    offer.ShipsFromCountry = getXmlInnerText(objOffer.SelectSingleNode("./ShipsFrom/Country"));
                                    offer.SubCondition = getXmlInnerText(objOffer.SelectSingleNode("./SubCondition"));
                                    offer.IsBuyBoxWinner = getXmlInnerText(objOffer.SelectSingleNode("./IsBuyBoxWinner"));
                                    offer.IsFeaturedMerchant = getXmlInnerText(objOffer.SelectSingleNode("./IsFeaturedMerchant"));
                                    offer.IsFulfilledByAmazon = getXmlInnerText(objOffer.SelectSingleNode("./IsFulfilledByAmazon"));
                                    offer.SellerId = getXmlInnerText(objOffer.SelectSingleNode("./SellerId"));
                                    offer.ConditionNotes = getXmlInnerText(objOffer.SelectSingleNode("./ConditionNotes"));


                                    //Amazon.co.jp本体価格
                                    if (offer.SellerId == SELLERID_AMAZON)
                                        item.pa = offer.ListingPrice;


                                    //Offerリストへ入れる
                                    if (offer.SubCondition == "new")
                                        item.lpn.Add(offer);
                                    else
                                        item.lpu.Add(offer);
                                }

                                dct[strKey] = item;
                            }
                            else
                            {
                            }
                        }
                        else
                        {
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.outputError(ex);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.outputError(ex);
            }
        }

        internal static void getCatalogItemToDictionary(Dictionary<String, String> response, ref Dictionary<String, DataModel.ItemAttribute> dct)
        {
            const Decimal INCHI_TO_CM = (Decimal)2.54;
            const Decimal POUNDS_TO_KG = (Decimal)0.45;

            try
            {
                foreach (String strKey in response.Keys)
                {
                    try
                    {
                        if (dct.Keys.Contains(strKey))
                        {
                            String strXml = response[strKey];
                            if (!String.IsNullOrEmpty(strXml))
                            {
                                XmlDocument objDom = new XmlDocument();
                                objDom.LoadXml(strXml);

                                if (objDom != null)
                                {
                                    DataModel.ItemAttribute item = dct[strKey];

                                    try
                                    {
                                        XmlNode objItem = objDom.DocumentElement;

                                        if (item.packageQty == 0)
                                            int.TryParse(getXmlInnerText(objItem.SelectSingleNode(".//number_of_items/value")), out item.packageQty);

                                        if (item.packageQty == 0)
                                        {
                                            string strUnit = getXmlInnerText(objItem.SelectSingleNode(".//unit_count/type/value"));
                                            String strQty = getXmlInnerText(objItem.SelectSingleNode(".//unit_count/value"));
                                            if (strUnit == "個")
                                            {
                                                int.TryParse(strQty, out item.packageQty);
                                            }
                                            else
                                            {

                                            }
                                        }

                                        if (item.packageQty == 0)
                                            item.packageQty = 1;

                                        item.category = getXmlInnerText(objItem.SelectSingleNode(".//salesRanks/ranks/title"));
                                        int.TryParse(getXmlInnerText(objItem.SelectSingleNode(".//salesRanks/ranks/value")), out item.rank);

                                        item.title = getXmlInnerText(objItem.SelectSingleNode(".//summaries/itemName"));
                                        item.brand = getXmlInnerText(objItem.SelectSingleNode(".//summaries/brandName"));
                                        if (String.IsNullOrEmpty(item.brand))
                                            item.brand = getXmlInnerText(objItem.SelectSingleNode(".//summaries/manufacturer"));

                                        if (objItem.SelectSingleNode(".//item_package_dimensions") != null)
                                        {
                                            item.width = Decimal.Parse(getXmlInnerText(objItem.SelectSingleNode(".//item_package_dimensions/width/value")));
                                            switch (getXmlInnerText(objItem.SelectSingleNode(".//item_package_dimensions/width/unit")))
                                            {
                                                case "centimeters":
                                                    item.width *= 1;
                                                    break;

                                                case "inches":
                                                    item.width = item.width * INCHI_TO_CM;
                                                    break;

                                                case "millimeters":
                                                    item.width /= 10;
                                                    break;

                                                case "meters":
                                                    item.width *= 100;
                                                    break;

                                                default:
                                                    item.width *= 1;
                                                    break;
                                            }

                                            item.width = Math.Ceiling(item.width);


                                            item.height = Decimal.Parse(getXmlInnerText(objItem.SelectSingleNode(".//item_package_dimensions/height/value")));
                                            switch (getXmlInnerText(objItem.SelectSingleNode(".//item_package_dimensions/height/unit")))
                                            {
                                                case "centimeters":
                                                    item.height *= 1;
                                                    break;

                                                case "inches":
                                                    item.height = item.height * INCHI_TO_CM;
                                                    break;

                                                case "millimeters":
                                                    item.height /= 10;
                                                    break;

                                                case "meters":
                                                    item.height *= 100;
                                                    break;

                                                default:
                                                    item.height *= 1;
                                                    break;
                                            }
                                            item.height = Math.Ceiling(item.height);

                                            item.length = Decimal.Parse(getXmlInnerText(objItem.SelectSingleNode(".//item_package_dimensions/length/value")));
                                            switch (getXmlInnerText(objItem.SelectSingleNode(".//item_package_dimensions/length/unit")))
                                            {
                                                case "centimeters":
                                                    item.length *= 1;
                                                    break;

                                                case "inches":
                                                    item.length = item.length * INCHI_TO_CM;
                                                    break;

                                                case "millimeters":
                                                    item.length /= 10;
                                                    break;

                                                case "meters":
                                                    item.length *= 100;
                                                    break;

                                                default:
                                                    item.length *= 1;
                                                    break;
                                            }

                                            item.length = Math.Ceiling(item.length);
                                        }

                                        if (objItem.SelectSingleNode(".//item_package_weight") != null)
                                        {
                                            item.weight = Decimal.Parse(getXmlInnerText(objItem.SelectSingleNode(".//item_package_weight/value")));
                                            switch (getXmlInnerText(objItem.SelectSingleNode(".//item_package_weight/unit")))
                                            {
                                                case "pound":
                                                    item.weight *= POUNDS_TO_KG;
                                                    break;
                                                case "grams":
                                                    item.weight /= 1000;
                                                    break;
                                                default:
                                                    item.weight *= 1;
                                                    break;
                                            }
                                            item.weight = Math.Round(item.weight, 1);
                                        }

                                        item.imgS = getXmlInnerText(objItem.SelectSingleNode(".//images//images/link"));

                                        foreach (XmlNode objEan in objItem.SelectNodes(".//identifiers/identifiers"))
                                        {
                                            if (getXmlInnerText(objEan.SelectSingleNode("./identifierType")) == "EAN")
                                            {
                                                if (String.IsNullOrEmpty(item.ean))
                                                    item.ean = getXmlInnerText(objEan.SelectSingleNode("./identifier"));
                                            }
                                        }

                                        if (int.TryParse(getXmlInnerText(objItem.SelectSingleNode(".//attributes/list_price/value")), out item.pb))
                                        {
                                            if (getXmlInnerText(objItem.SelectSingleNode(".//attributes/list_price/currency")) != "JPY")
                                            {
                                                item.pb = 0;
                                            }
                                        }


                                        /*
                                        if (!String.IsNullOrEmpty(item.brand))
                                            item.brand = getXmlInnerText(objItem.SelectSingleNode(".//AttributeSets/Label"));

                                        if (!String.IsNullOrEmpty(item.brand))
                                            item.brand = getXmlInnerText(objItem.SelectSingleNode(".//AttributeSets/Manufacturer"));

                                        if (!String.IsNullOrEmpty(item.brand))
                                            item.brand = getXmlInnerText(objItem.SelectSingleNode(".//AttributeSets/Publisher"));

                                        if (!String.IsNullOrEmpty(item.brand))
                                            item.brand = getXmlInnerText(objItem.SelectSingleNode(".//AttributeSets/Studio"));

                                        item.imgS = getXmlInnerText(objItem.SelectSingleNode(".//SmallImage/URL"));
                                        item.title = getXmlInnerText(objItem.SelectSingleNode(".//AttributeSets/Title"));
                                        item.model = getXmlInnerText(objItem.SelectSingleNode(".//AttributeSets/Model"));
                                        item.color = getXmlInnerText(objItem.SelectSingleNode(".//AttributeSets/Color"));
                                        item.release = getXmlInnerText(objItem.SelectSingleNode(".//AttributeSets/ReleaseDate"));

                                        int.TryParse(getXmlInnerText(objItem.SelectSingleNode(".//AttributeSets/PackageQuantity")), out item.packageQty);

                                        int.TryParse(getXmlInnerText(objItem.SelectSingleNode(".//AttributeSets/ListPrice/Amount")), out item.pb);
                                        */
                                    }
                                    catch (Exception ex)
                                    {
                                        Log.outputError(ex);
                                    }

                                    String strFunctionName = System.Reflection.MethodBase.GetCurrentMethod().Name;
                                    if (item.dctXml == null)
                                        item.dctXml = new Dictionary<string, string>();

                                    if (item.dctXml.Keys.Contains(strFunctionName))
                                        item.dctXml[strFunctionName] = strXml;
                                    else
                                        item.dctXml.Add(strFunctionName, strXml);

                                    dct[strKey] = item;
                                }
                            }
                            else
                            {
                            }
                        }
                        else
                        {
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.outputError(ex);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.outputError(ex);
            }
        }

        internal static void listCatalogItemsToDictionary(Dictionary<String, String> response, ref Dictionary<String, DataModel.ItemAttribute> dct)
        {
            try
            {
                foreach (String strKey in response.Keys)
                {
                    try
                    {
                        if (dct.Keys.Contains(strKey))
                        {
                            String strXml = response[strKey];
                            if (!String.IsNullOrEmpty(strXml))
                            {
                                XmlDocument objDom = new XmlDocument();
                                objDom.LoadXml(strXml);

                                XmlNodeList objItems = objDom.SelectNodes(".//items");
                                if (objItems.Count > 0)
                                {
                                    int idxRow = 0;
                                    foreach (XmlNode objItem in objItems)
                                    {
                                        DataModel.ItemAttribute item = dct[strKey];

                                        if (strKey.Length == 13)
                                            item.ean = strKey;

                                        item.asin = getXmlInnerText(objItem.SelectSingleNode(".//asin"));

                                        int intQty = 0;
                                        if (int.TryParse(getXmlInnerText(objItem.SelectSingleNode(".//packageQuantity")), out intQty))
                                            item.packageQty = intQty;

                                        item.title = getXmlInnerText(objItem.SelectSingleNode(".//itemName"));

                                        //1Jan : nAsinあった場合の対応
                                        if (idxRow == 0)
                                            dct[strKey] = item;
                                        else
                                            dct.Add(strKey + new string('*', idxRow), item);

                                        idxRow++;
                                    }
                                }
                            }
                            else
                            {
                            }
                        }
                        else
                        {
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.outputError(ex);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.outputError(ex);
            }
        }

        private static void setItemByCatalogv0(XmlNode objItem, ref DataModel.ItemAttribute item)
        {
            const Decimal INCHI_TO_CM = (Decimal)2.54;
            const Decimal POUNDS_TO_KG = (Decimal)0.45;

            try
            {
                item.category = getXmlInnerText(objItem.SelectSingleNode(".//AttributeSets/Binding"));
                item.brand = getXmlInnerText(objItem.SelectSingleNode(".//AttributeSets/Brand"));

                if (!String.IsNullOrEmpty(item.brand))
                    item.brand = getXmlInnerText(objItem.SelectSingleNode(".//AttributeSets/Label"));

                if (!String.IsNullOrEmpty(item.brand))
                    item.brand = getXmlInnerText(objItem.SelectSingleNode(".//AttributeSets/Manufacturer"));

                if (!String.IsNullOrEmpty(item.brand))
                    item.brand = getXmlInnerText(objItem.SelectSingleNode(".//AttributeSets/Publisher"));

                if (!String.IsNullOrEmpty(item.brand))
                    item.brand = getXmlInnerText(objItem.SelectSingleNode(".//AttributeSets/Studio"));

                item.imgS = getXmlInnerText(objItem.SelectSingleNode(".//SmallImage/URL"));
                item.title = getXmlInnerText(objItem.SelectSingleNode(".//AttributeSets/Title"));
                item.model = getXmlInnerText(objItem.SelectSingleNode(".//AttributeSets/Model"));
                item.color = getXmlInnerText(objItem.SelectSingleNode(".//AttributeSets/Color"));
                item.release = getXmlInnerText(objItem.SelectSingleNode(".//AttributeSets/ReleaseDate"));

                int.TryParse(getXmlInnerText(objItem.SelectSingleNode(".//AttributeSets/PackageQuantity")), out item.packageQty);

                if (item.packageQty != 1)
                {
                }

                int.TryParse(getXmlInnerText(objItem.SelectSingleNode(".//AttributeSets/ListPrice/Amount")), out item.pb);
                int.TryParse(getXmlInnerText(objItem.SelectSingleNode(".//SalesRankings/Rank")), out item.rank);

                Decimal.TryParse(getXmlInnerText(objItem.SelectSingleNode(".//AttributeSets/PackageDimensions/Width/value")), out item.width);
                Decimal.TryParse(getXmlInnerText(objItem.SelectSingleNode(".//AttributeSets/PackageDimensions/Height/value")), out item.height);
                Decimal.TryParse(getXmlInnerText(objItem.SelectSingleNode(".//AttributeSets/PackageDimensions/Length/value")), out item.length);
                Decimal.TryParse(getXmlInnerText(objItem.SelectSingleNode(".//AttributeSets/PackageDimensions/Weight/value")), out item.weight);
                item.width *= INCHI_TO_CM;
                item.height *= INCHI_TO_CM;
                item.length *= INCHI_TO_CM;
                item.weight *= POUNDS_TO_KG;
                item.width = Math.Round(item.width, 0);
                item.height = Math.Round(item.height, 0);
                item.length = Math.Round(item.length, 0);
                item.weight = Math.Round(item.weight, 1);
            }
            catch (Exception ex)
            {
                Log.outputError(ex);
            }
        }

        internal static void getListingOffersToDictionary(Dictionary<String, String> response, ref Dictionary<String, DataModel.ItemAttribute> dct)
        {
            try
            {
                foreach (String strKey in response.Keys)
                {
                    try
                    {
                        if (dct.Keys.Contains(strKey))
                        {
                            String strJson = response[strKey];
                            if (!String.IsNullOrEmpty(strJson))
                            {
                                String strXml = response[strKey];
                                if (!String.IsNullOrEmpty(strXml))
                                {
                                    DataModel.ItemAttribute item = dct[strKey];

                                    XmlDocument objDom = new XmlDocument();
                                    objDom.LoadXml(strXml);

                                    XmlNodeList objLowestPrices = objDom.SelectNodes(".//payload/Summary/LowestPrices");
                                    XmlNodeList objBuyBoxPrices = objDom.SelectNodes(".//payload/Summary/BuyBoxPrices");
                                    XmlNodeList objNumberOfOffers = objDom.SelectNodes(".//payload/Summary/NumberOfOffers");
                                    XmlNodeList objBuyBoxEligibleOffers = objDom.SelectNodes(".//payload/Summary/BuyBoxEligibleOffers");
                                    XmlNodeList objOffers = objDom.SelectNodes(".//payload/Offers");

                                    //最安値
                                    foreach (XmlNode objLowestPrice in objLowestPrices)
                                    {
                                        XmlNode objCondition = objLowestPrice.SelectSingleNode("./condition");
                                        XmlNode objFulfillmentChannel = objLowestPrice.SelectSingleNode("./fulfillmentChannel");
                                        int intListingPrice = 0;
                                        int intShippingPrice = 0;
                                        int intPoint = 0;

                                        int.TryParse(getXmlInnerText(objLowestPrice.SelectSingleNode("./ListingPrice/Amount")), out intListingPrice);
                                        int.TryParse(getXmlInnerText(objLowestPrice.SelectSingleNode("./Shipping/Amount")), out intShippingPrice);
                                        int.TryParse(getXmlInnerText(objLowestPrice.SelectSingleNode("./Points/Amount")), out intPoint);
                                        switch (getXmlInnerText(objCondition) + "_" + getXmlInnerText(objFulfillmentChannel))
                                        {
                                            case "new_Amazon":
                                                item.pna = intListingPrice + intShippingPrice;
                                                item.pna_s = intShippingPrice;
                                                item.pna_p = intPoint;
                                                if (item.pn == 0)
                                                {
                                                    item.pn = intListingPrice + intShippingPrice;
                                                    item.pn_s = intShippingPrice;
                                                    item.pn_p = intPoint;
                                                }
                                                break;

                                            case "new_Merchant":
                                                item.pnm = intListingPrice + intShippingPrice;
                                                item.pnm_s = intShippingPrice;
                                                item.pnm_p = intPoint;
                                                if (item.pn == 0)
                                                {
                                                    item.pn = intListingPrice + intShippingPrice;
                                                    item.pn_s = intShippingPrice;
                                                    item.pn_p = intPoint;
                                                }
                                                break;

                                            case "used_Amazon":
                                                item.pua = intListingPrice + intShippingPrice;
                                                item.pua_s = intShippingPrice;
                                                item.pua_p = intPoint;
                                                if (item.pu == 0)
                                                {
                                                    item.pu = intListingPrice + intShippingPrice;
                                                    item.pu_s = intShippingPrice;
                                                    item.pu_p = intPoint;
                                                }
                                                break;

                                            case "used_Merchant":
                                                item.pum = intListingPrice + intShippingPrice;
                                                item.pum_s = intShippingPrice;
                                                item.pum_p = intPoint;
                                                if (item.pu == 0)
                                                {
                                                    item.pu = intListingPrice + intShippingPrice;
                                                    item.pu_s = intShippingPrice;
                                                    item.pu_p = intPoint;
                                                }
                                                break;
                                        }
                                    }

                                    //カート価格
                                    foreach (XmlNode objBuyBoxPrice in objBuyBoxPrices)
                                    {
                                        XmlNode objCondition = objBuyBoxPrice.SelectSingleNode("./condition");
                                        int intListingPrice = 0;
                                        int intShippingPrice = 0;
                                        int intPoint = 0;

                                        int.TryParse(getXmlInnerText(objBuyBoxPrice.SelectSingleNode("./ListingPrice/Amount")), out intListingPrice);
                                        int.TryParse(getXmlInnerText(objBuyBoxPrice.SelectSingleNode("./Shipping/Amount")), out intShippingPrice);
                                        int.TryParse(getXmlInnerText(objBuyBoxPrice.SelectSingleNode("./Points/Amount")), out intPoint);
                                        switch (getXmlInnerText(objCondition))
                                        {
                                            case "new":
                                                item.pc = intListingPrice + intShippingPrice;
                                                item.pc_s = intShippingPrice;
                                                item.pc_p = intPoint;
                                                if (item.pn == 0)
                                                {
                                                    item.pn = intListingPrice + intShippingPrice;
                                                    item.pn_s = intShippingPrice;
                                                    item.pn_p = intPoint;
                                                }
                                                break;

                                            case "used":
                                                break;
                                        }
                                    }

                                    //出品者数
                                    foreach (XmlNode objNumberOfOffer in objNumberOfOffers)
                                    {
                                        XmlNode objCondition = objNumberOfOffer.SelectSingleNode("./condition");
                                        XmlNode objFulfillmentChannel = objNumberOfOffer.SelectSingleNode("./fulfillmentChannel");
                                        int intQ = 0;

                                        int.TryParse(getXmlInnerText(objNumberOfOffer.SelectSingleNode("./OfferCount")), out intQ);
                                        switch (getXmlInnerText(objCondition) + "_" + getXmlInnerText(objFulfillmentChannel))
                                        {
                                            case "new_Amazon":
                                                item.qna = intQ;
                                                break;

                                            case "new_Merchant":
                                                item.qnm = intQ;
                                                break;

                                            case "used_Amazon":
                                                item.qua = intQ;
                                                break;

                                            case "used_Merchant":
                                                item.qum = intQ;
                                                break;
                                        }
                                    }

                                    //カート出品者数
                                    foreach (XmlNode objBuyBoxEligibleOffer in objBuyBoxEligibleOffers)
                                    {
                                        XmlNode objCondition = objBuyBoxEligibleOffer.SelectSingleNode("./condition");
                                        XmlNode objFulfillmentChannel = objBuyBoxEligibleOffer.SelectSingleNode("./fulfillmentChannel");
                                        int intQ = 0;

                                        int.TryParse(getXmlInnerText(objBuyBoxEligibleOffer.SelectSingleNode("./OfferCount")), out intQ);
                                        switch (getXmlInnerText(objCondition) + "_" + getXmlInnerText(objFulfillmentChannel))
                                        {
                                            case "new_Amazon":
                                                item.qca = intQ;
                                                break;

                                            case "new_Merchant":
                                                item.qcm = intQ;
                                                break;

                                            case "used_Amazon":
                                                break;

                                            case "used_Merchant":
                                                break;
                                        }
                                    }

                                    //価格リスト
                                    if (item.lpn == null)
                                        item.lpn = new List<DataModel.Offer>();

                                    if (item.lpu == null)
                                        item.lpu = new List<DataModel.Offer>();

                                    foreach (XmlNode objOffer in objOffers)
                                    {
                                        DataModel.Offer offer = new DataModel.Offer();
                                        int.TryParse(getXmlInnerText(objOffer.SelectSingleNode("./Shipping/Amount")), out offer.Shipping);
                                        int.TryParse(getXmlInnerText(objOffer.SelectSingleNode("./ListingPrice/Amount")), out offer.ListingPrice);
                                        int.TryParse(getXmlInnerText(objOffer.SelectSingleNode("./ShippingTime/maximumHours")), out offer.maximumHours);
                                        int.TryParse(getXmlInnerText(objOffer.SelectSingleNode("./ShippingTime/minimumHours")), out offer.minimumHours);
                                        offer.availabilityType = getXmlInnerText(objOffer.SelectSingleNode("./ShippingTime/availabilityType"));
                                        int.TryParse(getXmlInnerText(objOffer.SelectSingleNode("./SellerFeedbackRating/FeedbackCount")), out offer.FeedbackCount);
                                        int.TryParse(getXmlInnerText(objOffer.SelectSingleNode("./SellerFeedbackRating/SellerFeedbackRating")), out offer.SellerFeedbackRating);
                                        offer.SubCondition = getXmlInnerText(objOffer.SelectSingleNode("./SubCondition"));
                                        offer.ShipsFromCountry = getXmlInnerText(objOffer.SelectSingleNode("./ShipsFrom/Country"));
                                        offer.SubCondition = getXmlInnerText(objOffer.SelectSingleNode("./SubCondition"));
                                        offer.IsBuyBoxWinner = getXmlInnerText(objOffer.SelectSingleNode("./IsBuyBoxWinner"));
                                        offer.IsFeaturedMerchant = getXmlInnerText(objOffer.SelectSingleNode("./IsFeaturedMerchant"));
                                        offer.IsFulfilledByAmazon = getXmlInnerText(objOffer.SelectSingleNode("./IsFulfilledByAmazon"));
                                        offer.SellerId = getXmlInnerText(objOffer.SelectSingleNode("./SellerId"));
                                        offer.ConditionNotes = getXmlInnerText(objOffer.SelectSingleNode("./ConditionNotes"));


                                        //Amazon.co.jp本体価格
                                        if (offer.SellerId == SELLERID_AMAZON)
                                            item.pa = offer.ListingPrice;


                                        //Offerリストへ入れる
                                        if (offer.SubCondition == "new")
                                            item.lpn.Add(offer);
                                        else
                                            item.lpu.Add(offer);
                                    }

                                    dct[strKey] = item;
                                }
                                else
                                {
                                }
                            }
                            else
                            {
                            }
                        }
                        else
                        {
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.outputError(ex);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.outputError(ex);
            }
        }

        internal static void getPricingToDictionary(Dictionary<String, String> response, ref Dictionary<String, DataModel.ItemAttribute> dct)
        {
            try
            {
                foreach (String strKey in response.Keys)
                {
                    String strXml = response[strKey];
                    XmlDocument objDom = new XmlDocument();
                    objDom.LoadXml(strXml);

                    foreach (XmlNode objItem in objDom.SelectNodes(".//Product"))
                    {
                        String strAsin = getXmlInnerText(objItem.SelectSingleNode("./Identifiers/MarketplaceASIN/ASIN"));
                        if (dct.Keys.Contains(strAsin))
                        {
                            DataModel.ItemAttribute item = dct[strAsin];
                            item.sku = getXmlInnerText(objItem.SelectSingleNode("./Offers/SellerSKU"));
                            switch (getXmlInnerText(objItem.SelectSingleNode("./Offers/ItemCondition"))){
                                case "New":
                                    int.TryParse(getXmlInnerText(objItem.SelectSingleNode("./RegularPrice/Amount")), out item.pn);
                                    break;

                                case "Used":
                                    int.TryParse(getXmlInnerText(objItem.SelectSingleNode("./RegularPrice/Amount")), out item.pu);
                                    break;
                            }
                            dct[strAsin] = item;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.outputError(ex);
            }
            //自出品物があればレスポンスが空でなくなる
            /*
            {
                "payload": [
                    {
                        "status": "Success",
                        "ASIN": "B07MDFWHN4",
                        "Product": {
                            "Identifiers": {
                                "MarketplaceASIN": {
                                    "MarketplaceId": "A1VC38T7YXB528",
                                    "ASIN": "B07MDFWHN4"
                                }
                            },
                            "Offers": [
                                {
                                    "BuyingPrice": {
                                        "ListingPrice": {
                                            "CurrencyCode": "JPY",
                                            "Amount": 6470.00
                                        },
                                        "LandedPrice": {
                                            "CurrencyCode": "JPY",
                                            "Amount": 6470.00
                                        },
                                        "Shipping": {
                                            "CurrencyCode": "JPY",
                                            "Amount": 0.00
                                        }
                                    },
                                    "RegularPrice": {
                                        "CurrencyCode": "JPY",
                                        "Amount": 6470.00
                                    },
                                    "FulfillmentChannel": "AMAZON",
                                    "ItemCondition": "New",
                                    "ItemSubCondition": "New",
                                    "SellerSKU": "210508-4901301366078-6400-3280-17"
                                }
                            ]
                        }
                    }
                ]
            }
             */
        }

        internal static void getCompetitivePricingToDictionary(Dictionary<String, String> response, ref Dictionary<String, DataModel.ItemAttribute> dct)
        {
            try
            {
                foreach (String strKey in response.Keys)
                {
                    try
                    {
                        String strXml = response[strKey];

                        if (!String.IsNullOrEmpty(strXml))
                        {
                            XmlDocument objDom = new XmlDocument();
                            objDom.LoadXml(strXml);
                            XmlNodeList objItems = objDom.SelectNodes(".//payload");
                            if (objItems.Count > 0)
                            {
                                foreach (XmlNode objItem in objItems)
                                {
                                    String strAsin = getXmlInnerText(objItem.SelectSingleNode("./ASIN"));
                                    if (dct.Keys.Contains(strAsin))
                                    {
                                        DataModel.ItemAttribute item = dct[strAsin];

                                        int intPc = 0;
                                        int intPc_s = 0;
                                        if (int.TryParse(getXmlInnerText(objItem.SelectSingleNode(".//Product/CompetitivePricing/CompetitivePrices/Price/ListingPrice/Amount")), out intPc) &&
                                            int.TryParse(getXmlInnerText(objItem.SelectSingleNode(".//Product/CompetitivePricing/CompetitivePrices/Price/Shipping/Amount")), out intPc_s))
                                            item.pc = intPc + intPc_s;

                                        item.qc = objItem.SelectNodes(".//Product/CompetitivePricing/CompetitivePrices").Count;

                                        foreach (XmlNode objNumberOfOfferListings in objItem.SelectNodes(".//CompetitivePricing/NumberOfOfferListings"))
                                        {
                                            int intQ = 0;
                                            switch (getXmlInnerText(objNumberOfOfferListings.SelectSingleNode(".//condition")))
                                            {
                                                case "New":
                                                    if (int.TryParse(getXmlInnerText(objNumberOfOfferListings.SelectSingleNode("./Count")), out intQ))
                                                        item.qn = intQ;
                                                    break;

                                                case "Used":
                                                    if (int.TryParse(getXmlInnerText(objNumberOfOfferListings.SelectSingleNode("./Count")), out intQ))
                                                        item.qu = intQ;
                                                    break;
                                            }
                                        }

                                        dct[strAsin] = item;
                                    }
                                    else
                                    {
                                    }
                                }
                            }
                        }
                        else
                        {
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.outputError(ex);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.outputError(ex);
            }
        }

        internal static void getMyFeesEstimateForASINToDictionary(Dictionary<String, String> response, ref Dictionary<String, DataModel.ItemAttribute> dct)
        {
            try
            {
                foreach (String strAsin in response.Keys)
                {
                    try
                    {
                        if (dct.Keys.Contains(strAsin))
                        {
                            String strXml = response[strAsin];
                            if (!String.IsNullOrEmpty(strXml))
                            {
                                XmlDocument objDom = new XmlDocument();
                                objDom.LoadXml(strXml);

                                DataModel.ItemAttribute item = dct[strAsin];

                                int intPriceSale = 0;
                                if (int.TryParse(getXmlInnerText(objDom.SelectSingleNode(".//payload/FeesEstimateResult/FeesEstimateIdentifier/PriceToEstimateFees/ListingPrice/Amount")), out intPriceSale))
                                {
                                    XmlNodeList objFeeDetailLists = objDom.SelectNodes(".//payload/FeesEstimateResult/FeesEstimate/FeeDetailList");
                                    foreach (XmlNode objFeeDetailList in objFeeDetailLists)
                                    {
                                        int intFee = 0;
                                        switch (getXmlInnerText(objFeeDetailList.SelectSingleNode("./FeeType")))
                                        {
                                            case "ReferralFee":
                                                if (int.TryParse(getXmlInnerText(objFeeDetailList.SelectSingleNode("./FinalFee/Amount")), out intFee))
                                                    item.feeSales = (intFee * 100) / (Decimal)intPriceSale;
                                                break;

                                            case "VariableClosingFee":
                                                if (int.TryParse(getXmlInnerText(objFeeDetailList.SelectSingleNode("./FinalFee/Amount")), out intFee))
                                                    item.feeCategory = intFee;
                                                break;

                                            case "PerItemFee":
                                                //if (int.TryParse(getXmlInnerText(objFeeDetailList.SelectSingleNode("./FinalFee/Amount")), out intFee))
                                                //    item.feeSales = intFee * 100 / intPriceSale;
                                                break;

                                            case "FBAFees":
                                                if (int.TryParse(getXmlInnerText(objFeeDetailList.SelectSingleNode("./FinalFee/Amount")), out intFee))
                                                    item.feeFBA = intFee;
                                                break;
                                        }
                                    }
                                }

                                dct[strAsin] = item;
                            }
                            else
                            {
                            }
                        }
                        else
                        {
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.outputError(ex);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.outputError(ex);
            }
        }

        internal static void getMyFeesEstimateForSKUToDictionary(Dictionary<String, String> response, ref Dictionary<String, DataModel.ItemAttribute> dct)
        {

        }

        internal static void getOrderToDictionary(String response, ref Dictionary<String, DataModel.ItemAttribute> dct)
        {

        }

        internal static void getOrdersToDictionary(String response, ref Dictionary<String, DataModel.ItemAttribute> dct)
        {

        }

        internal static void getOrderItemsToDictionary(String response, ref Dictionary<String, DataModel.ItemAttribute> dct)
        {

        }

        internal static void getOrderItemsBuyerInfoToDictionary(String response, ref Dictionary<String, DataModel.ItemAttribute> dct)
        {

        }

        internal static void getOrderAddressToDictionary(String response, ref Dictionary<String, DataModel.ItemAttribute> dct)
        {

        }

        internal static void getOrderBuyerInfoToDictionary(String response, ref Dictionary<String, DataModel.ItemAttribute> dct)
        {

        }

        internal static void listFinancialEventGroupsToDictionary(String response, ref Dictionary<String, DataModel.ItemAttribute> dct)
        {

        }

        internal static void listFinancialEventsByGroupIdToDictionary(String response, ref Dictionary<String, DataModel.ItemAttribute> dct)
        {

        }

        internal static void listFinancialEventsByOrderIdToDictionary(String response, ref Dictionary<String, DataModel.ItemAttribute> dct)
        {

        }

        internal static void listFinancialEventsToDictionary(String response, ref Dictionary<String, DataModel.ItemAttribute> dct)
        {

        }

        internal static Dictionary<String, DataModel.ItemAttribute> convertDictionaryJan2Asin(Dictionary<String, DataModel.ItemAttribute> dct)
        {
            Dictionary<String, DataModel.ItemAttribute> dctRet = new Dictionary<string, DataModel.ItemAttribute>();
            try
            {
                //ASINベースのDictionaryを作成
                foreach (DataModel.ItemAttribute item in dct.Values)
                {
                    if (!String.IsNullOrEmpty(item.asin))
                    {
                        if (!dctRet.Keys.Contains(item.asin))
                            dctRet.Add(item.asin, item);

                    }
                }
            }
            catch (Exception ex)
            {
            }
            return dctRet;
        }
    }
}

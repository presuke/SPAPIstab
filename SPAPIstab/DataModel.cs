using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPAPIstab
{
    class DataModel
    {
        internal struct ItemAttribute
        {
            internal String asin;   //ASINコード
            internal String ean;    //JANコード
            internal String sku;    //SKU
            internal String title;  //商品名
            internal String category;   //商品カテゴリ
            internal String brand;  //ブランド
            internal String model;  //型式
            internal String color;  //色
            internal int rank;  //ランキング

            internal int packageQty;    //入数

            internal int pb;    //定価
            internal int pc;    //カート価格
            internal int pc_s;  //カート価格送料
            internal int pc_p;  //カート価格ポイント
            internal int pa;    //amazon価格

            internal int pn;    //新品最安値
            internal int pn_s;  //新品最安値送料
            internal int pn_p;  //新品最安値ポイント
            internal int pna;   //新品FBA最安値
            internal int pna_s;   //新品FBA最安値送料
            internal int pna_p;   //新品FBA最安値ポイント
            internal int pnm;   //新品自己発最安値
            internal int pnm_s; //新品自己発最安値送料
            internal int pnm_p; //新品自己発最安値ポイント

            internal int pu;    //中古最安値
            internal int pu_s;  //中古最安値送料
            internal int pu_p;  //中古最安値ポイント
            internal int pua;   //中古FBA最安値
            internal int pua_s;   //中古FBA最安値送料
            internal int pua_p;   //中古FBA最安値ポイント
            internal int pum;   //中古自己発最安値
            internal int pum_s; //中古自己発最安値送料
            internal int pum_p; //中古自己発最安値ポイント

            internal int qn;    //新品出品者数
            internal int qna;   //？新品FBA出品者数
            internal int qnm;   //？新品自己発出品者数

            internal int qu;    //中古出品者数
            internal int qua;   //？中古FBA出品者数
            internal int qum;   //？中古自己発出品者数

            internal int qc;    //？カート出品者数
            internal int qca;   //？カートFBA出品者数
            internal int qcm;   //？カート自己発出品者数


            internal int feeFBA;    //FBA配送手数料
            internal Decimal feeSales;  //販売手数料率（％）
            internal int feeCategory;   //カテゴリ成約料

            internal String release;    //販売開始年月日
            internal String imgS;   //商品画像（URL）
            internal Decimal weight;    //重さ
            internal Decimal height;    //高さ
            internal Decimal width; //幅
            internal Decimal length;    //奥行き

            internal List<Offer> lpn;   //新品出品リスト
            internal List<Offer> lpu;   //中古出品リスト

            internal Dictionary<String, String> dctXml; //APIレスポンス格納用Map
        }

        internal struct Offer
        {
            internal int ListingPrice;
            internal int Shipping;
            internal int maximumHours;
            internal int minimumHours;
            internal String availabilityType;
            internal int FeedbackCount;
            internal int SellerFeedbackRating;
            internal String ShipsFromCountry;
            internal String SubCondition;
            internal String IsFeaturedMerchant;
            internal String IsBuyBoxWinner;
            internal String IsFulfilledByAmazon;
            internal String SellerId;
            internal String ConditionNotes;
        }
    }
}

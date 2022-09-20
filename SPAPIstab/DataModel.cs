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
            internal int No;
            internal int rowidx;
            internal String asin;
            internal String asinByEan;
            internal String asinVariation;
            internal String asinFrequentlyBoughtTogether;
            internal String ean;
            internal String storecode;
            internal String title;
            internal String category;
            internal String subCategory;
            internal String brand;
            internal String model;
            internal String color;
            internal int rank;
            internal int rankAvg3;
            internal int rankAvg6;
            internal int rankAvg12;
            internal int rankCnt3;
            internal int rankCnt6;
            internal int rankCnt12;
            internal int rankUp3;
            internal int rankUp6;
            internal int rankUp12;

            internal int packageQty;

            internal Decimal rate_premium;

            internal int pb;
            internal int pc;
            internal int pc_s;
            internal int pc_p;
            internal int pa;
            internal int paLast;
            internal String paLastDate;
            internal int paMin;
            internal String paMinDate;
            internal int paMax;
            internal String paMaxDate;
            internal int paHighestOn1Y;
            internal int paHighestOn2Y;
            internal int paLowestOn1Y;
            internal int paLowestOn2Y;
            internal DateTime dtPaHighestOn1Y;
            internal DateTime dtPaHighestOn2Y;
            internal DateTime dtPaLowestOn1Y;
            internal DateTime dtPaLowestOn2Y;
            internal int paCntStockLess;
            internal TimeSpan paTotalTimeStockLess;
            internal TimeSpan paTotalTimeStock;
            internal int paAvg3;
            internal int paAvg6;
            internal int paAvg12;
            internal int paMin3;
            internal int paMin6;
            internal int paMin12;
            internal int paMax3;
            internal int paMax6;
            internal int paMax12;

            internal int pn;
            internal int pn_s;
            internal int pn_p;
            internal int pna;
            internal int pna_s;
            internal int pna_p;
            internal int pnm;
            internal int pnm_s;
            internal int pnm_p;
            internal int pnAvg3;
            internal int pnAvg6;
            internal int pnAvg12;
            internal int pnMin3;
            internal int pnMin6;
            internal int pnMin12;
            internal int pnMax3;
            internal int pnMax6;
            internal int pnMax12;
            internal int pnCnt3;
            internal int pnCnt6;
            internal int pnCnt12;

            internal int pu;
            internal int pu_s;
            internal int pu_p;
            internal int pua;
            internal int pua_s;
            internal int pua_p;
            internal int pum;
            internal int pum_s;
            internal int pum_p;
            internal int pu_good;
            internal String pu_good_condition;
            internal int puAvg3;
            internal int puAvg6;
            internal int puAvg12;
            internal int puMin3;
            internal int puMin6;
            internal int puMin12;
            internal int puMax3;
            internal int puMax6;
            internal int puMax12;
            internal int puCnt3;
            internal int puCnt6;
            internal int puCnt12;

            internal int pm;
            internal int pm_s;
            internal int qa;
            internal int qn;
            internal int qna;
            internal int qnm;
            internal Decimal qnAvg3;
            internal Decimal qnAvg6;
            internal Decimal qnAvg12;
            internal int qnMin3;
            internal int qnMin6;
            internal int qnMin12;
            internal int qnMax3;
            internal int qnMax6;
            internal int qnMax12;
            internal int qnCnt3;
            internal int qnCnt6;
            internal int qnCnt12;

            internal int qu;
            internal int qua;
            internal int qum;
            internal Decimal quAvg3;
            internal Decimal quAvg6;
            internal Decimal quAvg12;
            internal int quMin3;
            internal int quMin6;
            internal int quMin12;
            internal int quMax3;
            internal int quMax6;
            internal int quMax12;
            internal int quCnt3;
            internal int quCnt6;
            internal int quCnt12;

            internal int qc;
            internal int qca;
            internal int qcm;

            internal int smin;
            internal int smax;

            internal int feeFBA;
            internal int feeStockFBA;
            internal Decimal feeSales;
            internal int feeCategory;

            internal int shiiretargetcnt;
            internal String sizeKbn;
            internal String release;
            internal String imgL;
            internal String imgS;
            internal Decimal weight;
            internal Decimal height;
            internal Decimal width;
            internal Decimal length;
            internal DateTime timestamp;

            internal List<Offer> lpn;
            internal List<Offer> lpu;

            internal int recieveSts;
            internal String debugMsg;
            internal String stockMessage;

            internal int duplicate;

            internal Dictionary<String, String> dctXml;
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

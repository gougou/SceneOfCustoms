﻿﻿using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using SceneOfCustoms.Common;
using SceneOfCustoms.Models;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Messaging;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Linq;

namespace SceneOfCustoms.Controllers
{
    public class WumaoController : Controller
    {
        // GET: Wumao

        public ActionResult Wumao_List()
        {
            ViewData["crumb"] = "数据";
            return View();
        }

        public ActionResult DataMatching_List()
        {
            ViewData["crumb"] = "物贸通数据匹配";
            return View();
        }

        public ActionResult DataMatching_Edit()
        {
            ViewData["ID"] = Request["ID"];
            ViewData["crumb"] = "物贸通数据修改";
            return View();
        }


        public ActionResult List_WuMaoBack()
        {
            ViewData["crumb"] = "物贸通返回";
            return View();
        }


        public string LoadDataMatching_Edit()
        {
            string ID = Request.QueryString["ID"];
            string sql = "SELECT * FROM LIST_WUMAODADAMATCHING WHERE ID = '" + ID + "'";
            DataTable dt = DBMgr.GetDataTable(sql);
            IsoDateTimeConverter iso = new IsoDateTimeConverter();//序列化JSON对象时,日期的处理格式
            iso.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
            string result = JsonConvert.SerializeObject(dt, iso);
            result = result.Substring(1, result.Length - 1);
            result = result.Substring(0, result.Length - 1);
            return result;
        }

        public string SyncData1()
        {
            string ORDERCODE = "SI161100014";

            string sql = " select * from LIST_WUMAO where ordercode ='" + ORDERCODE + "'";// and  APPCIQID is null
            DataTable dt = DBMgr.GetDataTable(sql);
            string sql1 = " select * from LIST_DECLARATION where ordercode ='" + ORDERCODE + "' and  ISDEL !='1'";
            DataTable dt1 = DBMgr.GetDataTable(sql1);

            if (dt.Rows.Count > 0 && dt1.Rows.Count > 0)
            {
                XmlDocument xmlDoc = new XmlDocument();
                //string path = @"D:/net/WINFORM/JiShi_WinForm/tem/tem.xml";
                string path = Server.MapPath("/tem/tem1.xml");
                xmlDoc.Load(path);
                XmlElement node;

                node = (XmlElement)xmlDoc.SelectSingleNode("ENTRY_RESULT/EportNo");
                node.InnerText = dt1.Rows[0]["ORDERCODE"] + "";


                node = (XmlElement)xmlDoc.SelectSingleNode("ENTRY_RESULT/ENTRYNO");
                node.InnerText = dt1.Rows[0]["DECLARATIONCODE"] + "";

                node = (XmlElement)xmlDoc.SelectSingleNode("ENTRY_RESULT/NoticeDate");
                //node.InnerText = dt.Rows[0]["D_DATE"] + "";
                node.InnerText = DateTime.Now.ToLocalTime().ToString("yyyyMMddTHHmmss");

                node = (XmlElement)xmlDoc.SelectSingleNode("ENTRY_RESULT/Channel");
                node.InnerText = "011";//007

                node = (XmlElement)xmlDoc.SelectSingleNode("ENTRY_RESULT/EportLocationCode");
                node.InnerText = "2358";

                node = (XmlElement)xmlDoc.SelectSingleNode("ENTRY_RESULT/ImportExportDate");
                node.InnerText = "T00000000";

                node = (XmlElement)xmlDoc.SelectSingleNode("ENTRY_RESULT/Packages");
                node.InnerText = "3";

                node = (XmlElement)xmlDoc.SelectSingleNode("ENTRY_RESULT/NetWeight");
                node.InnerText = "87.5340";

                node = (XmlElement)xmlDoc.SelectSingleNode("ENTRY_RESULT/GrossWeight");
                node.InnerText = "132.90";


                node = (XmlElement)xmlDoc.SelectSingleNode("ENTRY_RESULT/CorporationName");
                node.InnerText = dt.Rows[0]["APPCOMPANY"] + "";

                node = (XmlElement)xmlDoc.SelectSingleNode("ENTRY_RESULT/CorporationCustomsCode");
                node.InnerText = dt.Rows[0]["TRADE_CODE"] + "";


                node = (XmlElement)xmlDoc.SelectSingleNode("ENTRY_RESULT/ResultInformation");
                node.InnerText = "海关H2000入库";//申报成功

                xmlDoc.Save(path);
                //MessageQueue mq = new MessageQueue("FormatName:DIRECT=TCP:221.224.206.245\\Private$\\DataCenter_SZ");
                MessageQueue mq = new MessageQueue("FormatName:DIRECT=TCP:58.210.121.35\\Private$\\DataCenter_KS");
                System.Messaging.Message msg = new System.Messaging.Message();
                ////ZYDFL_S_系统名称_十个0_十个0_企业内部编号_GUID.xml
                string guid = Guid.NewGuid().ToString();
                string time = DateTime.Now.ToLocalTime().ToString("yyyyMMddHHmmssfff");
                //string Label = "QP_R_FL_" + dt.Rows[0]["TRADE_CODE"] + "_" + dt.Rows[0]["APPCOMPANY"] + "_" + dt.Rows[0]["ORDERCODE"] + "_" + time + "_" + guid + ".xml";
                //APPCOMPANY TRADE_CODE
                string Label = "QP_R_FL_" + dt.Rows[0]["APPCOMPANY"] + "_0000000000_" + dt.Rows[0]["ORDERCODE"] + "_" + time + "_" + guid + ".xml";

                if (!string.IsNullOrEmpty(dt1.Rows[0]["DECLARATIONCODE"] + ""))
                {
                    using (FileStream fstream = new FileStream(path, FileMode.Open))
                    {
                        msg.BodyStream = fstream;
                        msg.Label = Label;
                        mq.Send(msg, MessageQueueTransactionType.Single);
                    }
                    sql = " update LIST_WUMAO set APPCIQID='" + dt1.Rows[0]["DECLARATIONCODE"] + "' where  ordercode ='" + dt1.Rows[0]["ORDERCODE"] + "' ";
                    DBMgr.ExecuteNonQuery(sql);
                }
            }
            return "1";
        }

        public string SyncData()
        {
            string ORDERCODE = Request.Form["ORDERCODE"];
            string sql = "SELECT * FROM LIST_WUMAO WHERE ORDERCODE = '" + ORDERCODE + "'";
            DataTable dt = DBMgr.GetDataTable(sql);
            XmlDocument xmlDoc = new XmlDocument();
            string path = Server.MapPath("/tem/tem.xml");
            xmlDoc.Load(path);
            XmlElement node;
            if (dt.Rows.Count > 0)
            {
                node = (XmlElement)xmlDoc.SelectSingleNode("PASS_HEAD/GATEPASS_NO");
                node.InnerText = dt.Rows[0]["GATEPASS_NO"] + "";
                node = (XmlElement)xmlDoc.SelectSingleNode("PASS_HEAD/CORP_NO");
                node.InnerText = dt.Rows[0]["ORDERCODE"] + "";


                node = (XmlElement)xmlDoc.SelectSingleNode("PASS_HEAD/GOODS_NATURE_ID");
                node.InnerText = dt.Rows[0]["GOODS_NATURE_ID"] + "";


                node = (XmlElement)xmlDoc.SelectSingleNode("PASS_HEAD/PROVIDER_NAME");
                node.InnerText = dt.Rows[0]["PROVIDER_NAME"] + "";

                //if(){

                //DateTime.ParseExact(o[0].CREATETIME, "yyyyMMddHHmmss.fffffff", System.Globalization.CultureInfo.CurrentCulture).ToString("yyyy-MM-dd HH:mm:ss")
                //}
                node = (XmlElement)xmlDoc.SelectSingleNode("PASS_HEAD/D_DATE");
                //node.InnerText = dt.Rows[0]["D_DATE"] + "";
                node.InnerText = DateTime.Now.ToLocalTime().ToString("yyyy-MM-dd");

                node = (XmlElement)xmlDoc.SelectSingleNode("PASS_HEAD/I_E_FALG_TYPE");
                node.InnerText = dt.Rows[0]["I_E_FALG_TYPE"] + "";


                node = (XmlElement)xmlDoc.SelectSingleNode("PASS_HEAD/TRANSPORT_CODE");
                node.InnerText = dt.Rows[0]["TRANSPORT_CODE"] + "";

                node = (XmlElement)xmlDoc.SelectSingleNode("PASS_HEAD/TRANSPORT_NAME");
                node.InnerText = dt.Rows[0]["TRANSPORT_NAME"] + "";

                node = (XmlElement)xmlDoc.SelectSingleNode("PASS_HEAD/APPCOMPANY");
                node.InnerText = dt.Rows[0]["APPCOMPANY"] + "";


                node = (XmlElement)xmlDoc.SelectSingleNode("PASS_HEAD/APPCOMPANY_NAME");
                node.InnerText = dt.Rows[0]["APPCOMPANY_NAME"] + "";

                node = (XmlElement)xmlDoc.SelectSingleNode("PASS_HEAD/APPCIQID");
                node.InnerText = dt.Rows[0]["APPCIQID"] + "";

                node = (XmlElement)xmlDoc.SelectSingleNode("PASS_HEAD/BIZ_TYPE_ID");
                node.InnerText = dt.Rows[0]["BIZ_TYPE_ID"] + "";

                node = (XmlElement)xmlDoc.SelectSingleNode("PASS_HEAD/BIZ_TYPE_NAME");
                node.InnerText = dt.Rows[0]["BIZ_TYPE_NAME"] + "";

                node = (XmlElement)xmlDoc.SelectSingleNode("PASS_HEAD/TRADE_CODE");
                node.InnerText = dt.Rows[0]["TRADE_CODE"] + "";

                node = (XmlElement)xmlDoc.SelectSingleNode("PASS_HEAD/TRADE_NAME");
                node.InnerText = dt.Rows[0]["TRADE_NAME"] + "";

                node = (XmlElement)xmlDoc.SelectSingleNode("PASS_HEAD/CONSIGNEE_CODE");
                node.InnerText = dt.Rows[0]["CONSIGNEE_CODE"] + "";

                node = (XmlElement)xmlDoc.SelectSingleNode("PASS_HEAD/CONSIGNEE_NAME");
                node.InnerText = dt.Rows[0]["CONSIGNEE_NAME"] + "";

                node = (XmlElement)xmlDoc.SelectSingleNode("PASS_HEAD/TRADE_CODE_IN");
                node.InnerText = dt.Rows[0]["TRADE_CODE_IN"] + "";

                node = (XmlElement)xmlDoc.SelectSingleNode("PASS_HEAD/TRADE_NAME_IN");
                node.InnerText = dt.Rows[0]["TRADE_NAME_IN"] + "";

                node = (XmlElement)xmlDoc.SelectSingleNode("PASS_HEAD/PACK_NO");
                node.InnerText = dt.Rows[0]["PACK_NO"] + "";

                node = (XmlElement)xmlDoc.SelectSingleNode("PASS_HEAD/GROSS_WT");
                node.InnerText = dt.Rows[0]["GROSS_WT"] + "";

                node = (XmlElement)xmlDoc.SelectSingleNode("PASS_HEAD/NET_WT");
                node.InnerText = dt.Rows[0]["NET_WT"] + "";

                node = (XmlElement)xmlDoc.SelectSingleNode("PASS_HEAD/TRAFFICTYPE");
                node.InnerText = dt.Rows[0]["TRAFFICTYPE"] + "";

                node = (XmlElement)xmlDoc.SelectSingleNode("PASS_HEAD/BILL_TYPE");
                node.InnerText = dt.Rows[0]["BILL_TYPE"] + "";

                node = (XmlElement)xmlDoc.SelectSingleNode("PASS_HEAD/ENTRY_ID_OUT");
                node.InnerText = dt.Rows[0]["ENTRY_ID_OUT"] + "";

                node = (XmlElement)xmlDoc.SelectSingleNode("PASS_HEAD/APPCIQTYPE");
                node.InnerText = dt.Rows[0]["APPCIQTYPE"] + "";

                node = (XmlElement)xmlDoc.SelectSingleNode("PASS_HEAD/WRAP_TYPE_ID");
                node.InnerText = dt.Rows[0]["WRAP_TYPE_ID"] + "";

                node = (XmlElement)xmlDoc.SelectSingleNode("PASS_HEAD/MAINCODE");
                node.InnerText = dt.Rows[0]["MAINCODE"] + "";


                node = (XmlElement)xmlDoc.SelectSingleNode("PASS_HEAD/SUBCODE");
                node.InnerText = dt.Rows[0]["SUBCODE"] + "";

                node = (XmlElement)xmlDoc.SelectSingleNode("PASS_HEAD/MANUAL_NO");
                node.InnerText = dt.Rows[0]["MANUAL_NO"] + "";



                node = (XmlElement)xmlDoc.SelectSingleNode("PASS_HEAD/OUT_CODE");
                node.InnerText = dt.Rows[0]["OUT_CODE"] + "";

                node = (XmlElement)xmlDoc.SelectSingleNode("PASS_HEAD/TRANSFER_NO");
                node.InnerText = dt.Rows[0]["TRANSFER_NO"] + "";

                node = (XmlElement)xmlDoc.SelectSingleNode("PASS_HEAD/GOODS_TYPE_LY");
                node.InnerText = dt.Rows[0]["GOODS_TYPE_LY"] + "";

                node = (XmlElement)xmlDoc.SelectSingleNode("PASS_HEAD/ISHDZ");
                node.InnerText = dt.Rows[0]["ISHDZ"] + "";


                node = (XmlElement)xmlDoc.SelectSingleNode("PASS_HEAD/TRADETYPE");
                node.InnerText = dt.Rows[0]["TRADETYPE"] + "";


                node = (XmlElement)xmlDoc.SelectSingleNode("PASS_HEAD/OUT_GOODS_TYPE_LY");
                node.InnerText = dt.Rows[0]["OUT_GOODS_TYPE_LY"] + "";


                node = (XmlElement)xmlDoc.SelectSingleNode("PASS_HEAD/LY_BIZ_TYPE_ID");
                node.InnerText = dt.Rows[0]["LY_BIZ_TYPE_ID"] + "";

                node = (XmlElement)xmlDoc.SelectSingleNode("PASS_HEAD/GOODS_TYPE_ID");
                node.InnerText = dt.Rows[0]["GOODS_TYPE_ID"] + "";

                node = (XmlElement)xmlDoc.SelectSingleNode("PASS_HEAD/LYTYPE_ID");
                node.InnerText = dt.Rows[0]["LYTYPE_ID"] + "";

                node = (XmlElement)xmlDoc.SelectSingleNode("PASS_HEAD/AREA_CODE");
                node.InnerText = dt.Rows[0]["AREA_CODE"] + "";

                node = (XmlElement)xmlDoc.SelectSingleNode("PASS_HEAD/MEANSOFTRANSPORTNAME");
                node.InnerText = dt.Rows[0]["MEANSOFTRANSPORTNAME"] + "";

                node = (XmlElement)xmlDoc.SelectSingleNode("PASS_HEAD/MEANSOFTRANSPORTID");
                node.InnerText = dt.Rows[0]["MEANSOFTRANSPORTID"] + "";

                node = (XmlElement)xmlDoc.SelectSingleNode("PASS_HEAD/REMARK");
                node.InnerText = dt.Rows[0]["REMARK"] + "";

                node = (XmlElement)xmlDoc.SelectSingleNode("PASS_HEAD/FULL_NO_ZD");
                node.InnerText = dt.Rows[0]["FULL_NO_ZD"] + "";

                node = (XmlElement)xmlDoc.SelectSingleNode("PASS_HEAD/OUT_TRAF_MODE");
                node.InnerText = dt.Rows[0]["OUT_TRAF_MODE"] + "";

                node = (XmlElement)xmlDoc.SelectSingleNode("PASS_HEAD/IS_BLR");
                node.InnerText = dt.Rows[0]["IS_BLR"] + "";


                node = (XmlElement)xmlDoc.SelectSingleNode("PASS_HEAD/SEND_USER");
                node.InnerText = dt.Rows[0]["SEND_USER"] + "";


                node = (XmlElement)xmlDoc.SelectSingleNode("PASS_HEAD/SEND_TIME");
                node.InnerText = dt.Rows[0]["SEND_TIME"] + "";
            }

            xmlDoc.Save(path);

            //MessageQueue mq = new MessageQueue("FormatName:DIRECT=TCP:221.224.206.253\\Private$\\etf");
            //Message msg = new Message();
            //msg.Body = xmlDoc.ToString();
            //msg.Formatter = new System.Messaging.XmlMessageFormatter(new Type[] { typeof(string) });
            MessageQueue mq = new MessageQueue("FormatName:DIRECT=TCP:58.210.121.35\\Private$\\DataCenter_KS");
            //MessageQueue mq = new MessageQueue("FormatName:DIRECT=TCP:221.224.21.29\\Private$\\wmzyddata");
            Message msg = new Message();
            ////ZYDFL_S_系统名称_十个0_十个0_企业内部编号_GUID.xml old

            //作业单 ZYDFL_S_FL_申报单位十位编码_十个0_企业内部编号_GUID.xml new

            string guid = Guid.NewGuid().ToString();
            string Label = "ZYDFL_S_FL_" + dt.Rows[0]["APPCOMPANY"] + "_0000000000_" + dt.Rows[0]["ORDERCODE"] + "_" + guid + ".xml";

            using (FileStream fstream = new FileStream(path, FileMode.Open))
            {
                msg.BodyStream = fstream;
                msg.Label = Label;

                mq.Send(msg, MessageQueueTransactionType.Single);
                sql = "update LIST_WUMAO set STATUS='1' where ordercode='" + ORDERCODE + "'";
                DBMgr.ExecuteNonQuery(sql);
            }

            return "1";
        }


        public string LoadDataMatching_List()
        {
            int PageSize = Convert.ToInt32(Request.Params["rows"]);
            int Page = Convert.ToInt32(Request.Params["page"]);
            int total = 0;
            string sql = "select * from LIST_WUMAODADAMATCHING where 1=1";
            string sort = !string.IsNullOrEmpty(Request.Params["sort"]) && Request.Params["sort"] != "text" ? Request.Params["sort"] : "ID";
            string order = !string.IsNullOrEmpty(Request.Params["order"]) ? Request.Params["order"] : "DESC";
            sql = Extension.GetPageSql(sql, sort, order, ref total, (Page - 1) * PageSize, Page * PageSize);
            DataTable dt = DBMgr.GetDataTable(sql);
            IsoDateTimeConverter iso = new IsoDateTimeConverter();//序列化JSON对象时,日期的处理格式
            iso.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
            string result = JsonConvert.SerializeObject(dt, iso);
            result = "{\"total\":" + total + ",\"rows\":" + result + "}";
            return result;
        }

        public ActionResult SaveDataMatching(FormCollection form)
        {

            string ID = Request["ID"];
            string sql = "";

            if (!string.IsNullOrEmpty(ID))
            {
                sql = "update LIST_WUMAODADAMATCHING set ";
                sql += " BUSINAME = '" + Request.Form["BUSINAME"] + "',";
                sql += " GOODS_NATURE_ID = '" + Request.Form["GOODS_NATURE_ID"] + "',";
                sql += " I_E_FALG_TYPE = '" + Request.Form["I_E_FALG_TYPE"] + "',";
                sql += " BIZ_TYPE_ID = '" + Request.Form["BIZ_TYPE_ID"] + "',";
                sql += " TRAFFICTYPE = '" + Request.Form["TRAFFICTYPE"] + "',";
                sql += " BILL_TYPE = '" + Request.Form["BILL_TYPE"] + "',";
                sql += " APPCIQTYPE = '" + Request.Form["APPCIQTYPE"] + "',";
                sql += " OUT_TRAF_MODE = '" + Request.Form["OUT_TRAF_MODE"] + "',";
                sql += " BUSICODE = '" + Request.Form["BUSICODE"] + "',";
                sql = sql.Substring(0, sql.Length - 1);
                sql += " where ID =" + ID;
            }
            else
            {
                sql = "INSERT INTO LIST_WUMAODADAMATCHING ";
                sql += " ( ID,BUSINAME,GOODS_NATURE_ID , I_E_FALG_TYPE , BIZ_TYPE_ID ,  TRAFFICTYPE , BILL_TYPE , APPCIQTYPE , BUSICODE,OUT_TRAF_MODE )";
                sql += " VALUES ( LIST_WUMAODADAMATCHING_ID.Nextval, '" + Request.Form["BUSINAME"]
                    + "','" + Request.Form["GOODS_NATURE_ID"]
                    + "','" + Request.Form["I_E_FALG_TYPE"]
                    + "','" + Request.Form["BIZ_TYPE_ID"]
                    + "','" + Request.Form["TRAFFICTYPE"]
                    + "','" + Request.Form["BILL_TYPE"]
                    + "','" + Request.Form["APPCIQTYPE"]
                    + "','" + Request.Form["BUSICODE"]
                    + "','" + Request.Form["OUT_TRAF_MODE"]
                    + "')";
            }

            if (DBMgr.ExecuteNonQuery(sql) == 1)
            {

                return Json(new { Success = true }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { Success = false, }, JsonRequestBehavior.AllowGet);
            }
        }


        public string LoadWumao_List()
        {
            int PageSize = Convert.ToInt32(Request.Params["rows"]);
            int Page = Convert.ToInt32(Request.Params["page"]);
            int total = 0;
            string sql = "select * from list_wumao where 1=1";
            string data = Request["data"];
            if (data != null)
            {
                JObject jo = JsonConvert.DeserializeObject<JObject>(data);      //json格式转换为数组
                if (jo.Value<string>("ordercode_value") != "" && jo.Value<string>("ordercode") != "text")
                {
                    sql += " AND " + jo.Value<string>("ordercode") + " ='" + jo.Value<string>("ordercode_value") + "'";
                }
                if (jo.Value<string>("XCBUSINAME") != null && jo.Value<string>("XCBUSINAME") != "")
                {
                    sql += " AND XCBUSINAME = '" + jo.Value<string>("XCBUSINAME") + "' ";
                }
                if (jo.Value<string>("starttime") != "" && jo.Value<string>("starttime") != null)
                {
                    sql += " AND CREATETIME >= to_date('" + jo.Value<string>("starttime") + "','yyyy-MM-dd')";
                }
                if (jo.Value<string>("stoptime") != "" && jo.Value<string>("stoptime") != null)
                {
                    sql += " AND CREATETIME <= to_date('" + jo.Value<string>("stoptime") + "','yyyy-MM-dd')";
                }
            }
            string sort = !string.IsNullOrEmpty(Request.Params["sort"]) && Request.Params["sort"] != "text" ? Request.Params["sort"] : "ID";
            string order = !string.IsNullOrEmpty(Request.Params["order"]) ? Request.Params["order"] : "DESC";
            sql = Extension.GetPageSql(sql, sort, order, ref total, (Page - 1) * PageSize, Page * PageSize);
            DataTable dt = DBMgr.GetDataTable(sql);
            IsoDateTimeConverter iso = new IsoDateTimeConverter();//序列化JSON对象时,日期的处理格式
            iso.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
            string result = JsonConvert.SerializeObject(dt, iso);
            result = "{\"total\":" + total + ",\"rows\":" + result + "}";
            return result;
        }


        public string Load_WuMaoBack()
        {
            int PageSize = Convert.ToInt32(Request.Params["rows"]);
            int Page = Convert.ToInt32(Request.Params["page"]);
            int total = 0;
            string sql = "select * from LIST_WUMAO_BACK where 1=1";
            string data = Request["data"];
            if (data != null)
            {
                JObject jo = JsonConvert.DeserializeObject<JObject>(data);      //json格式转换为数组
                if (jo.Value<string>("ordercode_value") != "" && jo.Value<string>("ordercode") != "text")
                {
                    sql += " AND " + jo.Value<string>("ordercode") + " ='" + jo.Value<string>("ordercode_value") + "'";
                }
                if (jo.Value<string>("businessin_createname") != null && jo.Value<string>("businessin_createname") != "")
                {
                    sql += " AND CREATENAME = '" + jo.Value<string>("businessin_createname") + "' ";
                }
                if (jo.Value<string>("starttime") != "" && jo.Value<string>("starttime") != null)
                {
                    sql += " AND CREATETIME >= to_date('" + jo.Value<string>("starttime") + "','yyyy-MM-dd')";
                }
                if (jo.Value<string>("stoptime") != "" && jo.Value<string>("stoptime") != null)
                {
                    sql += " AND CREATETIME <= to_date('" + jo.Value<string>("stoptime") + "','yyyy-MM-dd')";
                }
            }
            string sort = !string.IsNullOrEmpty(Request.Params["sort"]) && Request.Params["sort"] != "text" ? Request.Params["sort"] : "ID";
            string order = !string.IsNullOrEmpty(Request.Params["order"]) ? Request.Params["order"] : "DESC";
            sql = Extension.GetPageSql(sql, sort, order, ref total, (Page - 1) * PageSize, Page * PageSize);
            DataTable dt = DBMgr.GetDataTable(sql);
            IsoDateTimeConverter iso = new IsoDateTimeConverter();//序列化JSON对象时,日期的处理格式
            iso.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
            string result = JsonConvert.SerializeObject(dt, iso);
            result = "{\"total\":" + total + ",\"rows\":" + result + "}";
            return result;
        }

    }
}
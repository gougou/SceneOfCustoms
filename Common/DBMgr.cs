﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using Oracle.ManagedDataAccess.Client;
using System.Data.Common;
using System.Data;
using System.Reflection;

namespace SceneOfCustoms.Common
{
    public class DBMgr
    {
        private static readonly string ConnectionString = ConfigurationManager.AppSettings["strconn"];

        public static DataSet GetDataSet(string sql)
        {
            DataSet ds = new DataSet();
            OracleConnection orclCon = null;
            try
            {
                using (orclCon = new OracleConnection(ConnectionString))
                {
                    DbCommand oc = orclCon.CreateCommand();
                    oc.CommandText = sql;
                    if (orclCon.State.ToString().Equals("Open"))
                    {
                        orclCon.Close();
                    }
                    orclCon.Open();
                    DbDataAdapter adapter = new OracleDataAdapter();
                    adapter.SelectCommand = oc;
                    adapter.Fill(ds);
                }
            }
            catch (Exception e)
            {

            }
            finally
            {
                orclCon.Close();
            }
            return ds;
        }

        public static DataTable GetDataTable(string sql)
        {
            DataSet ds = new DataSet();
            OracleConnection orclCon = null;
            try
            {
                using (orclCon = new OracleConnection(ConnectionString))
                {
                    DbCommand oc = orclCon.CreateCommand();
                    oc.CommandText = sql;
                    if (orclCon.State.ToString().Equals("Open"))
                    {
                        orclCon.Close();
                    }
                    orclCon.Open();
                    DbDataAdapter adapter = new OracleDataAdapter();
                    adapter.SelectCommand = oc;
                    adapter.Fill(ds);
                }
            }
            catch (Exception e)
            {
                //log.Error(e.Message + e.StackTrace);
            }
            finally
            {
                orclCon.Close();
            }
            return ds.Tables[0];
        }

        public static int ExecuteNonQuery(string sql)
        {
            int retcount = -1;
            OracleConnection orclCon = null;
            using (orclCon = new OracleConnection(ConnectionString))
            {
                OracleCommand oc = new OracleCommand(sql, orclCon);
                if (orclCon.State.ToString().Equals("Open"))
                {
                    orclCon.Close();
                }
                orclCon.Open();
                retcount = oc.ExecuteNonQuery();
                oc.Parameters.Clear();
            }
            //}
            //catch (Exception e)
            //{
            //    //log.Error(e.Message + e.StackTrace);
            //}
            //finally
            //{
            //    orclCon.Close();
            //}
            return retcount;
        }

    }
}
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
using System.Data.Odbc;

namespace NewProductionPlan
{
    internal class dataProvider
    {
        private OdbcConnection con = new OdbcConnection();
        private dbConnection dbCon = new dbConnection();


        public void ExecuteCommand(string MyQuery)
        {
            con = dbCon.dbConnect();
            con.Open();
            OdbcCommand sqlComm = new OdbcCommand(MyQuery, con);
            sqlComm.ExecuteNonQuery();
            con.Close();
        }
        public DataSet getDataSet(string MyQuery, string MyString, string dsnName)
        {
            OdbcDataAdapter daGen;

            DataSet dsGen = new DataSet();
            dbCon.dsnName = dsnName;

            con = dbCon.dbConnect();

            OdbcCommand cmd = new OdbcCommand(MyQuery, con);

            daGen = new OdbcDataAdapter(cmd);
            daGen.Fill(dsGen, MyString);

            con.Close();

            return dsGen;
        }

        // :::::::::::: Use this method to execute query by DataReader ::::::::::::
        public static OdbcDataReader getDataReader(string MyQuery)
        {
            OdbcConnection con = new OdbcConnection();
            dbConnection dbCon = new dbConnection();
            con = dbCon.dbConnect();
            con.Open();
            OdbcCommand cmd = new OdbcCommand(MyQuery, con);

            return cmd.ExecuteReader();
        }

        // :::::::::::: Use this method to return a DataTable from query string ::::::::::::
        public DataTable getDataTable(string MyQuery, string MyTable, string dsnName)
        {
            DataTable dt = new DataTable();
            OdbcConnection con1 = new OdbcConnection();
            dbConnection dbCon1 = new dbConnection();
            OdbcDataAdapter daGen1;
            DataSet dsGen1;

            dsGen1 = new DataSet();
            dbCon1.dsnName = dsnName;
            con1 = dbCon1.dbConnect();
            daGen1 = new OdbcDataAdapter(MyQuery, con1);
            daGen1.Fill(dsGen1, MyTable);
            dt = dsGen1.Tables[MyTable];
            dsGen1.Tables.Remove(MyTable);

            return dt;
        }

    }
}

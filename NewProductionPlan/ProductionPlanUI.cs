using System;
using System.Security;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CrystalDecisions.CrystalReports.Engine;
using NewProductionPlan.Reports;

namespace NewProductionPlan
{
    public partial class ProductionPlanUI : Form
    {
        SQL sq = new SQL();
        public DataTable dt = new DataTable();
        public DataTable dt1 = new DataTable();
        
        public int rowIndex;
        public int columnIndex;
        public string productionDate;
        public int goButtonClicked= 0;
        public int editButtonClicked=0;
        public int postedEntry = 0;
        public int postButtonClicked = 0;
        public int saveButtonClicked = 0;
        public Boolean oldRecord;
        public Boolean enableEdit;
        public int POSTSTATUS;

        public int btnClickFlag = 0;

        public AccpacCOMAPI.AccpacSession session;
        public AccpacCOMAPI.AccpacDBLink mDBLinkCmpRW;
        public AccpacCOMAPI.AccpacDBLink mDBLinkSysRW;

        dataProvider ReportData = new dataProvider();
        public ProductionPlanUI()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            string[] column = new string[] {"Sl No", "Item No", "Description", "UOM", "AOP QTY", "Plan QTY", "Shift A", "Shift B", 
                "Shift C","G Shift","D Shift","N Shift", "Pro. QTY","Shift A","Shift B","Shift C","Shift G","Shift D","Shift N","Comment"};
            int col = 20;
            dataGridViewShow.ColumnCount = 20;
            for (int i = 0; i < col; i++)
            {
                dataGridViewShow.Columns[i].Name = column[i];
            }

            //*************************DATA GENERATE IN cmbProductGroup************************
            dt = sq.get_rs("Select RTRIM(SEGVAL)+'-'+ LTRIM([desc]) AS des from ICSEGv where segment = 2");

            foreach (DataRow r in dt.Rows)
            {
                cmbProductGroup.Items.Add(r["des"].ToString());
            }
            cmbProductLine.Items.Add("Line1");

            //*************************Production Shift****************************************
            cmbProductShift.Items.Add("General");
            cmbProductShift.Items.Add("Day-Night");
            cmbProductShift.Items.Add("A-B-C Shift");

            dateTimePicker.Value = DateTime.Today;
            cmbProductGroup.SelectedIndex = 2;
            cmbProductLine.SelectedIndex = 0;
            cmbProductShift.SelectedIndex = 0;

            btnDelete.Enabled = false;
            buttonStatus();
        }
        private void btnGo_Click(object sender, EventArgs e)
        {
            btnClickFlag = 0;

            dataGridViewShow.ReadOnly = false;
            dataGridViewShow.Rows.Clear();
            dataGridViewShow.Columns.Clear();
            dataGridViewShow.Refresh();

            editButtonClicked = 0;
            postButtonClicked = 0;
            postedEntry = 0;
            btnDelete.Enabled = true;
            goButtonClicked = 1;
            enableEdit = false;

            btnGoFillData();
            buttonStatus();
        }
        public void btnGoFillData()
        {
            gridColum();
            productionDate = dateTimePicker.Value.ToString("yyyyMMdd");
            //*************** if data gridview hold any previous data***************
            dt = sq.get_rs("Select * from T_pcpln where PRODUCTIONDATE = '" + productionDate + "' and RIGHT(LEFT(WIPITEMNO,5),2) = '" + cmbProductGroup.Text.Substring(0, 2) + "' and SHIFTTYPE = '"+cmbProductShift.Text+"' order by WIPITEMNO");          
            if (dt.Rows.Count > 0)
            {
                //postedEntry=Convert.ToInt32( dt.Rows[0]["poststatus"].ToString());
                POSTSTATUS = Convert.ToInt32(dt.Rows[0]["poststatus"].ToString());
                int slNo = 1;
                oldRecord = true;
                int rowCnt = dt.Rows.Count;    
                foreach (DataRow r in dt.Rows)
                {
                    if (cmbProductShift.Text == "General")
                    {
                        this.dataGridViewShow.Rows.Add(slNo, r["WIPITEMNO"].ToString(), r["WIPITEMDESC"].ToString(),
                            r["STOCKUNIT"].ToString(), "", r["QUANTITYPLN"].ToString(), r["SHIFTGQTYPLN"].ToString(), r["QUANTITYPRD"].ToString(), r["SHIFTGQTYPRD"].ToString(), r["COMMENT"].ToString());
                    }
                    else if (cmbProductShift.Text == "Day-Night")
                    {
                        this.dataGridViewShow.Rows.Add(slNo, r["WIPITEMNO"].ToString(), r["WIPITEMDESC"].ToString(),
                            r["STOCKUNIT"].ToString(), "", r["QUANTITYPLN"].ToString(), r["SHIFTDQTYPLN"].ToString(), r["SHIFTNQTYPLN"].ToString(), r["QUANTITYPRD"].ToString(), r["SHIFTDQTYPRD"].ToString(), r["SHIFTNQTYPRD"].ToString(), r["COMMENT"].ToString());
                    }
                    else if (cmbProductShift.Text == "A-B-C Shift")
                    {
                        this.dataGridViewShow.Rows.Add(slNo, r["WIPITEMNO"].ToString(), r["WIPITEMDESC"].ToString(),
                            r["STOCKUNIT"].ToString(), "", r["QUANTITYPLN"].ToString(), r["SHIFTAQTYPLN"].ToString(), r["SHIFTBQTYPLN"].ToString(), r["SHIFTCQTYPLN"].ToString(), r["QUANTITYPRD"].ToString(), r["SHIFTAQTYPRD"].ToString(), r["SHIFTBQTYPRD"].ToString(), r["SHIFTCQTYPRD"].ToString(), r["COMMENT"].ToString());
                    }
                    slNo++;
                }

                if (cmbProductShift.Text == "General")
                {
                    dt1 = sq.get_rs("select  sum([SHIFTGQTYPLN]) as totalG from t_pcpln where [PRODUCTIONDATE] = '" + productionDate + "' and RIGHT(LEFT(WIPITEMNO,5),2) = '" + cmbProductGroup.Text.Substring(0, 2) + "' and SHIFTTYPE='"+cmbProductShift.Text+"' ");
                    foreach (DataRow rr in dt1.Rows)
                    {
                        dataGridViewShow.Rows.Add(" ", " ", "Total", "Kg", " ", rr["totalG"].ToString(), rr["totalG"].ToString(), rr["totalG"].ToString(), rr["totalG"].ToString());                        
                    }
                }
                else if (cmbProductShift.Text == "Day-Night")
                {
                    dt1 = sq.get_rs("select sum([QUANTITYPLN]) as totalQty, sum([SHIFTDQTYPLN]) as totalD, sum([SHIFTNQTYPLN]) as totalN, sum([QUANTITYPRD]) as totalQtyPrd , sum([SHIFTDQTYPRD]) as totatDPrd, sum([SHIFTNQTYPrd]) as totalNPrd from t_pcpln where [PRODUCTIONDATE] = '" + productionDate + "' and RIGHT(LEFT(WIPITEMNO,5),2) = '" + cmbProductGroup.Text.Substring(0, 2) + "' and SHIFTTYPE='" + cmbProductShift.Text + "' ");
                    foreach (DataRow rr in dt1.Rows)
                    {
                        dataGridViewShow.Rows.Add(" ", " ", "Total", "Kg", " ", rr["totalQty"].ToString(), rr["totalD"].ToString(), rr["totalN"].ToString(), rr["totalQtyPrd"].ToString(), rr["totatDPrd"].ToString(), rr["totalNPrd"].ToString());
                    }
                }
                else if (cmbProductShift.Text == "A-B-C Shift")
                {
                    dt1 = sq.get_rs("select sum([QUANTITYPLN]) as totalQty, sum([SHIFTAQTYPLN]) as totalA, sum([SHIFTBQTYPLN]) as totalB,sum([SHIFTCQTYPLN]) as totalC,sum([QUANTITYPRD]) as totalQtyPrd ,sum([SHIFTAQTYPRD]) as totatAPrd, sum([SHIFTBQTYPrd]) as totalBPrd,sum([SHIFTCQTYPrd]) as totalCPrd from t_pcpln where [PRODUCTIONDATE] = '" + productionDate + "' and RIGHT(LEFT(WIPITEMNO,5),2) = '" + cmbProductGroup.Text.Substring(0, 2) + "' and SHIFTTYPE='" + cmbProductShift.Text + "'");
                    foreach (DataRow rr in dt1.Rows)
                    {
                        dataGridViewShow.Rows.Add(" ", " ", "Total", "Kg", " ", rr["totalQty"].ToString(), rr["totalA"].ToString(), rr["totalB"].ToString(), rr["totalC"].ToString(), rr["totalQtyPrd"].ToString(), rr["totatAPrd"].ToString(), rr["totalBPrd"].ToString(), rr["totalCPrd"].ToString());
                    }
                }

                dataGridViewShow.Rows[rowCnt].DefaultCellStyle.Font = new Font("Courier New", 11, FontStyle.Bold);

                //********read only true for adding row****************** 
                
                dataGridViewShow.Rows[rowCnt].ReadOnly = true;
                dataGridViewShow.AllowUserToAddRows = false;

                //*********DataGridview not sortable*********************
                foreach (DataGridViewColumn column in dataGridViewShow.Columns)
                {
                    column.SortMode = DataGridViewColumnSortMode.NotSortable;
                }

                dataGridViewShow.ReadOnly = true;
            }
            else
            {

                oldRecord = false;
               
                gridColum();
                int rowCnt = dataGridViewShow.Rows.Count;
                dt = sq.get_rs("select FMTITEMNO as WIPITEMNO, [DESC] as WIPITEMDESC,  STOCKUNIT, '" + cmbProductShift.Text + "' AS SHIFTTYPE, '0' as QUANTITYPLN, '0' as SHIFTAQTYPLN, '0' as SHIFTBQTYPLN, '0' as SHIFTCQTYPLN, '0' as SHIFTGQTYPLN, '0' as SHIFTDQTYPLN, '0' as SHIFTNQTYPLN, '0' as QUANTITYPRD, '0' as SHIFTAQTYPRD, '0' as SHIFTBQTYPRD, '0' as SHIFTCQTYPRD, '0' as SHIFTGQTYPRD, '0' as SHIFTDQTYPRD, '0' as SHIFTNQTYPRD, '' as COMMENT from ICITEM where FMTITEMNO like '30%' and RIGHT(LEFT(FMTITEMNO,5),2) = '" + cmbProductGroup.Text.Substring(0, 2) + "' order by WIPITEMNO");
                int slNo = 1;
                foreach (DataRow r in dt.Rows)
                {              
                    if (cmbProductShift.Text == "General")
                    {
                        this.dataGridViewShow.Rows.Add(slNo, r["WIPITEMNO"].ToString(), r["WIPITEMDESC"].ToString(),
                            r["STOCKUNIT"].ToString(), "", "0", "0","0","0", "");
                    }

                    else if (cmbProductShift.Text == "Day-Night")
                    {
                        this.dataGridViewShow.Rows.Add(slNo, r["WIPITEMNO"].ToString(), r["WIPITEMDESC"].ToString(),
                            r["STOCKUNIT"].ToString(), "", "0", "0", "0", "0","0","0","");
                    }
                    else if (cmbProductShift.Text == "A-B-C Shift")
                    {
                        this.dataGridViewShow.Rows.Add(slNo, r["WIPITEMNO"].ToString(), r["WIPITEMDESC"].ToString(),
                            r["STOCKUNIT"].ToString(), "", "0", "0", "0", "0", "0","0","0","0","");
                    }
                    slNo++;
                }

                if (cmbProductShift.Text == "General")
                {
                    dataGridViewShow.Rows.Add(" ", " ", "Total", "Kg", " ", "0", "0", "0", "0");
                }
                else if (cmbProductShift.Text == "Day-Night")
                {
                    dataGridViewShow.Rows.Add(" ", "", "Total", "Kg", "", "0", "0", "0","0","0","0");
                }
                else if (cmbProductShift.Text == "A-B-C Shift")
                {                 
                    this.dataGridViewShow.Rows.Add(" ", "", "Total", "Kg", "", "0", "0", "0", "0","0","0","0","0");
                }

                rowCnt = dataGridViewShow.Rows.Count;
                dataGridViewShow.Rows[rowCnt-1].DefaultCellStyle.Font = new Font("Courier New", 11, FontStyle.Bold);
               
                dataGridViewShow.Rows[rowCnt-1].ReadOnly = true;
                dataGridViewShow.AllowUserToAddRows = false;

                //********** ********DataGridview not sortable************************8
                foreach (DataGridViewColumn column in dataGridViewShow.Columns)
                {
                    column.SortMode = DataGridViewColumnSortMode.NotSortable;
                }
            }
        }
        public void gridColum()
        {
            if (cmbProductShift.Text == "General")
            {
                try
                {
                    GeneralGrid();

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    Environment.Exit(0);
                }
            }
            else if (cmbProductShift.Text == "Day-Night")
            {
                try
                {
                    DayNightGrid();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    Environment.Exit(0);
                }                
            }

            else if (cmbProductShift.Text == "A-B-C Shift")
            {
                try
                {
                    ABCGrid();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    Environment.Exit(0);
                }
            }
        }
        private void btnClose_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        // ***************** To Save Gridview Data********************
        private void btnSave_Click(object sender, EventArgs e)
        {
            if (oldRecord == true && enableEdit == false)
            {
                MessageBox.Show("Cannot modify the record....");
                return;
            }     
            int rowCnt = dataGridViewShow.Rows.Count;
            dataGridViewShow.Rows[rowCnt - 1].ReadOnly = true;
            //////////////////////////////Today Editing////////////////////////////////////////////////
      
            if (dataGridViewShow.Rows[0].Cells[0].Value == null || (Convert.ToInt32(dataGridViewShow.Rows[rowCnt-1].Cells[5].Value.ToString()) == 0))         
            {
                MessageBox.Show("No data to save");
                goto done;
            }
            saveButtonClicked = 1;
            try
            {
                    btnSave.Focus();

                    productionDate = dateTimePicker.Value.ToString("yyyyMMdd");
                    dateTimePicker2.Value = DateTime.Now;
                    string postDate = dateTimePicker2.Value.ToString("yyyyMMdd");
                    string postTime = dateTimePicker2.Value.ToString("HHmm");
                    string userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                    int totalRow = dataGridViewShow.RowCount-1;

                    if (enableEdit == true)
                    {
                        dt = sq.get_rs("delete from t_pcpln where productiondate = '" + productionDate + "' and category = '" + cmbProductGroup.Text.Substring(3,10) + "' and shifttype = '" + cmbProductShift.Text + "'");
                    }
                    for (int i = 0; i < totalRow; i++)
                    {
                        for (int j = 1; j < 2; j++)
                        {
                            //*********************General Shift**********************
                            if (cmbProductShift.SelectedIndex == 0)
                            {
                                string itemNo = dataGridViewShow.Rows[i].Cells[j].Value.ToString();
                                string itemDescription = dataGridViewShow.Rows[i].Cells[j + 1].Value.ToString();
                                string itemCategory = cmbProductGroup.Text.Substring(3, 10);
                                string stockUnit = dataGridViewShow.Rows[i].Cells[j + 2].Value.ToString();
                                string productShift = cmbProductShift.Text;
                                string qtyPlan = dataGridViewShow.Rows[i].Cells[j + 4].Value.ToString();
                                string gShift = dataGridViewShow.Rows[i].Cells[j + 5].Value.ToString(); 

                                string qtyPrdPlan = dataGridViewShow.Rows[i].Cells[j + 6].Value.ToString();
                                string g_Shift = dataGridViewShow.Rows[i].Cells[j + 7].Value.ToString();
                                string comment = dataGridViewShow.Rows[i].Cells[j + 8].Value.ToString();
                                dt = sq.get_rs("insert into t_pcpln([PRODUCTIONDATE],[WIPITEMNO],[WIPITEMDESC],[CATEGORY],[STOCKUNIT],[SHIFTTYPE],[QUANTITYPLN],[SHIFTAQTYPLN],[SHIFTBQTYPLN],[SHIFTCQTYPLN],[SHIFTGQTYPLN],[SHIFTDQTYPLN],[SHIFTNQTYPLN],[QUANTITYPRD],[SHIFTAQTYPRD],[SHIFTBQTYPRD],[SHIFTCQTYPRD] ,[SHIFTGQTYPRD],[SHIFTDQTYPRD],[SHIFTNQTYPRD],[ACTUALQUANTITY],[COMMENT],[POSTATUS],[AUDTDATE],[POSTDATE],[POSTTIME],[POSTSTATUS],[AUDTUSER],[POSTUSER]) values ('" +
                                   productionDate + "','" + itemNo + "','" +itemDescription + "','" + itemCategory + "','" +stockUnit + "','" + productShift + "','" + qtyPlan
                                   + "',0,0,0,'" + gShift + "',0,0,'"+qtyPrdPlan+"',0,0,0,'"+g_Shift+"',0,0,0,'" + comment + "',1, '" + postDate + "','" + postDate + "','"+postTime+"',1,'"+userName+"','"+userName+"')");
                            }

                             //*********************Day-night Shift*********************
                            else if (cmbProductShift.SelectedIndex == 1)
                            {
                                string itemNo = dataGridViewShow.Rows[i].Cells[j].Value.ToString();
                                string itemDescription = dataGridViewShow.Rows[i].Cells[j + 1].Value.ToString();
                                string itemCategory = cmbProductGroup.Text.Substring(3, 10);
                                string stockUnit = dataGridViewShow.Rows[i].Cells[j + 2].Value.ToString();
                                string productShift = cmbProductShift.Text;
                                string qtyPlan = dataGridViewShow.Rows[i].Cells[j + 4].Value.ToString();
                                string dShift = dataGridViewShow.Rows[i].Cells[j + 5].Value.ToString();
                                string nShift = dataGridViewShow.Rows[i].Cells[j + 6].Value.ToString();

                                string qtyPrdPlan = dataGridViewShow.Rows[i].Cells[j + 7].Value.ToString();
                                string d_Shift = dataGridViewShow.Rows[i].Cells[j + 8].Value.ToString();
                                string n_Shift = dataGridViewShow.Rows[i].Cells[j + 9].Value.ToString();
                                string comment = dataGridViewShow.Rows[i].Cells[j + 10].Value.ToString();
                                dt = sq.get_rs("insert into t_pcpln([PRODUCTIONDATE],[WIPITEMNO],[WIPITEMDESC],[CATEGORY],[STOCKUNIT],[SHIFTTYPE],[QUANTITYPLN],[SHIFTAQTYPLN],[SHIFTBQTYPLN],[SHIFTCQTYPLN],[SHIFTGQTYPLN],[SHIFTDQTYPLN],[SHIFTNQTYPLN],[QUANTITYPRD],[SHIFTAQTYPRD],[SHIFTBQTYPRD],[SHIFTCQTYPRD] ,[SHIFTGQTYPRD],[SHIFTDQTYPRD],[SHIFTNQTYPRD],[ACTUALQUANTITY],[COMMENT],[POSTATUS],[AUDTDATE],[POSTDATE],[POSTTIME],[POSTSTATUS],[AUDTUSER],[POSTUSER]) values ('" +
                                   productionDate + "','" + itemNo + "','" + itemDescription + "','" + itemCategory + "','" + stockUnit + "','" + productShift + "','" + qtyPlan
                                   + "',0,0,0,0,'" + dShift + "','"+nShift+"','"+qtyPrdPlan+"',0,0,0,0,'"+d_Shift+"','"+n_Shift+"',0,'" + comment + "',1, '" + postDate + "','" + postDate + "','" + postTime + "',1,'" + userName + "','" + userName + "')");
                            }
                            //*************************ABC Shift**************************
                            else if(cmbProductShift.SelectedIndex == 2)
                            {
                                string itemNo = dataGridViewShow.Rows[i].Cells[j].Value.ToString();
                                string itemDescription = dataGridViewShow.Rows[i].Cells[j + 1].Value.ToString();
                                string itemCategory = cmbProductGroup.Text.Substring(3, 10);
                                string stockUnit = dataGridViewShow.Rows[i].Cells[j + 2].Value.ToString();
                                string productShift = cmbProductShift.Text;
                                string qtyPlan = dataGridViewShow.Rows[i].Cells[j + 4].Value.ToString();
                                string aShift = dataGridViewShow.Rows[i].Cells[j + 5].Value.ToString();
                                string bShift = dataGridViewShow.Rows[i].Cells[j + 6].Value.ToString();
                                string cShift = dataGridViewShow.Rows[i].Cells[j + 7].Value.ToString();

                                string qtyPrdPlan = dataGridViewShow.Rows[i].Cells[j + 8].Value.ToString();
                                string a_Shift = dataGridViewShow.Rows[i].Cells[j + 9].Value.ToString();
                                string b_Shift = dataGridViewShow.Rows[i].Cells[j + 10].Value.ToString();
                                string c_Shift = dataGridViewShow.Rows[i].Cells[j + 11].Value.ToString();
                                string comment = dataGridViewShow.Rows[i].Cells[j + 12].Value.ToString();
                                dt = sq.get_rs("insert into t_pcpln([PRODUCTIONDATE],[WIPITEMNO],[WIPITEMDESC],[CATEGORY],[STOCKUNIT],[SHIFTTYPE],[QUANTITYPLN],[SHIFTAQTYPLN],[SHIFTBQTYPLN],[SHIFTCQTYPLN],[SHIFTGQTYPLN],[SHIFTDQTYPLN],[SHIFTNQTYPLN],[QUANTITYPRD],[SHIFTAQTYPRD],[SHIFTBQTYPRD],[SHIFTCQTYPRD] ,[SHIFTGQTYPRD],[SHIFTDQTYPRD],[SHIFTNQTYPRD],[ACTUALQUANTITY],[COMMENT],[POSTATUS],[AUDTDATE],[POSTDATE],[POSTTIME],[POSTSTATUS],[AUDTUSER],[POSTUSER]) values ('" +
                                   productionDate + "','" + itemNo + "','" + itemDescription + "','" + itemCategory + "','" + stockUnit + "','" + productShift + "','" + qtyPlan+ "'," +
                                   "'" + aShift + "','" + bShift + "','"+cShift+"',0,0,0,'"+qtyPrdPlan+"','"+a_Shift+"','"+b_Shift+"','"+c_Shift+"',0,0,0,0,'" + comment + "',1, '" + postDate + "','" + postDate + "','" + postTime + "',1,'" + userName + "','" + userName + "')");
                            }
                        }
                    }
                MessageBox.Show("Data Saved Successfully");
                //btnGo.PerformClick();
            }
            catch (Exception ex)
            {

                MessageBox.Show("Error from Save event"+ex.Message);
                Environment.Exit(0);
            }
            done:
            ;

            btnGoFillData();        
            buttonStatus();
        }

        //********************************Edit Data**************************************
        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (oldRecord == true)
            {


                if (POSTSTATUS == 2)
                {



                    MessageBox.Show("Cannot modify posted record....");
                   return;
                    

                }

                enableEdit = true;

            buttonStatus();

                MessageBox.Show("You can edit this record....");
                dataGridViewShow.ReadOnly = false;
        

            }
    }

        // *****************************Generating Report Data***********************
        private void btnPrint_Click(object sender, EventArgs e)
        {
            if (cmbProductShift.SelectedIndex == 0)
            {
               rptGeneralShift();
            }
            else if (cmbProductShift.SelectedIndex == 1)
            {
                rptDayNightShift();
            }
            else if (cmbProductShift.SelectedIndex == 2)
            {
                rptABCPShift();
            }
        }
        public void rptGeneralShift()
        {
            if (cmbProductShift.SelectedIndex==0)
            {
                string productionDate = dateTimePicker.Value.ToString("yyyyMMdd");
                
                frmRPTViewer rptViewer = new frmRPTViewer();
                DataSet dsGeneral = new DataSet();
                rptGeneral objRptGeneral = new rptGeneral();

                string strSql = "select * from t_pcpln where productiondate ='" + productionDate + "' and [SHIFTTYPE]='" + cmbProductShift.Text + "'  and RIGHT(LEFT(WIPITEMNO,5),2) = '" + cmbProductGroup.Text.Substring(0, 2) + "'   order by WIPITEMNO";
                dsGeneral = ReportData.getDataSet(strSql, "t_pcpln", "TCPL");
                objRptGeneral.SetDataSource(dsGeneral);
                objRptGeneral.VerifyDatabase();
                
                rptViewer.crystalReportViewer.ReportSource = objRptGeneral;
                rptViewer.Refresh();
                rptViewer.Show();
            }
        }
        public void rptDayNightShift()
        {
            if (cmbProductShift.SelectedIndex == 1)
            {
                string productionDate = dateTimePicker.Value.ToString("yyyyMMdd");

                rptDayNight objRptGeneral = new rptDayNight();
                frmRPTViewer rptViewer = new frmRPTViewer();
                DataSet dsGeneral = new DataSet();

                string strSql = "select * from t_pcpln where productiondate ='" + productionDate + "' and [SHIFTTYPE]='" + cmbProductShift.Text + "'  and RIGHT(LEFT(WIPITEMNO,5),2) = '" + cmbProductGroup.Text.Substring(0, 2) + "'  order by WIPITEMNO";

                dsGeneral = ReportData.getDataSet(strSql, "t_pcpln", "TCPL");

                objRptGeneral.SetDataSource(dsGeneral);

                objRptGeneral.VerifyDatabase();
                rptViewer.crystalReportViewer.ReportSource = objRptGeneral;
                rptViewer.Refresh();
                rptViewer.Show();
            }
        }
        public void rptABCPShift()
        {
            if (cmbProductShift.SelectedIndex==2)
            {
                string productionDate = dateTimePicker.Value.ToString("yyyyMMdd");

                rptABC objRptGeneral = new rptABC();
                frmRPTViewer rptViewer = new frmRPTViewer();
                DataSet dsGeneral = new DataSet();

                string strSql = "select * from t_pcpln where productiondate ='" + productionDate + "' and [SHIFTTYPE]='" + cmbProductShift.Text + "'  and RIGHT(LEFT(WIPITEMNO,5),2) = '" + cmbProductGroup.Text.Substring(0, 2) + "'   order by WIPITEMNO";

                dsGeneral = ReportData.getDataSet(strSql, "t_pcpln", "TCPL");

                objRptGeneral.SetDataSource(dsGeneral);

                objRptGeneral.VerifyDatabase();
                rptViewer.crystalReportViewer.ReportSource = objRptGeneral;
                rptViewer.Refresh();
                rptViewer.Show();
            }
        }
        // ****************************For Post status***************************
        private void btnPost_Click(object sender, EventArgs e)
        {
            productionDate = dateTimePicker.Value.ToString("yyyyMMdd");
            DataTable dtpost = new DataTable();
            dtpost = sq.get_rs("update t_pcpln set postatus = 1,poststatus = 2 where productiondate = '" + productionDate + "' and category = '" + cmbProductGroup.Text.Substring(3, 10) + "' and shifttype = '" + cmbProductShift.Text + "'");

            postButtonClicked = 1;
            buttonStatus();
            MessageBox.Show("Posted successfully");
            dataGridViewShow.ReadOnly = true;
            btnGo.PerformClick();            
            btnGoFillData();
        }
        private void test(object sender, DataGridViewCellEventArgs e)
        {
            rowIndex = dataGridViewShow.CurrentCell.RowIndex;
            columnIndex = dataGridViewShow.CurrentCell.ColumnIndex;       
            try
            {
                if (columnIndex == 6 && cmbProductShift.SelectedIndex == 0)
                 {
                    totalGeneral();
                    dataGridViewShow.Rows[rowIndex].Cells[columnIndex - 1].Value = dataGridViewShow.Rows[rowIndex].Cells[columnIndex].Value;
                    
                    //******** for fill up set  production cell******************
                    dataGridViewShow.Rows[rowIndex].Cells[columnIndex + 2].Value = dataGridViewShow.Rows[rowIndex].Cells[columnIndex].Value;
                    dataGridViewShow.Rows[rowIndex].Cells[columnIndex + 1].Value = dataGridViewShow.Rows[rowIndex].Cells[columnIndex].Value;                  
                }
                //***********for (reverse) set production general shift******************
                if (columnIndex == 8 && cmbProductShift.SelectedIndex == 0)
                {
                    totalGeneral();
                    dataGridViewShow.Rows[rowIndex].Cells[columnIndex -1].Value = dataGridViewShow.Rows[rowIndex].Cells[columnIndex].Value;                    

                }

                //*******************Day-Night Shift********************************
                if ((columnIndex == 6 || columnIndex == 7) && cmbProductShift.SelectedIndex == 1)
                {
                    totalDay_Night();
                    
                    //******** for fill up set  production Day-Night cell******************
                    dataGridViewShow.Rows[rowIndex].Cells[columnIndex + 3].Value = dataGridViewShow.Rows[rowIndex].Cells[columnIndex].Value;
                }

                if ((columnIndex == 9 || columnIndex == 10) && cmbProductShift.SelectedIndex == 1)
                {
                    totalDay_Night();
          
                }

                //*****************************A-B-C Shift**********************************
                if ((columnIndex == 6 || columnIndex == 7 ||columnIndex == 8) && cmbProductShift.SelectedIndex == 2)
                {
                    totalABC_Shift();
                    dataGridViewShow.Rows[rowIndex].Cells[columnIndex + 4].Value = dataGridViewShow.Rows[rowIndex].Cells[columnIndex].Value;
                }
                //******** for fill up set  production ABC shift cell******************
                if ((columnIndex == 10 || columnIndex == 11 || columnIndex == 12) && cmbProductShift.SelectedIndex == 2)
                {
                    totalABC_Shift();
                }

                else
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Environment.Exit(0);
            }
        }

        //*************Summation the grid cell value**************
        public void totalGeneral()
        {
            try
            {              
                rowIndex = dataGridViewShow.CurrentCell.RowIndex;
                columnIndex = dataGridViewShow.CurrentCell.ColumnIndex;

                double sum = 0;
                int totalRows = dataGridViewShow.RowCount - 1;
                for (int i = 0; i < totalRows; i++)
                {
                    sum = sum + Convert.ToDouble(dataGridViewShow.Rows[i].Cells[6].Value.ToString());
                }
                dataGridViewShow.Rows[totalRows].Cells[5].Value = sum;
                dataGridViewShow.Rows[totalRows].Cells[6].Value = sum;

                //*************for set production general shift cell value sum*******
                double sum1 = 0;
                int totalRows1 = dataGridViewShow.RowCount - 1;
                for (int i = 0; i < totalRows; i++)
                {
                    sum1 = sum1 + Convert.ToDouble(dataGridViewShow.Rows[i].Cells[8].Value.ToString());
                }
                dataGridViewShow.Rows[totalRows1].Cells[7].Value = sum1;
                dataGridViewShow.Rows[totalRows1].Cells[8].Value = sum1;
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());
                dataGridViewShow.Rows[rowIndex].Cells[columnIndex].Value = "0";
            }
        }
        public void totalDay_Night()
        {
            try
            {
                rowIndex = dataGridViewShow.CurrentCell.RowIndex;
                columnIndex = dataGridViewShow.CurrentCell.ColumnIndex;
                
                double sum = 0, subSum = 0, sumcol6 = 0, sumcol7 = 0;
                int totalRows = dataGridViewShow.RowCount - 1;

                for (int i = 0; i < totalRows; i++)
                {
                    sumcol6 += Convert.ToDouble(dataGridViewShow.Rows[i].Cells[6].Value);
                    sumcol7 += Convert.ToDouble(dataGridViewShow.Rows[i].Cells[7].Value);
                    subSum += (Convert.ToDouble(dataGridViewShow.Rows[i].Cells[6].Value)) + (Convert.ToDouble(dataGridViewShow.Rows[i].Cells[7].Value));
                    sum += subSum;
                    dataGridViewShow.Rows[i].Cells[5].Value = subSum;
                    subSum = 0;
                }
                dataGridViewShow.Rows[totalRows].Cells[5].Value = sum.ToString();
                dataGridViewShow.Rows[totalRows].Cells[6].Value = sumcol6.ToString();
                dataGridViewShow.Rows[totalRows].Cells[7].Value = sumcol7.ToString();

                //*****For set production Day-Night Shift cell value sum******************

                double sum1 = 0, subSum1 = 0, sumcol9 = 0, sumcol10 = 0;
                int totalRows1 = dataGridViewShow.RowCount - 1;

                for (int i = 0; i < totalRows1; i++)
                {
                    sumcol9 += Convert.ToDouble(dataGridViewShow.Rows[i].Cells[9].Value);
                    sumcol10 += Convert.ToDouble(dataGridViewShow.Rows[i].Cells[10].Value);
                    subSum1 += (Convert.ToDouble(dataGridViewShow.Rows[i].Cells[9].Value)) + (Convert.ToDouble(dataGridViewShow.Rows[i].Cells[10].Value));
                    sum1 += subSum1;
                    dataGridViewShow.Rows[i].Cells[8].Value = subSum1;
                    subSum1 = 0;
                }
                dataGridViewShow.Rows[totalRows1].Cells[8].Value = sum1.ToString();
                dataGridViewShow.Rows[totalRows1].Cells[9].Value = sumcol9.ToString();
                dataGridViewShow.Rows[totalRows1].Cells[10].Value = sumcol10.ToString();

            }
            catch (Exception ex)
            {
                //if (dataGridViewShow.CurrentCell.ColumnIndex == 6 && dataGridViewShow.CurrentCell.ColumnIndex == 7)
                //{
                //    dataGridViewShow.Rows[rowIndex].Cells[columnIndex].Value = "0";  
                //}
            }
        }
        public void totalABC_Shift()
        {
            try
            {
                rowIndex = dataGridViewShow.CurrentCell.RowIndex;
                columnIndex = dataGridViewShow.CurrentCell.ColumnIndex;
                double sum = 0, subSum = 0, sumcol6 = 0, sumcol7 = 0, sumcol8 = 0;
                int totalRows = dataGridViewShow.RowCount - 1;
                for (int i = 0; i < totalRows; i++)
                {
                    sumcol6 += Convert.ToDouble(dataGridViewShow.Rows[i].Cells[6].Value);
                    sumcol7 += Convert.ToDouble(dataGridViewShow.Rows[i].Cells[7].Value);
                    sumcol8 += Convert.ToDouble(dataGridViewShow.Rows[i].Cells[8].Value);
                    subSum += (Convert.ToDouble(dataGridViewShow.Rows[i].Cells[6].Value)) + (Convert.ToDouble(dataGridViewShow.Rows[i].Cells[7].Value)) + (Convert.ToInt32(dataGridViewShow.Rows[i].Cells[8].Value)); 
                    sum += subSum;
                    dataGridViewShow.Rows[i].Cells[5].Value = subSum;
                    subSum = 0;
                }
                dataGridViewShow.Rows[totalRows].Cells[5].Value = sum.ToString();
                dataGridViewShow.Rows[totalRows].Cells[6].Value = sumcol6.ToString();
                dataGridViewShow.Rows[totalRows].Cells[7].Value = sumcol7.ToString();
                dataGridViewShow.Rows[totalRows].Cells[8].Value = sumcol8.ToString();

                //*****For set production ABC Shift cell value sum******************
                double sum2 = 0, subSum2 = 0, sumcol10 = 0, sumcol11 = 0, sumcol12 = 0;
                int totalRows2 = dataGridViewShow.RowCount - 1;
                for (int i = 0; i < totalRows; i++)
                {
                    sumcol10 += Convert.ToDouble(dataGridViewShow.Rows[i].Cells[10].Value);
                    sumcol11 += Convert.ToDouble(dataGridViewShow.Rows[i].Cells[11].Value);
                    sumcol12 += Convert.ToDouble(dataGridViewShow.Rows[i].Cells[12].Value);
                    subSum2 += (Convert.ToDouble(dataGridViewShow.Rows[i].Cells[10].Value)) + (Convert.ToDouble(dataGridViewShow.Rows[i].Cells[11].Value)) + (Convert.ToInt32(dataGridViewShow.Rows[i].Cells[12].Value));
                    sum2 += subSum2;
                    dataGridViewShow.Rows[i].Cells[9].Value = subSum2;
                    subSum2 = 0;
                }
                dataGridViewShow.Rows[totalRows2].Cells[9].Value = sum2.ToString();
                dataGridViewShow.Rows[totalRows2].Cells[10].Value = sumcol10.ToString();
                dataGridViewShow.Rows[totalRows2].Cells[11].Value = sumcol11.ToString();
                dataGridViewShow.Rows[totalRows2].Cells[12].Value = sumcol12.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show("From Total ABC_Shift"+ex.Message);
            }
        }

        //**********************validation for only numeric number******************
          private void dataGridViewShow_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
          {
              try
              {
                  e.Control.KeyPress -= new KeyPressEventHandler(Column1_KeyPress);
                  if ((dataGridViewShow.CurrentCell.ColumnIndex == 6 || btnClickFlag==1) || (dataGridViewShow.CurrentCell.ColumnIndex == 7 && (cmbProductShift.SelectedIndex == 1 || cmbProductShift.SelectedIndex == 2)|| btnClickFlag==1) || (dataGridViewShow.CurrentCell.ColumnIndex == 8 && (cmbProductShift.SelectedIndex == 2))|| btnClickFlag==1)
                  {
                      TextBox tb = e.Control as TextBox;

                      if (tb != null)
                      {
                          tb.KeyPress += new KeyPressEventHandler(Column1_KeyPress);
                      }
                  }
              }
              catch (Exception err)
              {
                  MessageBox.Show(err.Message);
              }
          }
          private void Column1_KeyPress(object sender, KeyPressEventArgs e)
          {
              int col = dataGridViewShow.CurrentCell.ColumnIndex;
              int row = dataGridViewShow.CurrentCell.RowIndex;

              //MessageBox.Show(col.ToString());
              try
              {
                  if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar!= '.')
                  {
                      e.Handled = true;
                  }
              }
              catch (Exception er)
              {
                  MessageBox.Show(er.Message);
              }

              //*********************** For Enter key press**************************
              if (e.KeyChar == (char) (Keys.Enter))
              {                 
                  //************** general shift***************************************
                  if (row < 12 && col == 9 && cmbProductShift.SelectedIndex == 0 && btnClickFlag == 0)
                  {
                      SendKeys.Send("{left}");
                  }
                      //*********** General shift for view production*************//
                  else if (row < 12 && col == 9 && cmbProductShift.SelectedIndex == 0 && btnClickFlag == 1)
                  {
                      SendKeys.Send("{left}");
                      SendKeys.Send("{left}");
                      SendKeys.Send("{left}");
                  }
                  //**********************Day-Night shift****************************
                  else if (row < 12 && col == 11 && cmbProductShift.SelectedIndex == 1 && btnClickFlag == 0)
                  {
                      SendKeys.Send("{left}");
                      SendKeys.Send("{left}");
                  }
                  //*********** Day-Night shift for view production*************//
                  else if (row < 12 && col == 11 && cmbProductShift.SelectedIndex == 1 && btnClickFlag == 1)
                  {
                      SendKeys.Send("{left}");
                      SendKeys.Send("{left}");
                      SendKeys.Send("{left}");
                      SendKeys.Send("{left}");
                      SendKeys.Send("{left}");
                  }
                   //**********************ABC shift*********************************
                  else if (row < 12 && col == 13 && cmbProductShift.SelectedIndex == 2 && btnClickFlag == 0)
                  {
                      SendKeys.Send("{left}");
                      SendKeys.Send("{left}");
                      SendKeys.Send("{left}");
                  }
                  //*********** ABC shift for view production*************//
                  else if (row < 12 && col == 13 && cmbProductShift.SelectedIndex == 2 && btnClickFlag == 1)
                  {
                      SendKeys.Send("{left}");
                      SendKeys.Send("{left}");
                      SendKeys.Send("{left}");

                      SendKeys.Send("{left}");
                      SendKeys.Send("{left}");
                      SendKeys.Send("{left}");
                      SendKeys.Send("{left}");
                  }
                  else
                  {
                      SendKeys.Send("{up}");
                      SendKeys.Send("{right}");
                  }
              }
          }
           //*******************enter key will work  when cell value null********************
          private void dataGridViewShow_CellEndEdit(object sender, DataGridViewCellEventArgs e)
          {
              int row = dataGridViewShow.CurrentCell.RowIndex;
              int col = dataGridViewShow.CurrentCell.ColumnIndex;

              try
              {
                  if (row < 12 && col == 7 && cmbProductShift.SelectedIndex == 0)
                  {
                      SendKeys.Send("{left}");
                  }                     

                  else if (row < 12 && col == 8 && cmbProductShift.SelectedIndex == 1)
                  {
                      SendKeys.Send("{left}");
                      SendKeys.Send("{left}");
                  }

                  else if (row < 12 && col == 9 && cmbProductShift.SelectedIndex == 2)
                  {
                      SendKeys.Send("{left}");
                      SendKeys.Send("{left}");
                      SendKeys.Send("{left}");
                  }             
                  else
                  {
                      SendKeys.Send("{up}");
                      SendKeys.Send("{right}");
                  }
              }
              catch (Exception exception)
              {
                  //if (exception. == "NullReferenceException")
                  {
                      dataGridViewShow.Rows[row].Cells[col].Value = "0";
                      //MessageBox.Show("Cell Edit: "+exception.GetHashCode().ToString());
                  }                  
              }
          }
        public void GeneralGrid()
        {
            dataGridViewShow.Rows.Clear();
            dataGridViewShow.Columns.Clear();
            dataGridViewShow.Refresh();

            dataGridViewShow.ColumnCount = 10;
            string[] general = new string[]{"Sl No", "Item No", "Description", "UOM", "AOP QTY", "Plan QTY", "G Shift","Prd.Qty","G-Shift", "Comment"};

            int len = general.Length;
            for (int i = 0; i < len; i++)
            {
                dataGridViewShow.Columns[i].Name = general[i];
                if (i < 6)
                {
                    dataGridViewShow.Columns[i].ReadOnly = true;
                }
                dataGridViewShow.EditMode = DataGridViewEditMode.EditOnKeystroke;
            }
            dataGridViewShow.Columns["Sl No"].Width = 30;
            dataGridViewShow.Columns["Item No"].Width = 78;
            dataGridViewShow.Columns["Description"].Width = 248;
            dataGridViewShow.Columns["UOM"].Width = 35;
            dataGridViewShow.Columns["AOP QTY"].Width = 60;

            dataGridViewShow.Columns["Plan QTY"].Width = 65;
            dataGridViewShow.Columns["Plan QTY"].DefaultCellStyle.BackColor = Color.Aquamarine;
            dataGridViewShow.Columns["Plan QTY"].DefaultCellStyle.Alignment=DataGridViewContentAlignment.MiddleCenter;

            dataGridViewShow.Columns["G Shift"].Width = 65;
            dataGridViewShow.Columns["G Shift"].DefaultCellStyle.BackColor = Color.Aquamarine;
            dataGridViewShow.Columns["G Shift"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            dataGridViewShow.Columns["Comment"].Width = 180;

           dataGridViewShow.Columns["G-Shift"].Width=65;
           dataGridViewShow.Columns["Prd.Qty"].Width = 65;

           dataGridViewShow.Columns["G-Shift"].Visible = false;
           dataGridViewShow.Columns["Prd.Qty"].Visible = false;
        }
        public void DayNightGrid()
        {
            dataGridViewShow.ColumnCount = 12;

            string[] dayNight = new string[] { "Sl No", "Item No", "Description", "UOM", "AOP QTY", "Plan QTY", "D Shift", "N Shift","Prd.Qty","D-Shift","N-Shift", "Comment" };

            int len = dayNight.Length;
            for (int i = 0; i < len; i++)
            {
                dataGridViewShow.Columns[i].Name = dayNight[i];
                if (i < 6)
                {
                    dataGridViewShow.Columns[i].ReadOnly = true;
                }
                dataGridViewShow.EditMode = DataGridViewEditMode.EditOnKeystroke;
            }
            dataGridViewShow.Columns["Sl No"].Width = 30;
            dataGridViewShow.Columns["Item No"].Width = 78;
            dataGridViewShow.Columns["Description"].Width = 248;
            dataGridViewShow.Columns["UOM"].Width = 35;
            dataGridViewShow.Columns["AOP QTY"].Width = 60;

            dataGridViewShow.Columns["Plan QTY"].Width = 65;
            dataGridViewShow.Columns["Plan QTY"].DefaultCellStyle.BackColor = Color.Aquamarine;
            dataGridViewShow.Columns["Plan QTY"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            dataGridViewShow.Columns["D Shift"].Width = 65;
            dataGridViewShow.Columns["D Shift"].DefaultCellStyle.BackColor = Color.Aquamarine;
            dataGridViewShow.Columns["D Shift"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            dataGridViewShow.Columns["N Shift"].Width = 65;
            dataGridViewShow.Columns["N Shift"].DefaultCellStyle.BackColor = Color.Aquamarine;
            dataGridViewShow.Columns["N Shift"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            dataGridViewShow.Columns["Comment"].Width = 180;

            dataGridViewShow.Columns["N-Shift"].Width = 65;
            dataGridViewShow.Columns["D-Shift"].Width = 65;
            dataGridViewShow.Columns["Prd.Qty"].Width = 65;

            dataGridViewShow.Columns["N-Shift"].Visible = false;
            dataGridViewShow.Columns["D-Shift"].Visible = false;
            dataGridViewShow.Columns["Prd.Qty"].Visible = false;
        }
        public void ABCGrid()
        {
            dataGridViewShow.ColumnCount = 14;

            string[] abcShift = new string[] { "Sl No", "Item No", "Description", "UOM", "AOP QTY", "Plan QTY", "A Shift", "B Shift", "C Shift", "Prd.Qty", "A-Shift", "B-Shift", "C-Shift", "Comment" };

            int len = abcShift.Length;
            for (int i = 0; i < len; i++)
            {
                dataGridViewShow.Columns[i].Name = abcShift[i];
                if (i < 6)
                {
                    dataGridViewShow.Columns[i].ReadOnly = true;
                }
                dataGridViewShow.EditMode = DataGridViewEditMode.EditOnKeystroke;
            }
            dataGridViewShow.Columns["Sl No"].Width = 30;
            dataGridViewShow.Columns["Item No"].Width = 78;
            dataGridViewShow.Columns["Description"].Width = 248;
            dataGridViewShow.Columns["UOM"].Width = 35;
            dataGridViewShow.Columns["AOP QTY"].Width = 60;

            dataGridViewShow.Columns["Plan QTY"].Width = 65;
            dataGridViewShow.Columns["Plan QTY"].DefaultCellStyle.BackColor = Color.Aquamarine;
            dataGridViewShow.Columns["Plan QTY"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            dataGridViewShow.Columns["A Shift"].Width = 65;
            dataGridViewShow.Columns["A Shift"].DefaultCellStyle.BackColor = Color.Aquamarine;
            dataGridViewShow.Columns["A Shift"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            dataGridViewShow.Columns["B Shift"].Width = 65;
            dataGridViewShow.Columns["B Shift"].DefaultCellStyle.BackColor = Color.Aquamarine;
            dataGridViewShow.Columns["B Shift"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;


            dataGridViewShow.Columns["C Shift"].Width = 65;
            dataGridViewShow.Columns["C Shift"].DefaultCellStyle.BackColor = Color.Aquamarine;
            dataGridViewShow.Columns["C Shift"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            dataGridViewShow.Columns["Comment"].Width = 180;

            dataGridViewShow.Columns["A-Shift"].Width = 65;
            dataGridViewShow.Columns["B-Shift"].Width = 65;
            dataGridViewShow.Columns["C-Shift"].Width = 65;
            dataGridViewShow.Columns["Prd.Qty"].Width = 65;

            dataGridViewShow.Columns["A-Shift"].Visible = false;
            dataGridViewShow.Columns["B-Shift"].Visible = false;
            dataGridViewShow.Columns["C-Shift"].Visible = false;
            dataGridViewShow.Columns["Prd.Qty"].Visible = false;
       
        }
        public void buttonStatus()
        {
            if (oldRecord == false)
            {
                btnSave.Enabled = true;
                btnEdit.Enabled = false;
                btnPost.Enabled = false;
                return;
            }
            else if(oldRecord == true && enableEdit == false && POSTSTATUS == 1)
            {
                btnSave.Enabled = false;
                btnEdit.Enabled = true;
                btnSetProduction.Enabled = true;
                btnPost.Enabled = true;
            }
            else if(oldRecord == true && enableEdit == true && POSTSTATUS == 1)
            {

                btnSave.Enabled = true;
                btnEdit.Enabled = false;
                btnSetProduction.Enabled = true;
                btnPost.Enabled = true;
            }
            else if(POSTSTATUS == 2)
            {
                btnSave.Enabled = false;
                btnEdit.Enabled = false;
                btnSetProduction.Enabled = false;
                btnPost.Enabled = false;
            }
        }
        private void btnDelete_Click(object sender, EventArgs e)
        {       
            frmDeleteCheck testDialog = new frmDeleteCheck();
            testDialog.prodGroup = cmbProductGroup.Text;
            testDialog.shiftType = cmbProductShift.Text;
            testDialog.productiondate = dateTimePicker.Value.ToString("yyyyMMdd"); 

             if (testDialog.ShowDialog(this) == DialogResult.OK)
                {
                    // Read the contents of testDialog's TextBox.
                   // this.txtResult.Text = testDialog.TextBox1.Text;
                }
               else
                   {
                    //this.txtResult.Text = "Cancelled";
                   }
                testDialog.Dispose();
                btnGo.PerformClick();    // *******Call for btnGo Button*****
         }
        private void btnSetProduction_Click(object sender, EventArgs e)
        {
            if (cmbProductShift.Text == "General")
            {
                dataGridViewShow.Columns["G-Shift"].Visible = true;
                dataGridViewShow.Columns["Prd.Qty"].Visible = true;
            }
            else if (cmbProductShift.Text == "Day-Night")
            {
                dataGridViewShow.Columns["N-Shift"].Visible = true;
                dataGridViewShow.Columns["D-Shift"].Visible = true;
                dataGridViewShow.Columns["Prd.Qty"].Visible = true;
            }
            else if (cmbProductShift.Text == "A-B-C Shift")
            {
                dataGridViewShow.Columns["A-Shift"].Visible = true;
                dataGridViewShow.Columns["B-Shift"].Visible = true;
                dataGridViewShow.Columns["C-Shift"].Visible = true;
                dataGridViewShow.Columns["Prd.Qty"].Visible = true;
            }       
        }
        private void btnViewProduction_Click(object sender, EventArgs e)
        {
            btnClickFlag = 1;

            if (cmbProductShift.Text == "General")
            {
                dataGridViewShow.Columns["Prd.Qty"].ReadOnly = true;      
    
                if (dataGridViewShow.Columns["G-Shift"].Visible == true)
                {
                    //dataGridViewShow.Columns["G-Shift"].Width = 0;
                    dataGridViewShow.Columns["G-Shift"].Visible = false;
                    dataGridViewShow.Columns["Prd.Qty"].Visible = false;
                }
                else
                {
                    dataGridViewShow.Columns["G-Shift"].Visible = true;
                    dataGridViewShow.Columns["Prd.Qty"].Visible = true;
                }
            }
            else if (cmbProductShift.Text == "Day-Night")
            {
                dataGridViewShow.Columns["Prd.Qty"].ReadOnly = true;   

                if (dataGridViewShow.Columns["N-Shift"].Visible == true)
                {             
                    dataGridViewShow.Columns["N-Shift"].Visible = false;
                    dataGridViewShow.Columns["D-Shift"].Visible = false;
                    dataGridViewShow.Columns["Prd.Qty"].Visible = false;
                }
                else
                {
                    dataGridViewShow.Columns["N-Shift"].Visible = true;
                    dataGridViewShow.Columns["D-Shift"].Visible = true;
                    dataGridViewShow.Columns["Prd.Qty"].Visible = true;
                }
            }
            else if (cmbProductShift.Text == "A-B-C Shift")
            {
                dataGridViewShow.Columns["Prd.Qty"].ReadOnly = true;  
                if (dataGridViewShow.Columns["A-Shift"].Visible == true)
                {
                    dataGridViewShow.Columns["A-Shift"].Visible = false;
                    dataGridViewShow.Columns["B-Shift"].Visible = false;
                    dataGridViewShow.Columns["C-Shift"].Visible = false;
                    dataGridViewShow.Columns["Prd.Qty"].Visible = false;
                }
                else
                {
                    dataGridViewShow.Columns["A-Shift"].Visible = true;
                    dataGridViewShow.Columns["B-Shift"].Visible = true;
                    dataGridViewShow.Columns["C-Shift"].Visible = true;
                    dataGridViewShow.Columns["Prd.Qty"].Visible = true;
                }
            }
         }
      }
  }

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NewProductionPlan
{
    public partial class frmDeleteCheck : Form
    {
        public string prodGroup;
        public string shiftType;
        public string productiondate;
        public frmDeleteCheck()
        {
            InitializeComponent();
        }
        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        private void btnSubmit_Click(object sender, EventArgs e)
        {
            try
            {
                ProductionPlanUI x = new ProductionPlanUI();
             
                string xp = (Convert.ToInt32(productiondate.Substring(0, 4)) + 1).ToString() +(Convert.ToInt32(productiondate.Substring(4, 2))+1).ToString()+(Convert.ToInt32(productiondate.Substring(6, 2))+1).ToString();

                if (txtPwd.Text == xp)
                {
                    SQL sq = new SQL();
                    DataTable dtDelete = new DataTable();
                    dtDelete =
                        sq.get_rs("delete from t_pcpln where productiondate ='" + productiondate + "' and category = '" +
                                  prodGroup.Substring(3, 10) + "' and shifttype = '" + shiftType + "'");
                    MessageBox.Show("Delete successfull");
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Invalid Password!!");
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }
        private void frmDeleteCheck_Load(object sender, EventArgs e)
        {
            this.ActiveControl = txtPwd;
        }

        private void txtPwd_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                if (e.KeyChar == 13)
                {
                    btnSubmit.PerformClick();
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }
    }
}

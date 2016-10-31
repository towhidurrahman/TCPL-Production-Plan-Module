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
    public partial class frmLogIn : Form
    {
        public frmLogIn()
        {
            InitializeComponent();
        }
        private void btnClose_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            ProductionPlanUI mainForm = new ProductionPlanUI();

            AccpacCOMAPI.AccpacSession session = new AccpacCOMAPI.AccpacSession();
            // this.Cursor = Cursors.WaitCursor;
            session.Init("", "IC60A", "IC2000", "60A");
            try
            {
                session.Open(txtUserId.Text, txtPassword.Text, "TCPDAT", dateTimePicker1.Value, 0, "");

                AccpacCOMAPI.AccpacDBLink mDBLinkCmpRW = session.OpenDBLink(AccpacCOMAPI.tagDBLinkTypeEnum.DBLINK_COMPANY, AccpacCOMAPI.tagDBLinkFlagsEnum.DBLINK_FLG_READWRITE);
                AccpacCOMAPI.AccpacDBLink mDBLinkSysRW = session.OpenDBLink(AccpacCOMAPI.tagDBLinkTypeEnum.DBLINK_SYSTEM, AccpacCOMAPI.tagDBLinkFlagsEnum.DBLINK_FLG_READWRITE);
                mainForm.session = session;
                mainForm.mDBLinkCmpRW = mDBLinkCmpRW;
                mainForm.mDBLinkSysRW = mDBLinkSysRW;

                this.Hide();
                mainForm.Show();
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
        }

        private void txtUserId_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)  // 13 for enter key
            {
                txtPassword.Focus();
            }

            e.KeyChar = Char.ToUpper(e.KeyChar);
        }
        private void txtPassword_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                //enter key is down
                btnLogin.PerformClick();
            }
            e.KeyChar = Char.ToUpper(e.KeyChar);
        }
    }
}

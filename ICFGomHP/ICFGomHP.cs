using System;
using System.Collections.Generic;
using System.Text;
using Plugins;
using CDTDatabase;
using CDTLib;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using System.Data;

namespace ICFGomHP
{
    public class ICFGomHP:ICForm
    {
        private List<InfoCustomForm> _lstInfo = new List<InfoCustomForm>();
        private DataCustomFormControl _data;
        #region ICForm Members

        public ICFGomHP()
        {
            InfoCustomForm info = new InfoCustomForm(IDataType.MasterDetailDt, 1611, "Gom phí dư để hạch toán","", "MT11");
            _lstInfo.Add(info);
        }

        public DataCustomFormControl Data
        {
            set { _data = value; }
        }

        public void Execute(int menuID)
        {
            if (menuID == _lstInfo[0].MenuID)
            {
                XtraForm1 frm = new XtraForm1();
                frm.Text = "Gom phí dư";
                frm.ShowDialog();

                if (frm.dtHocPhi != null)
                {
                    GridView gv = ((_data.FrmMain.Controls.Find("gcMain", true)[0] as GridControl).MainView as GridView);

                    foreach (DataRow dr in frm.dtHocPhi.Rows)
                    {
                        gv.AddNewRow();
                        gv.SetFocusedRowCellValue(gv.Columns["Ps"], dr["TienDu"]);
                        gv.SetFocusedRowCellValue(gv.Columns["TkCo"], frm.MaTK);
                        gv.SetFocusedRowCellValue(gv.Columns["MaKHCt"], string.IsNullOrEmpty(dr["MaHV"].ToString()) ? "HVTN" : dr["MaHV"].ToString());
                        gv.SetFocusedRowCellValue(gv.Columns["TenKHCt"], dr["TenHV"]);
                        gv.SetFocusedRowCellValue(gv.Columns["DienGiaiCT"], "Gom phí dư");
                        gv.SetFocusedRowCellValue(gv.Columns["MaBP"], frm.MaCN);
                        gv.UpdateCurrentRow();
                    }
                }
            }
        }

        public List<InfoCustomForm> LstInfo
        {
            get { return _lstInfo; }
        }
        #endregion
    }
}

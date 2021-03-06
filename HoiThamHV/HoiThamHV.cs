using System;
using System.Collections.Generic;
using System.Text;
using CDTDatabase;
using CDTLib;
using DevExpress;
using DevExpress.XtraEditors;
using Plugins;
namespace HoiThamHV
{
    public class  HoiThamHV: IC
    {
        #region IC Members

        private List<InfoCustom> _lstInfo = new List<InfoCustom>();

        public HoiThamHV()
        {
            InfoCustom ic = new InfoCustom(1173, "Hỏi thăm học viên", "Quản lý học viên");
            _lstInfo.Add(ic);
        }

        public void Execute(System.Data.DataRow drMenu)
        {
            int menuID = Int32.Parse(drMenu["MenuPluginID"].ToString());
            if (_lstInfo[0].CType == ICType.Custom && _lstInfo[0].MenuID == menuID)
            {
                frmHoiTham frm = new frmHoiTham();
                frm.Text = "Hỏi thăm học viên";
                frm.ShowDialog();
            }
        }

        public List<InfoCustom> LstInfo
        {
            get { return _lstInfo; }
        }

        #endregion
    }
}

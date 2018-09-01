using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using CDTDatabase;
using CDTLib;
using Plugins;
using System.Data;

namespace AddColMonThi
{
    public class AddColMonThi : ICData
    {
        private InfoCustomData _info;
        private DataCustomData _data;
        Database db = Database.NewDataDatabase();
        Database dbStruct = Database.NewStructDatabase();
        #region ICData Members

        //Cột môn thi chạy theo cấu trúc: 
        // 3 ký tự đầu là: Col
        // Các ký tự sau là số tăng dần
        public AddColMonThi()
        {
            _info = new InfoCustomData(IDataType.Single);
        }

        public DataCustomData Data
        {
            set { _data = value; }
        }

        public InfoCustomData Info
        {
            get { return _info; }
        }
        
        public void ExecuteBefore()
        {
            string sField = "", sLable = "", defName = "", sql = "";
            int Index = 0;

            DataTable dt = _data.DsData.Tables[0];            
            DataRow drMaster = dt.Rows[_data.CurMasterIndex];

            if (drMaster.RowState == DataRowState.Deleted)
            {
                XtraMessageBox.Show("Hệ thống không cho phép xóa môn thi /nChọn ngưng sử dụng để hủy bỏ môn thi", Config.GetValue("PackageName").ToString());                
                return;
            }

            sField = drMaster["MaMT"].ToString();
            sLable = drMaster["TenMT"].ToString();
            Index = Convert.ToInt32(sField.Replace("Col", ""));
            defName = string.Format("DF_DMKQ_{0}", Index);

            if (drMaster.RowState == DataRowState.Added)
            {
                if (_data.CurMasterIndex < 0)
                    return;
                DataView dv = new DataView(_data.DsDataCopy.Tables[0]);
                DataTable dtDB = dbStruct.GetDataTable(@"SELECT	DBName2 
                                                     FROM	sysDatabase
                                                     WHERE	LEFT(DBName2,3) <> 'CDT'");
                dv.RowStateFilter = DataViewRowState.Added;

                // insert cấu trúc (CDT) sau đó tạo cột dữ liệu                
                sql = string.Format(@"BEGIN TRAN TRAN_ADDCOL
                                    DECLARE @TableID INT
                                    SET		@TableID = (SELECT	sysTableID FROM	sysTable WHERE	TableName = 'DMKQ')

                                    INSERT INTO sysField
                                               (sysTableID,FieldName,AllowNull,Type,LabelName,TabIndex
	                                            ,MaxValue,MinValue,DefaultValue,Visible,IsBottom,IsFixCol
	                                            ,IsGroupCol,SmartView,DefaultName,EditMask,IsUnique,Enable)
                                    VALUES(@TableID,N'{0}',0,8,'{1}','{2}',100,0,0,1,0,0,0,1,'{3}','###.##',0,1) "
                                    , sField, sLable, Index, defName);

                foreach (DataRow dr in dtDB.Rows)
                {
                    sql += string.Format(@" ALTER TABLE {0}..DMKQ 
                                        ADD [{1}] DECIMAL(20,6) NOT NULL CONSTRAINT [{2}]   DEFAULT 0 "
                                            , dr["DBName2"].ToString(), sField, defName);
                }

                sql += @"IF @@ERROR <> 0
                        ROLLBACK TRAN TRAN_ADDCOL
                    ELSE
                        COMMIT TRAN TRAN_ADDCOL";

                if (!dbStruct.UpdateByNonQuery(sql))
                    return;
            }
            else if (drMaster.RowState == DataRowState.Modified)
            {
                sql = string.Format(@"  UPDATE	sysField SET
		                                        LabelName = N'{0}'
                                        WHERE	sysTableID = (SELECT sysTableID FROM sysTable 
                                                              WHERE	 TableName = 'DMKQ')
		                                        AND FieldName = '{1}' ", sLable, sField);
                if (!dbStruct.UpdateByNonQuery(sql))
                    return;
            }
        }

        public void ExecuteAfter()
        {
        }

        #endregion

    }
}

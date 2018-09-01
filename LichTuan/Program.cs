using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CDTLib;

namespace LichTuan
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            ////lay style mac dinh cho form
            DevExpress.Skins.SkinManager.EnableFormSkins();
            DevExpress.LookAndFeel.DefaultLookAndFeel defaultLookAndFeelMain = new DevExpress.LookAndFeel.DefaultLookAndFeel();
            defaultLookAndFeelMain.LookAndFeel.SetSkinStyle("Money Twins");
            Config.NewKeyValue("NamLamViec", args[0]);
            Config.NewKeyValue("MaCN", args[1]);
            Config.NewKeyValue("STTD", args[2]);
            Config.NewKeyValue("PackageName", args[3]);
            Config.NewKeyValue("DataConnection", args[4]);
            //Config.NewKeyValue("NamLamViec", 2014);
            //Config.NewKeyValue("MaCN", "HS");
            //Config.NewKeyValue("STTD", 60);
            //Config.NewKeyValue("PackageName", "HTE");
            //Config.NewKeyValue("DataConnection", "Server=Server\\HoaTieu;Database=STDHTE5;user=sa;pwd=ht");
            FrmChonTuan frm = new FrmChonTuan();
            if (frm.ShowDialog() == DialogResult.OK)
                Application.Run(new FrmMain(frm.NgayBD, frm.NgayKT));
        }
    }
}
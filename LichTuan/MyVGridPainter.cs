using System;
using System.Drawing;
using DevExpress.XtraVerticalGrid.Painters;
using DevExpress.Utils;
using DevExpress.XtraVerticalGrid.Events;
using DevExpress.XtraVerticalGrid.ViewInfo;

namespace WindowsApplication1
{
    public class MyVGridPainter : VGridPainter
    {
        public MyVGridPainter(PaintEventHelper eventHelper)
            : base(eventHelper)
        {

        }


        protected override void DrawRowValueCellCore(CustomDrawRowValueCellEventArgs e, DevExpress.XtraEditors.Drawing.BaseEditPainter pb, DevExpress.XtraEditors.ViewInfo.BaseEditViewInfo bvi, DevExpress.XtraVerticalGrid.ViewInfo.BaseViewInfo vi)
        {
            if (e.Row.VisibleIndex == 0)        //for header row
            {
                Rectangle bounds = MergedRowsHelper.GetCellBounds(e, pb, bvi, vi);
                bvi.Bounds = bounds;
                bvi.CalcViewInfo(e.Graphics);
                EventHelper.DrawnCell.Bounds = bounds;
                EventHelper.DrawnCell.DrawFocusFrame = false;
                e.Appearance.Assign(vi.PaintAppearance.RowHeaderPanel);
            }
            base.DrawRowValueCellCore(e, pb, bvi, vi);
        }

        protected override void DrawLines(Lines LinesInfo, Rectangle client)
        {
            //base.DrawLines(LinesInfo, client);
        }

    }
}

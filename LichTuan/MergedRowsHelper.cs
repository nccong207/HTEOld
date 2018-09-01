using System;
using DevExpress.XtraVerticalGrid;
using DevExpress.XtraVerticalGrid.Events;
using System.Drawing;
using DevExpress.XtraVerticalGrid.ViewInfo;

namespace WindowsApplication1
{
    public static class MergedRowsHelper
    {

        public static Rectangle GetCellBounds(CustomDrawRowValueCellEventArgs e, DevExpress.XtraEditors.Drawing.BaseEditPainter pb, DevExpress.XtraEditors.ViewInfo.BaseEditViewInfo bvi, DevExpress.XtraVerticalGrid.ViewInfo.BaseViewInfo vi)
        {
            Rectangle result = e.Bounds;
            MergedType mergedType = GetMergedType(e);
            if (mergedType == MergedType.Regular)
                return result;
            if (mergedType == MergedType.Middle)
                return Rectangle.Empty;
            return GetFirstMergedCellBounds(e, vi);
        }


        private static Rectangle GetValueBoundsByRecordIndex(DevExpress.XtraVerticalGrid.ViewInfo.BaseRowViewInfo vInfo, int recordIndex)
        {
            Rectangle bounds = Rectangle.Empty;
            foreach (RowValueInfo valueInfo in vInfo.ValuesInfo)
            {
                if (valueInfo.RecordIndex == recordIndex)
                {
                    bounds = valueInfo.Bounds;
                    break;
                }
            }
            return bounds;
        }

        private static Rectangle GetFirstMergedCellBounds(CustomDrawRowValueCellEventArgs e, BaseViewInfo vi)
        {
            int recordIndex = e.RecordIndex + 1;
            while (NeigbourEqual(e, recordIndex))
            {
                recordIndex++;
            }
            BaseRowViewInfo ri = vi.RowsViewInfo[e.Row.Index] as BaseRowViewInfo;
            Rectangle lastCellBounds = GetValueBoundsByRecordIndex(ri, recordIndex);
            if (lastCellBounds.Right < e.Bounds.Left)
                lastCellBounds = ri.RowRect;
            return new Rectangle(e.Bounds.Left, e.Bounds.Top, lastCellBounds.Right - e.Bounds.Left, e.Bounds.Height);
        }

        public static MergedType GetMergedType(CustomDrawRowValueCellEventArgs e)
        {
            if (NeigbourEqual(e, e.RecordIndex - 1))
                return MergedType.Middle;
            if (NeigbourEqual(e, e.RecordIndex + 1))
                return MergedType.First;
            return MergedType.Regular;
        }
        private static bool NeigbourEqual(CustomDrawRowValueCellEventArgs e, int recordIndex)
        {
            if (recordIndex < 0 || e.RecordIndex < 0 || recordIndex >= e.Row.Grid.RecordCount)
                return false;
            if (e.CellValue == null)
                return e.Row.Grid.GetCellValue(e.Row, recordIndex) == null;
            return e.CellValue.Equals(e.Row.Grid.GetCellValue(e.Row, recordIndex));
        }
    }

    public enum MergedType
    {
        Middle,
        First,
        Regular
    }
}

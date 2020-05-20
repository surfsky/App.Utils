using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace App.Utils
{
    /// <summary>
    /// Excel 操作辅助类
    /// </summary>
    public class ExcelExporter
    {
        // 导出Excel文件
        public static void Export<T>(IList<T> objs, string fileName = "Export.xls", bool showFieldDescription=false)
        {
            fileName = HttpUtility.UrlEncode(fileName, Encoding.UTF8);
            HttpContext.Current.Response.ClearContent();
            HttpContext.Current.Response.ContentEncoding = Encoding.UTF8;
            HttpContext.Current.Response.ContentType = "application/vnd.ms-excel; charset=utf-8";
            HttpContext.Current.Response.AddHeader("Content-Disposition", "attachment;filename=" + fileName);
            HttpContext.Current.Response.Write(ExcelHelper.ToExcelXml<T>(objs, showFieldDescription)); // 还是用xml吧，每个字段都是字符串类型，避免客户输入不同格式的数据
            HttpContext.Current.Response.End();
        }

        // 导出Excel文件
        public static void Export(DataTable dt, string fileName = "Export.xls")
        {
            fileName = HttpUtility.UrlEncode(fileName, Encoding.UTF8);
            HttpContext.Current.Response.ClearContent();
            HttpContext.Current.Response.ContentEncoding = Encoding.UTF8;
            HttpContext.Current.Response.ContentType = "application/vnd.ms-excel; charset=utf-8";
            HttpContext.Current.Response.AddHeader("Content-Disposition", "attachment;filename=" + fileName);
            HttpContext.Current.Response.Write(ExcelHelper.ToExcelXml(dt)); // 还是用xml吧，每个字段都是字符串类型，避免客户输入不同格式的数据
            HttpContext.Current.Response.End();
        }

    }
}
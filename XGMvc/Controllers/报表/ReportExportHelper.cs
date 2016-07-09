using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using Svg;
using System.Drawing.Imaging;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Text;


namespace BaothApp.Controllers.报表
{
    public class ReportExportHelper
    {
        /// <summary>
        ///     TypeString:
        ///     image/png
        ///     image/jpeg
        ///     application/pdf
        ///     image/svg+xml
        /// </summary>
        private string TypeString { get; set; }
        /// <summary>
        ///     导出类型为JPG、PDF和SVG
        /// </summary>
        public ExportType ExportType { get; set; }
        /// <summary>
        ///     从页面获取导出的图形的数据
        /// </summary>
        public MemoryStream ExportData { get; set; }
        /// <summary>
        ///     导出文件名
        /// </summary>
        public string FileName { get; set; }


        public void Export(HttpResponse Response)
        {
            MemoryStream tStream = new MemoryStream();
            string tTmp = new Random().Next().ToString();
            string extName = "jpg"; //文件扩展名

            Svg.SvgDocument svgObj = SvgDocument.Open(ExportData);
            switch (ExportType)
            {
                case ExportType.JPG:
                    TypeString = "-m image/jpeg";
                    extName = "jpg";
                    svgObj.Draw().Save(tStream, ImageFormat.Jpeg);
                    break;
                case ExportType.PNG:
                    TypeString = "-m image/png";
                    extName = "png";
                    svgObj.Draw().Save(tStream, ImageFormat.Png);
                    break;
                case ExportType.PDF:
                    TypeString = "-m application/pdf";
                    extName = "pdf";
                    PdfWriter tWriter = null;
                    Document tDocumentPdf = null;
                    try
                    {
                        svgObj.Draw().Save(tStream, ImageFormat.Png);
                        tDocumentPdf = new Document(new Rectangle((float)svgObj.Width, (float)svgObj.Height));
                        tDocumentPdf.SetMargins(0.0f, 0.0f, 0.0f, 0.0f);
                        iTextSharp.text.Image tGraph = iTextSharp.text.Image.GetInstance(tStream.ToArray());
                        tGraph.ScaleToFit((float)svgObj.Width, (float)svgObj.Height);

                        tStream = new MemoryStream();
                        tWriter = PdfWriter.GetInstance(tDocumentPdf, tStream);
                        tDocumentPdf.Open();
                        tDocumentPdf.NewPage();
                        tDocumentPdf.Add(tGraph);
                        tDocumentPdf.CloseDocument();
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                    finally
                    {
                        tDocumentPdf.Close();
                        tDocumentPdf.Dispose();
                        tWriter.Close();
                        tWriter.Dispose();
                        ExportData.Dispose();
                        ExportData.Close();

                    }
                    break;

                case ExportType.SVG:
                    TypeString = "-m image/svg+xml";
                    extName = "svg";
                    tStream = ExportData;
                    break;
            }

            Response.ClearContent();
            Response.ClearHeaders();
            Response.ContentType = TypeString;
            Response.AppendHeader("Content-Disposition", "attachment; filename=" + FileName + "." + extName + "");
            Response.BinaryWrite(tStream.ToArray());
            Response.End();

        }

        public ReportExportHelper()
        {
            ExportType = ExportType.JPG;

        }
    }

    public enum ExportType
    {
        /// <summary>
        ///     导出JPG图片
        /// </summary>
        /// <remarks>
        ///     导出JPG图片
        /// </remarks>
        JPG,
        /// <summary>
        ///     导出PNG图片
        /// </summary>
        /// <remarks>
        ///     导出PNG图片
        /// </remarks>
        PNG,
        /// <summary>
        ///     导出PDF
        /// </summary>
        /// <remarks>
        ///     导出PDF
        /// </remarks>
        PDF,
        /// <summary>
        ///     导出SVG+XML
        /// </summary>
        /// <remarks>
        ///     
        /// </remarks>
        SVG


    }
}
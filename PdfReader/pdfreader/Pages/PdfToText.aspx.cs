using iTextSharp.text.pdf;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace PdfReader.Pages
{
    public partial class PdfToText : System.Web.UI.Page
    {
        #region Pageload
        protected void Page_Load(object sender, EventArgs e)
        {
            convertPdfToText();
        }
        #endregion

        #region pdf to text (file to file)
        internal static void convertPdfToText()
        {
            String inputFilePath = @"C:\demo.pdf";
            String outputFilePath = @"C:\output.txt";
            StreamWriter writer = new StreamWriter(outputFilePath);
            iTextSharp.text.pdf.PdfDocument doc = new iTextSharp.text.pdf.PdfDocument(inputFilePath);
            PDFTextMgr textMgr = PDFTextHandler.ExportPDFTextManager(doc);
            int pageCount = doc.GetPageCount();
            for (int i = 0; i < pageCount; i++)
            {
                PDFPage page = (PDFPage)doc.GetPage(i);
                List<PDFTextLine> pageTextLines = textMgr.ExtractTextLine(page);
                writeTextLines(pageTextLines, writer);
            }
            writer.Close();
        }
        #endregion

        #region pdf to text (stream to stream)
        internal static void convertPdfStreamToText()
        {
            String inputFilePath = @"C:\demo.pdf";
            byte[] arr = File.ReadAllBytes(inputFilePath);
            Stream inputStream = new MemoryStream(arr);
            Stream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            PdfDocument doc = new PDFDocument(inputStream);
            PDFTextMgr textMgr = PDFTextHandler.ExportPDFTextManager(doc);
            int pageCount = doc.GetPageCount();
            for (int i = 0; i < pageCount; i++)
            {
                PDFPage page = (PDFPage)doc.GetPage(i);
                List<PDFTextLine> pageTextLines = textMgr.ExtractTextLine(page);
                writeTextLines(pageTextLines, writer);
            }
            writer.Close();
        }
        #endregion

        #region write text to stream
        private static void writeTextLines(List<PDFTextLine> pageTextLines, StreamWriter writer)
        {
            String lineText = "";
            float positionY = 0f;
            float height = 0f;
            float positionX = 0f;

            #region current page do not contain images
            if (pageTextLines != null)
            {
                for (int i = 0; i < pageTextLines.Count; i++)
                {
                    RectangleF rectangle = pageTextLines[i].GetBoundary();
                    if (i != 0 && !isEqual(positionY + height, rectangle.Y + rectangle.Height))
                    {
                        writer.WriteLine(lineText);
                        lineText = "";
                    }
                    if (positionX > rectangle.X)
                    {
                        lineText = getTextLineContent(pageTextLines[i]) + " " + lineText;
                    }
                    else
                    {
                        lineText += getTextLineContent(pageTextLines[i]);
                        lineText += "    ";
                    }
                    positionY = rectangle.Y;
                    height = rectangle.Height;
                    positionX = rectangle.X;
                    if (i == pageTextLines.Count - 1)
                    {
                        writer.WriteLine(lineText);
                    }
                }
            }
            #endregion

            writer.WriteLine(" ");
            writer.WriteLine(" ");
            writer.Flush();
        }

        private static String getTextLineContent(PDFTextLine pdfTextLine)
        {
            List<PDFTextWord> words = pdfTextLine.GetTextWord();
            String wordText = "";
            float positionX = 0;
            float width = 0;
            for (int i = 0; i < words.Count; i++)
            {
                RectangleF rectange = words[i].GetBoundary();
                if (i != 0 && !isEqual(positionX + width, rectange.X))
                    wordText += " ";
                wordText += words[i].GetContent();
                positionX = rectange.X;
                width = rectange.Width;
            }

            return wordText;
        }

        private static bool isEqual(float first, float second)
        {
            if (first - second < 2F && first - second > -2F)
                return true;
            return false;
        }
        #endregion
    }
}
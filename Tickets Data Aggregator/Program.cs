using System;
using System.Globalization;
using System.IO;
using System.Linq.Expressions;
using System.Text;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.PageSegmenter;
using UglyToad.PdfPig.DocumentLayoutAnalysis.ReadingOrderDetector;
using UglyToad.PdfPig.DocumentLayoutAnalysis.WordExtractor;
using UglyToad.PdfPig.Fonts.Standard14Fonts;
using UglyToad.PdfPig.Writer;
using static System.Runtime.InteropServices.JavaScript.JSType;


string targetPath = @"C:\Users\boahd\Downloads\Tickets\TicketsVerkauf";
string[] filesPDFs = new PDFReader().FilesReader(targetPath);
new PDFReader().PDFReaderData(filesPDFs);
Console.ReadKey();
public class PDFReader
{
    public void PDFReaderData(string[] filesPDFs)
    {
        var pageNumber = 1;

        foreach (var file in filesPDFs)
        {
            using (PdfDocument document = PdfDocument.Open(file))
            {
                var builder = new PdfDocumentBuilder { };
                PdfDocumentBuilder.AddedFont font = builder.AddStandard14Font(Standard14Font.Helvetica);
                var pageBuilder = builder.AddPage(document, pageNumber);
                pageBuilder.SetStrokeColor(0, 255, 0);
                var page = document.GetPage(pageNumber);

                var letters = page.Letters; // no preprocessing

                // 1. Extract words
                var wordExtractor = NearestNeighbourWordExtractor.Instance;

                var words = wordExtractor.GetWords(letters);

                // 2. Segment page
                var pageSegmenter = DocstrumBoundingBoxes.Instance;

                var textBlocks = pageSegmenter.GetBlocks(words);

                // 3. Postprocessing
                var readingOrder = UnsupervisedReadingOrderDetector.Instance;
                var orderedTextBlocks = readingOrder.Get(textBlocks);

                // 4. Add debug info - Bounding boxes and reading order
                foreach (var block in orderedTextBlocks)
                {
                    var bbox = block.BoundingBox;
                    pageBuilder.DrawRectangle(bbox.BottomLeft, (decimal)bbox.Width, (decimal)bbox.Height);
                    pageBuilder.AddText(block.ReadingOrder.ToString(), 8, bbox.TopLeft, font);


                    if (block.ReadingOrder > 0 && block.ReadingOrder < orderedTextBlocks.Count() - 1)
                    {
                        switch (new string(orderedTextBlocks.Last().Text.Skip(24).ToArray()))
                        {

                            case "jp":
                                CultureInfo.CurrentCulture = new CultureInfo("ja-JP");
                                string dateString = block.TextLines[1].ToString().Substring(6);
                                DateTime date = DateTime.Parse(dateString);
                                PrinterInData(@"C:\Users\boahd\Downloads\Tickets\DATA\aggregatedTickets.txt", block, date);
                                break;

                            case "com":
                                CultureInfo.CurrentCulture = new CultureInfo("en-US");
                                string dateString2 = block.TextLines[1].ToString().Substring(6);
                                DateTime date2 = DateTime.Parse(dateString2);

                                PrinterInData(@"C:\Users\boahd\Downloads\Tickets\DATA\aggregatedTickets.txt", block, date2);
                                break;



                            case "fr":
                                CultureInfo.CurrentCulture = new CultureInfo("fr-FR");
                                string dateString3 = block.TextLines[1].ToString().Substring(6);
                                DateTime date3 = DateTime.Parse(dateString3);
                                PrinterInData(@"C:\Users\boahd\Downloads\Tickets\DATA\aggregatedTickets.txt", block, date3);
                                break;
                        }
                    }
                }

            }

        }
    }
    public string[] FilesReader(string targetPath)
    {
        return Directory.GetFiles(targetPath);
    }

    public void PrinterInData(string filePath, UglyToad.PdfPig.DocumentLayoutAnalysis.TextBlock block, DateTime date)
    {
        if (!File.Exists(filePath))
        {

            using (StreamWriter streamwriter = new StreamWriter(filePath, false))
            {
                streamwriter.WriteLine("{0,-30} | {1, -29} | {2, -10}", block.TextLines[0].ToString().Substring(7), date.ToString("d"), block.TextLines[2].ToString().Substring(6));
            }
        }
        else
        {
            using (StreamWriter writer = new StreamWriter(filePath, true))
            {
                writer.WriteLine("{0,-30} | {1, -29} | {2, -10}", block.TextLines[0].ToString().Substring(7), date.ToString("d"), block.TextLines[2].ToString().Substring(6));
            }
        }
    }
}

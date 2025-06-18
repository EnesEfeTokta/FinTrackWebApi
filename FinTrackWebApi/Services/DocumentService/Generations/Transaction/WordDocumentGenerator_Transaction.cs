using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using FinTrackWebApi.Services.DocumentService.Models;

namespace FinTrackWebApi.Services.DocumentService.Generations.Transaction
{
    public class WordDocumentGenerator_Transaction : IDocumentGenerator
    {
        public string FileExtension => ".docx";
        public string MimeType => "application/vnd.openxmlformats-officedocument.wordprocessingml.document";

        public Task<byte[]> GenerateAsync<TData>(TData data) where TData : class
        {
            if (!(data is TransactionsRaportModel reportData))
            {
                throw new ArgumentException($"Unsupported data type '{typeof(TData).FullName}' for Word generation. Expected TransactionsRaportModel.", nameof(data));
            }

            using (MemoryStream mem = new MemoryStream())
            {
                using (WordprocessingDocument wordDocument = WordprocessingDocument.Create(mem, WordprocessingDocumentType.Document, true)) // autoSave true
                {
                    MainDocumentPart mainPart = wordDocument.AddMainDocumentPart();
                    mainPart.Document = new Document();
                    Body body = mainPart.Document.AppendChild(new Body());

                    SectionProperties sectionProps = new SectionProperties();
                    PageMargin pageMargin = new PageMargin()
                    {
                        Top = 1080,
                        Right = (UInt32Value)1080U,
                        Bottom = 1080,
                        Left = (UInt32Value)1080U,
                        Header = (UInt32Value)720U,
                        Footer = (UInt32Value)720U,
                        Gutter = (UInt32Value)0U
                    };
                    sectionProps.Append(pageMargin);

                    AddParagraph(body, reportData.ReportTitle, justification: JustificationValues.Center, isBold: true, fontSize: "36");
                    AddParagraph(body, $"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}", justification: JustificationValues.Center, fontSize: "18");
                    if (!string.IsNullOrWhiteSpace(reportData.Description))
                    {
                        AddParagraph(body, "Description:", isBold: true, fontSize: "22", spaceAfter: "0");
                        AddParagraph(body, reportData.Description, fontSize: "22");
                    }
                    AddParagraph(body, "");

                    AddParagraph(body, "Transaction Details", isBold: true, fontSize: "28");
                    AddParagraph(body, "");

                    Table table = new Table();

                    TableProperties tblProps = new TableProperties();
                    TableWidth tableWidth = new TableWidth() { Width = "5000", Type = TableWidthUnitValues.Pct };
                    tblProps.Append(tableWidth);

                    tblProps.Append(new TableBorders(
                            new TopBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
                            new BottomBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
                            new LeftBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
                            new RightBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
                            new InsideHorizontalBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
                            new InsideVerticalBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 }
                        )
                    );
                    table.AppendChild(tblProps);

                    TableRow headerRow = new TableRow();
                    AddHeaderCell(headerRow, "#", fontSize: "20");
                    AddHeaderCell(headerRow, "Account Name", fontSize: "20");
                    AddHeaderCell(headerRow, "Category", fontSize: "20");
                    AddHeaderCell(headerRow, "Amount", JustificationValues.Right, fontSize: "20");
                    AddHeaderCell(headerRow, "Description", fontSize: "20");
                    AddHeaderCell(headerRow, "Transaction Date", JustificationValues.Center, fontSize: "20");
                    table.Append(headerRow);

                    if (reportData.Items != null && reportData.Items.Any())
                    {
                        int index = 1;
                        foreach (var item in reportData.Items)
                        {
                            TableRow dataRow = new TableRow();
                            AddTableCell(dataRow, index++.ToString(), fontSize: "18");
                            AddTableCell(dataRow, item.AccountName, fontSize: "18");
                            AddTableCell(dataRow, item.CategoryName, fontSize: "18");
                            AddTableCell(dataRow, item.Amount.ToString("N2"), JustificationValues.Right, fontSize: "18");
                            AddTableCell(dataRow, item.Description, fontSize: "18");
                            AddTableCell(dataRow, item.TransactionDateUtc.ToString("yyyy-MM-dd"), JustificationValues.Center, fontSize: "18");
                            table.Append(dataRow);
                        }
                    }
                    else
                    {
                        TableRow emptyRow = new TableRow();
                        TableCell emptyCell = AddTableCell(emptyRow, "No transaction details found.", fontSize: "18");
                        emptyCell.Append(new TableCellProperties(new GridSpan() { Val = 6 }));
                        emptyRow.Append(emptyCell);
                        table.Append(emptyRow);
                    }

                    body.AppendChild(table);
                    AddParagraph(body, "");

                    if (reportData.Items != null && reportData.Items.Any())
                    {
                        AddParagraph(body, $"Total Transactions Count: {reportData.TotalCount}", justification: JustificationValues.Right, fontSize: "20");
                    }

                    AddParagraph(body, "");

                    mainPart.Document.Body.Append(sectionProps);
                    mainPart.Document.Save();
                }
                return Task.FromResult(mem.ToArray());
            }
        }

        private static Paragraph AddParagraph(Body body, string text, string? fontSize = "22", bool isBold = false, JustificationValues? justification = null, string? spaceAfter = null, string? spaceBefore = null)
        {
            Paragraph para = new Paragraph();
            ParagraphProperties paraProps = new ParagraphProperties();

            if (justification.HasValue)
                paraProps.Append(new Justification { Val = justification.Value });

            SpacingBetweenLines spacing = new SpacingBetweenLines();
            bool hasSpacing = false;

            if (!string.IsNullOrEmpty(spaceAfter))
            {
                spacing.After = spaceAfter;
                hasSpacing = true;
            }
            if (!string.IsNullOrEmpty(spaceBefore))
            {
                spacing.Before = spaceBefore;
                hasSpacing = true;
            }
            if (hasSpacing)
                paraProps.Append(spacing);

            if (paraProps.HasChildren)
                para.PrependChild(paraProps);

            Run run = para.AppendChild(new Run());
            RunProperties runProps = run.AppendChild(new RunProperties());

            if (isBold)
                runProps.Append(new Bold());
            if (!string.IsNullOrEmpty(fontSize))
                runProps.Append(new FontSize { Val = fontSize });

            run.AppendChild(new Text(text) { Space = SpaceProcessingModeValues.Preserve });

            body.AppendChild(para);
            return para;
        }

        private static void AddHeaderCell(TableRow row, string text, JustificationValues? justification = null, string? fontSize = "20")
        {
            TableCell tc = new TableCell();

            TableCellProperties tcp = new TableCellProperties(
                new TableCellVerticalAlignment { Val = TableVerticalAlignmentValues.Center }
            );

            tc.Append(tcp);


            Paragraph p = new Paragraph();
            ParagraphProperties pPr = new ParagraphProperties();
            if (justification.HasValue)
            {
                pPr.Append(new Justification() { Val = justification.Value });
            }

            pPr.Append(new SpacingBetweenLines { LineRule = LineSpacingRuleValues.Auto, Line = "240", Before = "0", After = "0" });
            p.Append(pPr);

            Run run = p.AppendChild(new Run());
            RunProperties runProps = run.AppendChild(new RunProperties());
            runProps.Append(new Bold());
            if (!string.IsNullOrEmpty(fontSize))
                runProps.Append(new FontSize { Val = fontSize });

            run.AppendChild(new Text(text));

            tc.Append(p);
            row.Append(tc);
        }

        private static TableCell AddTableCell(TableRow row, string text, JustificationValues? justification = null, string? fontSize = "18")
        {
            TableCell tc = new TableCell();
            TableCellProperties tcp = new TableCellProperties(
                new TableCellVerticalAlignment { Val = TableVerticalAlignmentValues.Center }
            );

            tc.Append(tcp);

            Paragraph p = new Paragraph();
            ParagraphProperties pPr = new ParagraphProperties();

            if (justification.HasValue)
            {
                pPr.Append(new Justification() { Val = justification.Value });
            }

            pPr.Append(new SpacingBetweenLines { LineRule = LineSpacingRuleValues.Auto, Line = "240", Before = "0", After = "0" });
            p.Append(pPr);

            Run run = p.AppendChild(new Run());
            RunProperties runProps = run.AppendChild(new RunProperties());

            if (!string.IsNullOrEmpty(fontSize))
                runProps.Append(new FontSize { Val = fontSize });

            run.AppendChild(new Text(text) { Space = SpaceProcessingModeValues.Preserve });

            tc.Append(p);
            row.Append(tc);

            return tc;
        }
    }
}
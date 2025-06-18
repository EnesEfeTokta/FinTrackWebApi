using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using FinTrackWebApi.Services.DocumentService.Models;

namespace FinTrackWebApi.Services.DocumentService.Generations.Budget
{
    public class WordDocumentGenerator_Budget : IDocumentGenerator
    {
        public string FileExtension => ".docx";
        public string MimeType =>
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document";

        public Task<byte[]> GenerateAsync<TData>(TData data)
            where TData : class
        {
            if (!(data is BudgetReportModel reportData))
            {
                throw new ArgumentException(
                    $"Unsupported data type '{typeof(TData).FullName}' for Word generation. Expected BudgetReportModel.",
                    nameof(data)
                );
            }

            using (MemoryStream mem = new MemoryStream())
            {
                using (
                    WordprocessingDocument wordDocument = WordprocessingDocument.Create(
                        mem,
                        WordprocessingDocumentType.Document
                    )
                )
                {
                    MainDocumentPart mainPart = wordDocument.AddMainDocumentPart();
                    mainPart.Document = new Document();
                    Body body = mainPart.Document.AppendChild(new Body());

                    SectionProperties sectionProps = body.AppendChild(new SectionProperties());
                    PageMargin pageMargin = new PageMargin()
                    {
                        Top = 1080,
                        Right = (UInt32Value)1080U,
                        Bottom = 1080,
                        Left = (UInt32Value)1080U,
                        Header = (UInt32Value)720U,
                        Footer = (UInt32Value)720U,
                        Gutter = (UInt32Value)0U,
                    };
                    sectionProps.Append(pageMargin);

                    AddParagraph(
                        body,
                        reportData.ReportTitle,
                        justification: JustificationValues.Center,
                        isBold: true,
                        fontSize: "36"
                    );
                    AddParagraph(
                        body,
                        $"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}",
                        justification: JustificationValues.Center,
                        fontSize: "18"
                    );
                    AddParagraph(
                        body,
                        "Description:",
                        isBold: true,
                        fontSize: "20",
                        spaceAfter: "0"
                    );
                    AddParagraph(body, reportData.Description, fontSize: "20");
                    AddParagraph(body, "");

                    AddParagraph(body, "Budget Details", isBold: true, fontSize: "28");
                    AddParagraph(body, "");

                    Table table = new Table();

                    TableProperties tblProps = new TableProperties();
                    TableWidth tableWidth = new TableWidth()
                    {
                        Width = "5000",
                        Type = TableWidthUnitValues.Pct,
                    };
                    tblProps.Append(tableWidth);

                    tblProps.Append(
                        new TableBorders(
                            new TopBorder
                            {
                                Val = new EnumValue<BorderValues>(BorderValues.Single),
                                Size = 4,
                            },
                            new BottomBorder
                            {
                                Val = new EnumValue<BorderValues>(BorderValues.Single),
                                Size = 4,
                            },
                            new LeftBorder
                            {
                                Val = new EnumValue<BorderValues>(BorderValues.Single),
                                Size = 4,
                            },
                            new RightBorder
                            {
                                Val = new EnumValue<BorderValues>(BorderValues.Single),
                                Size = 4,
                            },
                            new InsideHorizontalBorder
                            {
                                Val = new EnumValue<BorderValues>(BorderValues.Single),
                                Size = 4,
                            },
                            new InsideVerticalBorder
                            {
                                Val = new EnumValue<BorderValues>(BorderValues.Single),
                                Size = 4,
                            }
                        )
                    );

                    table.AppendChild(tblProps);

                    TableRow headerRow = new TableRow();
                    AddHeaderCell(headerRow, "#");
                    AddHeaderCell(headerRow, "Name");
                    AddHeaderCell(headerRow, "Description");
                    AddHeaderCell(headerRow, "Category");
                    AddHeaderCell(headerRow, "Type");
                    AddHeaderCell(headerRow, "Start");
                    AddHeaderCell(headerRow, "End");
                    AddHeaderCell(headerRow, "Created");
                    AddHeaderCell(headerRow, "Updated");
                    AddHeaderCell(headerRow, "Allocated", JustificationValues.Right);
                    table.Append(headerRow);

                    int index = 1;
                    foreach (var item in reportData.Items)
                    {
                        TableRow dataRow = new TableRow();
                        AddTableCell(dataRow, index++.ToString());
                        AddTableCell(dataRow, item.Name);
                        AddTableCell(dataRow, item.Description);
                        AddTableCell(dataRow, item.Category);
                        AddTableCell(dataRow, item.Type);
                        AddTableCell(dataRow, item.StartDate.ToString("yyyy-MM-dd"));
                        AddTableCell(dataRow, item.EndDate.ToString("yyyy-MM-dd"));
                        AddTableCell(dataRow, item.CreatedAt.ToString("yyyy-MM-dd"));
                        AddTableCell(
                            dataRow,
                            item.UpdatedAt == DateTime.MinValue || item.UpdatedAt == default
                                ? "-"
                                : item.UpdatedAt.ToString("yyyy-MM-dd")
                        );
                        AddTableCell(
                            dataRow,
                            item.AllocatedAmount.ToString(),
                            JustificationValues.Right
                        );
                        table.Append(dataRow);
                    }

                    body.AppendChild(table);

                    AddParagraph(body, "");

                    AddParagraph(
                        body,
                        $"Page 1 of X",
                        justification: JustificationValues.Center,
                        fontSize: "16"
                    );
                }

                return Task.FromResult(mem.ToArray());
            }
        }

        private static Paragraph AddParagraph(
            Body body,
            string text,
            string? fontSize = "20",
            bool isBold = false,
            JustificationValues? justification = null,
            string? spaceAfter = null
        )
        {
            Paragraph para = new Paragraph();
            Run run = para.AppendChild(new Run());
            RunProperties runProps = run.AppendChild(new RunProperties());

            if (isBold)
                runProps.Append(new Bold());
            if (!string.IsNullOrEmpty(fontSize))
                runProps.Append(new FontSize { Val = fontSize });

            run.AppendChild(new Text(text) { Space = SpaceProcessingModeValues.Preserve });

            ParagraphProperties paraProps = new ParagraphProperties();
            if (justification.HasValue)
                paraProps.Append(new Justification { Val = justification.Value });
            if (!string.IsNullOrEmpty(spaceAfter))
                paraProps.Append(new SpacingBetweenLines { After = spaceAfter });
            if (paraProps.HasChildren)
                para.PrependChild(paraProps);

            body.AppendChild(para);
            return para;
        }

        private static void AddHeaderCell(
            TableRow row,
            string text,
            JustificationValues? justification = null
        )
        {
            TableCell tc = new TableCell();

            TableCellProperties tcp = new TableCellProperties();
            tc.Append(tcp);

            Paragraph p = new Paragraph();
            ParagraphProperties pPr = new ParagraphProperties();
            if (justification.HasValue)
            {
                pPr.Append(new Justification() { Val = justification.Value });
            }

            pPr.Append(new SpacingBetweenLines { After = "0" });
            p.Append(pPr);

            Run run = p.AppendChild(new Run());
            RunProperties runProps = run.AppendChild(new RunProperties());
            runProps.Append(new Bold());
            runProps.Append(new FontSize { Val = "18" });
            run.AppendChild(new Text(text));

            tc.Append(p);
            row.Append(tc);
        }

        private static void AddTableCell(
            TableRow row,
            string text,
            JustificationValues? justification = null
        )
        {
            TableCell tc = new TableCell();

            TableCellProperties tcp = new TableCellProperties();
            tc.Append(tcp);

            Paragraph p = new Paragraph();
            ParagraphProperties pPr = new ParagraphProperties();
            if (justification.HasValue)
            {
                pPr.Append(new Justification() { Val = justification.Value });
            }
            pPr.Append(new SpacingBetweenLines { After = "0" });
            p.Append(pPr);

            Run run = p.AppendChild(new Run());
            RunProperties runProps = run.AppendChild(new RunProperties());
            runProps.Append(new FontSize { Val = "18" });
            run.AppendChild(new Text(text) { Space = SpaceProcessingModeValues.Preserve });

            tc.Append(p);
            row.Append(tc);
        }
    }
}

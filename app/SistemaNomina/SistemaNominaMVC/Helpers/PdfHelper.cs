using DinkToPdf;
using DinkToPdf.Contracts;
using System.IO;

namespace SistemaNominaMVC.Helpers
{
    public static class PdfHelper
    {
        private static IConverter _converter;

        public static void Configure(IConverter converter)
        {
            _converter = converter;
        }

        public static byte[] GeneratePdf(string htmlContent, string titulo)
        {
            var doc = new HtmlToPdfDocument()
            {
                GlobalSettings = {
                    ColorMode = ColorMode.Color,
                    Orientation = Orientation.Portrait,
                    PaperSize = PaperKind.A4,
                    DocumentTitle = titulo,
                    Out = null
                },
                Objects = {
                    new ObjectSettings() {
                        PagesCount = true,
                        HtmlContent = htmlContent,
                        WebSettings = { DefaultEncoding = "utf-8" },
                        HeaderSettings = {
                            FontSize = 9,
                            Right = "Página [page] de [toPage]",
                            Line = true,
                            Spacing = 2.5
                        },
                        FooterSettings = {
                            FontSize = 9,
                            Center = $"Generado: {DateTime.Now:dd/MM/yyyy HH:mm:ss}",
                            Line = true,
                            Spacing = 2.5
                        }
                    }
                }
            };

            return _converter.Convert(doc);
        }

        public static string GenerateHtmlTable(string titulo, string[] headers, List<string[]> rows)
        {
            var html = $@"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset='utf-8'>
                <style>
                    body {{ font-family: Arial, sans-serif; margin: 20px; }}
                    h1 {{ color: #4e73df; text-align: center; }}
                    .fecha {{ text-align: right; color: #666; margin-bottom: 20px; }}
                    table {{ width: 100%; border-collapse: collapse; margin-top: 20px; }}
                    th {{ background-color: #4e73df; color: white; padding: 10px; text-align: left; }}
                    td {{ padding: 8px; border-bottom: 1px solid #ddd; }}
                    tr:nth-child(even) {{ background-color: #f2f2f2; }}
                    .total {{ font-weight: bold; background-color: #e2e3e5; }}
                </style>
            </head>
            <body>
                <h1>{titulo}</h1>
                <div class='fecha'>Generado: {DateTime.Now:dd/MM/yyyy HH:mm:ss}</div>
                <table>
                    <thead>
                        <tr>";

            foreach (var header in headers)
            {
                html += $"<th>{header}</th>";
            }

            html += "</tr></thead><tbody>";

            foreach (var row in rows)
            {
                html += "<tr>";
                foreach (var cell in row)
                {
                    html += $"<td>{cell}</td>";
                }
                html += "</tr>";
            }

            html += "</tbody></table></body></html>";
            return html;
        }
    }
}
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using System.IO;
using System.Collections.Generic;

namespace SistemaNominaMVC.Helpers
{
    public static class ExcelHelper
    {
        static ExcelHelper()
        {
            // Para EPPlus 4.5.3.3 no se necesita licencia
        }

        public static byte[] GenerateExcel<T>(string titulo, string[] headers, List<T> data, System.Func<T, object[]> rowMapper)
        {
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add(titulo.Length > 30 ? titulo.Substring(0, 30) : titulo);

                // Título del reporte
                worksheet.Cells[1, 1].Value = titulo;
                worksheet.Cells[1, 1, 1, headers.Length].Merge = true;
                worksheet.Cells[1, 1].Style.Font.Size = 16;
                worksheet.Cells[1, 1].Style.Font.Bold = true;
                worksheet.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[1, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[1, 1].Style.Fill.BackgroundColor.SetColor(Color.LightBlue);

                // Fecha del reporte
                worksheet.Cells[2, 1].Value = $"Generado: {DateTime.Now:dd/MM/yyyy HH:mm:ss}";
                worksheet.Cells[2, 1, 2, headers.Length].Merge = true;
                worksheet.Cells[2, 1].Style.Font.Italic = true;

                // Encabezados
                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cells[4, i + 1].Value = headers[i];
                    worksheet.Cells[4, i + 1].Style.Font.Bold = true;
                    worksheet.Cells[4, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[4, i + 1].Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                    worksheet.Cells[4, i + 1].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }

                // Datos
                int row = 5;
                foreach (var item in data)
                {
                    var values = rowMapper(item);
                    for (int i = 0; i < values.Length; i++)
                    {
                        worksheet.Cells[row, i + 1].Value = values[i];
                        worksheet.Cells[row, i + 1].Style.Border.BorderAround(ExcelBorderStyle.Thin);

                        // Si es valor numérico (decimal), formatear como moneda
                        if (values[i] is string str && str.StartsWith("$"))
                        {
                            worksheet.Cells[row, i + 1].Style.Numberformat.Format = "$#,##0.00";
                        }
                    }
                    row++;
                }

                // Autoajustar columnas
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                return package.GetAsByteArray();
            }
        }

        // Método alternativo para generar Excel directamente desde listas
        public static byte[] GenerateExcelFromRows(string titulo, string[] headers, List<object[]> rows)
        {
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add(titulo.Length > 30 ? titulo.Substring(0, 30) : titulo);

                // Título
                worksheet.Cells[1, 1].Value = titulo;
                worksheet.Cells[1, 1, 1, headers.Length].Merge = true;
                worksheet.Cells[1, 1].Style.Font.Size = 16;
                worksheet.Cells[1, 1].Style.Font.Bold = true;
                worksheet.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[1, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[1, 1].Style.Fill.BackgroundColor.SetColor(Color.LightBlue);

                // Fecha
                worksheet.Cells[2, 1].Value = $"Generado: {DateTime.Now:dd/MM/yyyy HH:mm:ss}";
                worksheet.Cells[2, 1, 2, headers.Length].Merge = true;

                // Encabezados
                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cells[4, i + 1].Value = headers[i];
                    worksheet.Cells[4, i + 1].Style.Font.Bold = true;
                    worksheet.Cells[4, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[4, i + 1].Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                }

                // Datos
                int row = 5;
                foreach (var rowData in rows)
                {
                    for (int i = 0; i < rowData.Length; i++)
                    {
                        worksheet.Cells[row, i + 1].Value = rowData[i];
                    }
                    row++;
                }

                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                return package.GetAsByteArray();
            }
        }
    }
}
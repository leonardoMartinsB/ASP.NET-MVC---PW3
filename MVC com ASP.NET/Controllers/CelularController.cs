using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Globalization;

using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.IO.Font.Constants;
using iText.Kernel.Font;
using iText.Layout.Borders;
using Microsoft.AspNetCore.Hosting;
using System.IO;

using ClosedXML.Excel; // exel

namespace WebApplication1.Controllers
{
    public class CelularController : Controller
    {
        private static List<Celular> _celulares = new List<Celular>
        {
            new Celular { Id = 1, Marca = "Apple", Modelo = "iPhone 14", Preco = 7000, Data = new DateTime(2023, 10, 1) },
            new Celular { Id = 2, Marca = "Samsung", Modelo = "Galaxy S23", Preco = 6500, Data = new DateTime(2023, 9, 15) },
            new Celular { Id = 3, Marca = "Xiaomi", Modelo = "Mi 12", Preco = 4000, Data = new DateTime(2023, 8, 20) },
            new Celular { Id = 4, Marca = "Motorola", Modelo = "Edge 40", Preco = 3500, Data = new DateTime(2023, 7, 10) },
            new Celular { Id = 5, Marca = "Huawei", Modelo = "P50 Pro", Preco = 5000, Data = new DateTime(2023, 6, 5) },
            new Celular { Id = 6, Marca = "Oppo", Modelo = "Find X5", Preco = 6000, Data = new DateTime(2023, 5, 18) },
            new Celular { Id = 7, Marca = "Realme", Modelo = "GT 2 Pro", Preco = 4500, Data = new DateTime(2023, 4, 25) },
            new Celular { Id = 8, Marca = "OnePlus", Modelo = "9 Pro", Preco = 5500, Data = new DateTime(2023, 3, 30) },
            new Celular { Id = 9, Marca = "Google", Modelo = "Pixel 7", Preco = 6200, Data = new DateTime(2023, 2, 10) },
            new Celular { Id = 10, Marca = "Sony", Modelo = "Xperia 1 IV", Preco = 7500, Data = new DateTime(2023, 1, 15) }
        };



        public IActionResult Index() => View(_celulares);

        public IActionResult Visualizar(int id)
        {
            var celular = _celulares.FirstOrDefault(c => c.Id == id);
            return celular == null ? NotFound() : View(celular);
        }

        public IActionResult Cadastrar() => View();

        [HttpPost]
        public IActionResult Cadastrar(Celular celular)
        {
            if (ModelState.IsValid)
            {
                celular.Preco = RemoverFormatacaoPreco(celular.Preco.ToString());
                celular.Id = _celulares.Any() ? _celulares.Max(c => c.Id) + 1 : 1;
                _celulares.Add(celular);
                return RedirectToAction("Index");
            }
            return View(celular);
        }

        public IActionResult Editar(int id)
        {
            var celular = _celulares.FirstOrDefault(c => c.Id == id);
            return celular == null ? NotFound() : View(celular);
        }

        [HttpPost]
        public IActionResult Editar(Celular celular)
        {
            var existente = _celulares.FirstOrDefault(c => c.Id == celular.Id);
            if (existente == null)
                return NotFound();

            existente.Marca = celular.Marca;
            existente.Modelo = celular.Modelo;
            existente.Preco = celular.Preco;
            existente.Data = celular.Data;

            return RedirectToAction("Index");
        }


        public IActionResult Excluir(int id)
        {
            var celular = _celulares.FirstOrDefault(c => c.Id == id);
            return celular == null ? NotFound() : View(celular);
        }

        [HttpPost]
        public IActionResult ExcluirConfirmado(int id)
        {
            var celular = _celulares.FirstOrDefault(c => c.Id == id);
            if (celular != null)
            {
                _celulares.Remove(celular);
                return RedirectToAction("Index");
            }
            return NotFound();
        }

        private decimal RemoverFormatacaoPreco(string preco)
        {
            try
            {

                if (decimal.TryParse(preco, out decimal valorDireto))
                {
                    Console.WriteLine($"Valor convertido diretamente: {valorDireto}");
                    return valorDireto;
                }

                string precoLimpo = preco
                    .Replace("R$", "")
                    .Replace(" ", "")
                    .Replace(".", "")
                    .Replace(",", ".");

                if (decimal.TryParse(precoLimpo, out decimal resultado))
                {
                    Console.WriteLine($"Valor após limpeza de formatação: {resultado}");
                    return resultado;
                }

                Console.WriteLine("Não foi possível converter o preço, retornando zero");
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao processar o preço: {ex.Message}");
                return 0;
            }
        }

        public IActionResult BaixarPDF()
        {
            using (var memoryStream = new MemoryStream())
            {
                var writer = new PdfWriter(memoryStream);
                var pdf = new PdfDocument(writer);
                var document = new Document(pdf);

                // Fontes
                PdfFont normalFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
                PdfFont boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

                document.SetFont(normalFont);

                // Título
                document.Add(new Paragraph("Lista de Celulares")
                    .SetFont(boldFont)
                    .SetFontSize(20)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetMarginBottom(20));

                // Tabela com 5 colunas
                Table table = new Table(UnitValue.CreatePercentArray(new float[] { 1, 3, 3, 3, 2 })).UseAllAvailableWidth();

                // Cabeçalho
                table.AddHeaderCell(CreateHeaderCell("ID", boldFont));
                table.AddHeaderCell(CreateHeaderCell("Marca", boldFont));
                table.AddHeaderCell(CreateHeaderCell("Modelo", boldFont));
                table.AddHeaderCell(CreateHeaderCell("Data de Fabricação", boldFont));
                table.AddHeaderCell(CreateHeaderCell("Preço", boldFont));

                // Dados
                foreach (var celular in _celulares)
                {
                    table.AddCell(CreateCell(celular.Id.ToString(), normalFont));
                    table.AddCell(CreateCell(celular.Marca, normalFont));
                    table.AddCell(CreateCell(celular.Modelo, normalFont));
                    table.AddCell(CreateCell(celular.Data.ToString("dd/MM/yyyy"), normalFont));
                    table.AddCell(CreateCell(celular.Preco.ToString("C", CultureInfo.GetCultureInfo("pt-BR")), normalFont));
                }

                document.Add(table);
                document.Close();

                return File(memoryStream.ToArray(), "application/pdf", "ListaDeCelulares.pdf");
            }
        }

        // Estilo cabeçalho
        private Cell CreateHeaderCell(string content, PdfFont font)
        {
            return new Cell()
                .Add(new Paragraph(content).SetFont(font))
                .SetBackgroundColor(iText.Kernel.Colors.ColorConstants.LIGHT_GRAY)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetBorder(new SolidBorder(1));
        }

        // Estilo conteúdo
        private Cell CreateCell(string content, PdfFont font)
        {
            return new Cell()
                .Add(new Paragraph(content).SetFont(font))
                .SetTextAlignment(TextAlignment.LEFT)
                .SetBorder(new SolidBorder(0.5f));
        }

        public IActionResult BaixarExcel()
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Celulares");

                // Cabeçalhos
                worksheet.Cell(1, 1).Value = "ID";
                worksheet.Cell(1, 2).Value = "Marca";
                worksheet.Cell(1, 3).Value = "Modelo";
                worksheet.Cell(1, 4).Value = "Preço";
                worksheet.Cell(1, 5).Value = "Data de Lançamento";

                // Estilo do cabeçalho
                var headerRange = worksheet.Range(1, 1, 1, 5);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                // Dados
                int linha = 2;
                foreach (var celular in _celulares) // Alterado de _carros para _celulares
                {
                    worksheet.Cell(linha, 1).Value = celular.Id;
                    worksheet.Cell(linha, 2).Value = celular.Marca;
                    worksheet.Cell(linha, 3).Value = celular.Modelo;
                    worksheet.Cell(linha, 4).Value = celular.Preco.ToString("C");
                    worksheet.Cell(linha, 5).Value = celular.Data.ToString("dd/MM/yyyy"); // Data de Lançamento

                    linha++;
                }

                // Ajustar tamanhos das colunas automaticamente
                worksheet.Columns().AdjustToContents();

                // Centralizar as células de ID e Data
                worksheet.Column(1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Column(5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Bordas para toda a tabela
                var dataRange = worksheet.Range(1, 1, linha - 1, 5);
                dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ListaDeCelulares.xlsx"); // Nome alterado para "ListaDeCelulares.xlsx"
                }
            }
        }

    }
}
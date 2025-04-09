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

namespace WebApplication1.Controllers
{
    public class CelularController : Controller
    {
        private static List<Celular> _celulares = new List<Celular>
        {
            new Celular { Id = 1, Marca = "Apple", Modelo = "iPhone 14", Preco = 7000, Data = new DateTime(2023, 10, 1) },
            new Celular { Id = 2, Marca = "Samsung", Modelo = "Galaxy S23", Preco = 6500, Data = new DateTime(2023, 9, 15) },
            new Celular { Id = 3, Marca = "Xiaomi", Modelo = "Mi 12", Preco = 4000, Data = new DateTime(2023, 8, 20) }
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
    }
}
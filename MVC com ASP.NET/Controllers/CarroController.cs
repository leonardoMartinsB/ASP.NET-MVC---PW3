using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

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
    public class CarroController : Controller
    {
        private static List<Carro> _carros = new List<Carro>
        {
            new Carro { Id = 1, Marca = "Tesla", Modelo = "Model S", Cor = "Branco", Placa = "ABC1234", Ano = new DateTime(2023, 03, 15), Preco = 2500000 },
            new Carro { Id = 2, Marca = "BMW", Modelo = "X5", Cor = "Preto", Placa = "XYZ5678", Ano = new DateTime(2022, 06, 10), Preco = 580000 },
            new Carro { Id = 3, Marca = "Audi", Modelo = "A4", Cor = "Prata", Placa = "LMN9101", Ano = new DateTime(2021, 08, 25), Preco = 180000 }
        };
        public IActionResult Index() => View(_carros);

        public IActionResult Visualizar(int id)
        {
            var carro = _carros.FirstOrDefault(c => c.Id == id);
            return carro == null ? NotFound() : View(carro);
        }

        public IActionResult Cadastrar() => View();

        [HttpPost]
        public IActionResult Cadastrar(Carro carro)
        {
            if (!ModelState.IsValid)
                return View(carro);

            carro.Placa = RemoverFormatacaoPlaca(carro.Placa);
            carro.Preco = RemoverFormatacaoPreco(carro.Preco.ToString());

            carro.Id = _carros.Any() ? _carros.Max(c => c.Id) + 1 : 1;
            _carros.Add(carro);

            return RedirectToAction("Index");
        }

        public IActionResult Editar(int id)
        {
            var carro = _carros.FirstOrDefault(c => c.Id == id);
            if (carro == null)
                return NotFound();

            var carroViewModel = new Carro
            {
                Id = carro.Id,
                Marca = carro.Marca,
                Modelo = carro.Modelo,
                Cor = carro.Cor,
                Ano = carro.Ano,
                Preco = carro.Preco,
                Placa = FormatarPlaca(carro.Placa)
            };

            return View(carroViewModel);
        }

        [HttpPost]
        public IActionResult Editar(Carro carro)
        {
            if (!ModelState.IsValid)
            {
                Console.WriteLine("ModelState Inválido!");
                return View(carro);
            }

            var carroExistente = _carros.FirstOrDefault(c => c.Id == carro.Id);
            if (carroExistente != null)
            {
                carroExistente.Marca = carro.Marca;
                carroExistente.Modelo = carro.Modelo;
                carroExistente.Cor = carro.Cor;
                carroExistente.Ano = carro.Ano;
                carroExistente.Preco = RemoverFormatacaoPreco(carro.Preco.ToString());
                carroExistente.Placa = RemoverFormatacaoPlaca(carro.Placa);

                Console.WriteLine($"Carro salvo com sucesso! Preço: {carroExistente.Preco}");
                return RedirectToAction("Index");
            }

            return NotFound();
        }

        public IActionResult Excluir(int id)
        {
            var carro = _carros.FirstOrDefault(c => c.Id == id);
            return carro == null ? NotFound() : View(carro);
        }

        [HttpPost]
        public IActionResult ExcluirConfirmado(int id)
        {
            var carro = _carros.FirstOrDefault(c => c.Id == id);
            if (carro != null)
            {
                _carros.Remove(carro);
                return RedirectToAction("Index");
            }
            return NotFound();
        }

        private string FormatarPlaca(string placa)
        {
            return placa.Length == 7 ? placa.Substring(0, 3) + "-" + placa.Substring(3) : placa;
        }


        private string RemoverFormatacaoPlaca(string placa)
        {
            return placa.Replace("-", "").ToUpper();
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

                // Título centralizado
                document.Add(new Paragraph("Lista de Carros")
                    .SetFont(boldFont)
                    .SetFontSize(20)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetMarginBottom(20));

                // Tabela com 7 colunas
                Table table = new Table(UnitValue.CreatePercentArray(new float[] { 1, 2.5f, 2.5f, 2f, 1.5f, 2.5f, 2.5f }))
                    .UseAllAvailableWidth();

                // Cabeçalhos
                table.AddHeaderCell(CreateHeaderCell("ID", boldFont));
                table.AddHeaderCell(CreateHeaderCell("Marca", boldFont));
                table.AddHeaderCell(CreateHeaderCell("Modelo", boldFont));
                table.AddHeaderCell(CreateHeaderCell("Cor", boldFont));
                table.AddHeaderCell(CreateHeaderCell("Ano", boldFont));
                table.AddHeaderCell(CreateHeaderCell("Placa", boldFont));
                table.AddHeaderCell(CreateHeaderCell("Preço", boldFont));

                // Dados
                foreach (var carro in _carros)
                {
                    table.AddCell(CreateCell(carro.Id.ToString(), normalFont));
                    table.AddCell(CreateCell(carro.Marca, normalFont));
                    table.AddCell(CreateCell(carro.Modelo, normalFont));
                    table.AddCell(CreateCell(carro.Cor, normalFont));
                    table.AddCell(CreateCell(carro.Ano.Year.ToString(), normalFont));
                    table.AddCell(CreateCell(carro.Placa, normalFont));
                    table.AddCell(CreateCell($"R$ {carro.Preco:N2}", normalFont));
                }

                document.Add(table);
                document.Close();

                return File(memoryStream.ToArray(), "application/pdf", "ListaDeCarros.pdf");
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
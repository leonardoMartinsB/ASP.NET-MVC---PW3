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

using ClosedXML.Excel; // exel

namespace WebApplication1.Controllers
{
    public class EventoController : Controller
    {
        private static List<Evento> _eventos = new List<Evento>
        {
            new Evento { Id = 1, Nome = "Lollapalooza", Data = new DateTime(2025, 4, 15), Local = "São Paulo" },
            new Evento { Id = 2, Nome = "Rock in Rio", Data = new DateTime(2026, 9, 10), Local = "Rio de Janeiro" },
            new Evento { Id = 3, Nome = "Comic Con", Data = new DateTime(2025, 12, 5), Local = "São Paulo" },
            new Evento { Id = 4, Nome = "Festival de Cinema de Cannes", Data = new DateTime(2025, 5, 15), Local = "Cannes" },
            new Evento { Id = 5, Nome = "Festival de Parintins", Data = new DateTime(2025, 6, 20), Local = "Parintins" },
            new Evento { Id = 6, Nome = "Festa do Peão de Barretos", Data = new DateTime(2025, 8, 2), Local = "Barretos" },
            new Evento { Id = 7, Nome = "Festa de São João", Data = new DateTime(2025, 6, 24), Local = "Campina Grande" },
            new Evento { Id = 8, Nome = "F1 Grand Prix", Data = new DateTime(2025, 11, 1), Local = "São Paulo" },
            new Evento { Id = 9, Nome = "Rock n' Jazz Festival", Data = new DateTime(2025, 10, 8), Local = "Curitiba" },
            new Evento { Id = 10, Nome = "São Paulo Fashion Week", Data = new DateTime(2025, 10, 30), Local = "São Paulo" }
        };


        public IActionResult Index()
        {
            return View(_eventos);
        }

        public IActionResult Cadastrar()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Cadastrar(Evento evento)
        {
            evento.Id = _eventos.Any() ? _eventos.Max(e => e.Id) + 1 : 1;
            _eventos.Add(evento);
            return RedirectToAction("Index");
        }

        public IActionResult Editar(int id)
        {
            var evento = _eventos.FirstOrDefault(e => e.Id == id);
            if (evento == null) return NotFound();
            return View(evento);
        }

        [HttpPost]
        public IActionResult Editar(Evento evento)
        {
            var eventoExistente = _eventos.FirstOrDefault(e => e.Id == evento.Id);
            if (eventoExistente == null) return NotFound();

            eventoExistente.Nome = evento.Nome;
            eventoExistente.Data = evento.Data;
            eventoExistente.Local = evento.Local;

            return RedirectToAction("Index");
        }

        public IActionResult Visualizar(int id)
        {
            var evento = _eventos.FirstOrDefault(e => e.Id == id);
            if (evento == null) return NotFound();
            return View(evento);
        }

        public IActionResult Excluir(int id)
        {
            var evento = _eventos.FirstOrDefault(e => e.Id == id);
            if (evento == null) return NotFound();
            return View(evento);
        }

        [HttpPost]
        public IActionResult ExcluirConfirmado(int id)
        {
            var eventoParaRemover = _eventos.FirstOrDefault(e => e.Id == id);
            if (eventoParaRemover != null)
            {
                _eventos.Remove(eventoParaRemover);
            }
            return RedirectToAction("Index");
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
                document.Add(new Paragraph("Lista de Eventos")
                    .SetFont(boldFont)
                    .SetFontSize(20)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetMarginBottom(20));

                // Tabela com 4 colunas
                Table table = new Table(UnitValue.CreatePercentArray(new float[] { 1, 4, 4, 3 })).UseAllAvailableWidth();

                // Cabeçalho
                table.AddHeaderCell(CreateHeaderCell("ID", boldFont));
                table.AddHeaderCell(CreateHeaderCell("Nome", boldFont));
                table.AddHeaderCell(CreateHeaderCell("Local", boldFont));
                table.AddHeaderCell(CreateHeaderCell("Data", boldFont));

                // Dados
                foreach (var evento in _eventos)
                {
                    table.AddCell(CreateCell(evento.Id.ToString(), normalFont));
                    table.AddCell(CreateCell(evento.Nome, normalFont));
                    table.AddCell(CreateCell(evento.Local, normalFont));
                    table.AddCell(CreateCell(evento.Data.ToString("dd/MM/yyyy"), normalFont));
                }

                document.Add(table);
                document.Close();

                return File(memoryStream.ToArray(), "application/pdf", "ListaDeEventos.pdf");
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
                var worksheet = workbook.Worksheets.Add("Eventos");

                // Cabeçalhos
                worksheet.Cell(1, 1).Value = "ID";
                worksheet.Cell(1, 2).Value = "Nome";
                worksheet.Cell(1, 3).Value = "Local";
                worksheet.Cell(1, 4).Value = "Data";

                // Estilo do cabeçalho
                var headerRange = worksheet.Range(1, 1, 1, 4);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                // Dados
                int linha = 2;
                foreach (var evento in _eventos) // Alterado de _celulares para _eventos
                {
                    worksheet.Cell(linha, 1).Value = evento.Id;
                    worksheet.Cell(linha, 2).Value = evento.Nome;
                    worksheet.Cell(linha, 3).Value = evento.Local;
                    worksheet.Cell(linha, 4).Value = evento.Data.ToString("dd/MM/yyyy"); // Data do Evento

                    linha++;
                }

                // Ajustar tamanhos das colunas automaticamente
                worksheet.Columns().AdjustToContents();

                // Centralizar as células de ID e Data
                worksheet.Column(1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Column(4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Bordas para toda a tabela
                var dataRange = worksheet.Range(1, 1, linha - 1, 4);
                dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ListaDeEventos.xlsx"); // Nome alterado para "ListaDeEventos.xlsx"
                }
            }
        }
    }
}
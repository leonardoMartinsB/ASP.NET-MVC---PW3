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

using ClosedXML.Excel; // <- adicione esse using no topo

namespace WebApplication1.Controllers
{
    public class AlunoController : Controller
    {
        private static List<Aluno> _alunos = new List<Aluno>
        {
            new Aluno { Id = 1, Nome = "João Silva", RA = 123456, DataNascimento = new DateTime(2000, 5, 12) },
            new Aluno { Id = 2, Nome = "Maria Souza", RA = 654321, DataNascimento = new DateTime(2001, 9, 30) },
            new Aluno { Id = 3, Nome = "Carlos Mendes", RA = 789012, DataNascimento = new DateTime(1999, 8, 22) },
            new Aluno { Id = 4, Nome = "Ana Paula", RA = 210987, DataNascimento = new DateTime(2002, 1, 15) },
            new Aluno { Id = 5, Nome = "Felipe Rocha", RA = 345678, DataNascimento = new DateTime(2000, 3, 10) },
            new Aluno { Id = 6, Nome = "Juliana Costa", RA = 456789, DataNascimento = new DateTime(2001, 7, 5) },
            new Aluno { Id = 7, Nome = "Ricardo Lima", RA = 567890, DataNascimento = new DateTime(1998, 11, 25) },
            new Aluno { Id = 8, Nome = "Patrícia Gomes", RA = 678901, DataNascimento = new DateTime(1999, 4, 18) },
            new Aluno { Id = 9, Nome = "Bruno Fernandes", RA = 789123, DataNascimento = new DateTime(2002, 6, 8) },
            new Aluno { Id = 10, Nome = "Camila Ferreira", RA = 890123, DataNascimento = new DateTime(2000, 12, 2) }
        };


        public IActionResult Index() => View(_alunos);

        public IActionResult Visualizar(int id)
        {
            var aluno = _alunos.FirstOrDefault(a => a.Id == id);
            return aluno == null ? NotFound() : View(aluno);
        }

        public IActionResult Cadastrar() => View();

        [HttpPost]
        public IActionResult Cadastrar(Aluno aluno)
        {
            if (!ModelState.IsValid)
                return View(aluno);

            aluno.Id = _alunos.Any() ? _alunos.Max(a => a.Id) + 1 : 1;
            _alunos.Add(aluno);

            return RedirectToAction("Index");
        }

        public IActionResult Editar(int id)
        {
            var aluno = _alunos.FirstOrDefault(a => a.Id == id);
            return aluno == null ? NotFound() : View(aluno);
        }

        [HttpPost]
        public IActionResult Editar(Aluno aluno)
        {
            if (!ModelState.IsValid)
                return View(aluno);

            var alunoExistente = _alunos.FirstOrDefault(a => a.Id == aluno.Id);
            if (alunoExistente != null)
            {
                alunoExistente.Nome = aluno.Nome;
                alunoExistente.RA = aluno.RA;
                alunoExistente.DataNascimento = aluno.DataNascimento;
                return RedirectToAction("Index");
            }

            return NotFound();
        }

        public IActionResult Excluir(int id)
        {
            var aluno = _alunos.FirstOrDefault(a => a.Id == id);
            return aluno == null ? NotFound() : View(aluno);
        }

        [HttpPost]
        public IActionResult ExcluirConfirmado(int id)
        {
            var aluno = _alunos.FirstOrDefault(a => a.Id == id);
            if (aluno != null)
            {
                _alunos.Remove(aluno);
                return RedirectToAction("Index");
            }
            return NotFound();
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
                document.Add(new Paragraph("Lista de Alunos")
                    .SetFont(boldFont)
                    .SetFontSize(20)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetMarginBottom(20));

                // Tabela
                Table table = new Table(UnitValue.CreatePercentArray(new float[] { 1, 4, 3, 3 })).UseAllAvailableWidth();

                // Cabeçalho
                table.AddHeaderCell(CreateHeaderCell("ID", boldFont));
                table.AddHeaderCell(CreateHeaderCell("Nome", boldFont));
                table.AddHeaderCell(CreateHeaderCell("RA", boldFont));
                table.AddHeaderCell(CreateHeaderCell("Nascimento", boldFont));

                // Dados
                foreach (var aluno in _alunos)
                {
                    table.AddCell(CreateCell(aluno.Id.ToString(), normalFont));
                    table.AddCell(CreateCell(aluno.Nome, normalFont));
                    table.AddCell(CreateCell(aluno.RA.ToString(), normalFont));
                    table.AddCell(CreateCell(aluno.DataNascimento.ToString("dd/MM/yyyy"), normalFont));
                }

                document.Add(table);
                document.Close();

                return File(memoryStream.ToArray(), "application/pdf", "ListaDeAlunos.pdf");
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
                var worksheet = workbook.Worksheets.Add("Alunos");

                // Cabeçalhos
                worksheet.Cell(1, 1).Value = "ID";
                worksheet.Cell(1, 2).Value = "Nome";
                worksheet.Cell(1, 3).Value = "RA";
                worksheet.Cell(1, 4).Value = "Data de Nascimento";

                // Estilo do cabeçalho
                var headerRange = worksheet.Range(1, 1, 1, 4);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                // Dados
                int linha = 2;
                foreach (var aluno in _alunos)
                {
                    worksheet.Cell(linha, 1).Value = aluno.Id;
                    worksheet.Cell(linha, 2).Value = aluno.Nome;
                    worksheet.Cell(linha, 3).Value = aluno.RA;
                    worksheet.Cell(linha, 4).Value = aluno.DataNascimento.ToString("dd/MM/yyyy");

                    linha++;
                }

                // Ajustar tamanhos das colunas automaticamente
                worksheet.Columns().AdjustToContents();

                // Centralizar todas as células de ID e RA
                worksheet.Column(1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Column(3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Column(4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Bordas para toda a tabela
                var dataRange = worksheet.Range(1, 1, linha - 1, 4);
                dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ListaDeAlunos.xlsx");
                }
            }
        }
    }
}
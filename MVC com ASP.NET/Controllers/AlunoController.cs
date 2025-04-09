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
    public class AlunoController : Controller
    {
        private static List<Aluno> _alunos = new List<Aluno>
        {
            new Aluno { Id = 1, Nome = "João Silva", RA = 123456, DataNascimento = new DateTime(2000, 5, 12) },
            new Aluno { Id = 2, Nome = "Maria Souza", RA = 654321, DataNascimento = new DateTime(2001, 9, 30)}
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
    }
}
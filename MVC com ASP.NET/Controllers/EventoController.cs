using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WebApplication1.Controllers
{
    public class EventoController : Controller
    {
        private static List<Evento> _eventos = new List<Evento>
        {
            new Evento { Id = 1, Nome = "Lollapalooza", Data = new DateTime(2025, 4, 15), Local = "São Paulo" },
            new Evento { Id = 2, Nome = "Rock in Rio", Data = new DateTime(2026, 9, 10), Local = "Rio de Janeiro" },
            new Evento { Id = 3, Nome = "Comic Con", Data = new DateTime(2025, 12, 5), Local = "São Paulo" }
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
    }
}
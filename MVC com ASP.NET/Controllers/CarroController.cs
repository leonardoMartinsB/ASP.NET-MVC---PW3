using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace WebApplication1.Controllers
{
    public class CarroController : Controller
    {
        private static List<Carro> _carros = new List<Carro>
        {
            new Carro { Id = 1, Marca = "Toyota", Modelo = "Corolla", Ano = 2022, Preco = 120000 },
            new Carro { Id = 2, Marca = "Honda", Modelo = "Civic", Ano = 2021, Preco = 110000 },
            new Carro { Id = 3, Marca = "Ford", Modelo = "Focus", Ano = 2020, Preco = 90000 }
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
            if (ModelState.IsValid)
            {
                carro.Id = _carros.Any() ? _carros.Max(c => c.Id) + 1 : 1;
                _carros.Add(carro);
                return RedirectToAction("Index");
            }
            return View(carro);
        }

        public IActionResult Editar(int id)
        {
            var carro = _carros.FirstOrDefault(c => c.Id == id);
            return carro == null ? NotFound() : View(carro);
        }

        [HttpPost]
        public IActionResult Editar(Carro carro)
        {
            var carroExistente = _carros.FirstOrDefault(c => c.Id == carro.Id);
            if (carroExistente != null && ModelState.IsValid)
            {
                carroExistente.Marca = carro.Marca;
                carroExistente.Modelo = carro.Modelo;
                carroExistente.Ano = carro.Ano;
                carroExistente.Preco = carro.Preco;
                return RedirectToAction("Index");
            }
            return View(carro);
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
    }
}

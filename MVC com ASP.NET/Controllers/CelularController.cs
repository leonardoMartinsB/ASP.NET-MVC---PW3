using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Globalization;

namespace WebApplication1.Controllers
{
    public class CelularController : Controller
    {
        private static List<Celular> _celulares = new List<Celular>
        {
            new Celular { Id = 1, Marca = "Apple", Modelo = "iPhone 14", Preco = 7000 },
            new Celular { Id = 2, Marca = "Samsung", Modelo = "Galaxy S23", Preco = 6500 },
            new Celular { Id = 3, Marca = "Xiaomi", Modelo = "Mi 12", Preco = 4000 }
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
            var celularExistente = _celulares.FirstOrDefault(c => c.Id == celular.Id);
            if (celularExistente != null && ModelState.IsValid)
            {
                celularExistente.Marca = celular.Marca;
                celularExistente.Modelo = celular.Modelo;
                celularExistente.Preco = RemoverFormatacaoPreco(celular.Preco.ToString());
                return RedirectToAction("Index");
            }
            return View(celular);
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

    }
}
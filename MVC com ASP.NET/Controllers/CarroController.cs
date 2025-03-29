using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WebApplication1.Controllers
{
    public class CarroController : Controller
    {
        private static List<Carro> _carros = new List<Carro>
        {
            new Carro { Id = 1, Marca = "Tesla", Modelo = "Model S", Cor = "Branco", Placa = "ABC1234", Ano = 2023, Preco = 2500000  },
            new Carro { Id = 2, Marca = "BMW", Modelo = "X5", Cor = "Preto", Placa = "XYZ5678", Ano = 2022, Preco = 580000 },
            new Carro { Id = 3, Marca = "Audi", Modelo = "A4", Cor = "Prata", Placa = "LMN9101", Ano = 2021, Preco = 180000 }
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
    }
}
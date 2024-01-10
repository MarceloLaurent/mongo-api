using Api.Data.Collections;
using Api.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class InfectadoController : ControllerBase
    {
        Data.MongoDB _mongoDB;
        IMongoCollection<Infectado> _infectadosCollection;

        public InfectadoController(Data.MongoDB mongoDB)
        {
            _mongoDB = mongoDB;
            _infectadosCollection = _mongoDB.DB.GetCollection<Infectado>(typeof(Infectado).Name.ToLower());
        }

        private bool ContemInfectado(string cpf)
        {
            var infectado = Builders<Infectado>.Filter.Eq("cPF", cpf);
            var result = _infectadosCollection.Find(infectado).FirstOrDefault();

            return result != null;
        }

        [HttpPost]
        public ActionResult SalvarInfectado([FromBody] InfectadoDto dto)
        {
            var infectado = new Infectado(dto.CPF, dto.DataNascimento, dto.Sexo, dto.Latitude, dto.Longitude);

            if (!ContemInfectado(dto.CPF))
            {
                _infectadosCollection.InsertOne(infectado);

                return StatusCode(201, "Infectado adicionado com sucesso");
            }
            else
            {
                return StatusCode(202, "Infectado já existente");
            }

        }

        [HttpGet]
        public ActionResult ObterInfectados()
        {
            var infectados = _infectadosCollection.Find(Builders<Infectado>.Filter.Empty).ToList();

            return Ok(infectados);
        }

        [HttpGet("{cpf}")]
        public ActionResult ObterInfectadoPeloCPF(string cpf)
        {
            var infectado = _infectadosCollection.Find(Builders<Infectado>.Filter.Eq("cPF", cpf)).FirstOrDefault();

            return Ok(infectado);
        }

        [HttpPut]
        public ActionResult AtualizarInfectado([FromBody] InfectadoDto dto)
        {
            var infectado = new Infectado(dto.CPF, dto.DataNascimento, dto.Sexo, dto.Latitude, dto.Longitude);

            if (ContemInfectado(dto.CPF))
            {
                _infectadosCollection.UpdateMany(Builders<Infectado>.Filter.Where(_ => _.CPF == dto.CPF),
                Builders<Infectado>.Update.Set("dataNascimento", dto.DataNascimento)
                                        .Set("sexo", dto.Sexo)
                                        .Set("latitude", dto.Latitude)
                                        .Set("longitude", dto.Longitude));

                return StatusCode(201, "Infectado atualizado com sucesso");
            }
            else
            {
                return StatusCode(204, "Infectado não encontrado");
            }
        }

        [HttpDelete("{cpf}")]
        public ActionResult DeletarInfectado(string cpf)
        {
            if (ContemInfectado(cpf))
            {
                _infectadosCollection.DeleteOne(Builders<Infectado>.Filter.Where(_ => _.CPF == cpf));

                return Ok("Infectado deletado com sucesso");
            }
            else
            {
                return StatusCode(204, "Infectado não encontrado");
            }
        }
    }
}

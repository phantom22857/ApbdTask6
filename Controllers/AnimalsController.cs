
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Tutorial5.Models;
using Tutorial5.Models.DTOs;

namespace Tutorial5.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnimalsController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public AnimalsController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult GetAnimals(string orderBy = "name")
        {
            string orderByColumn = orderBy.ToLower() switch
            {
                "description" => "Description",
                "category" => "Category",
                "area" => "Area",
                _ => "Name"
            };

            using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
            connection.Open();

            string sqlQuery = $"SELECT * FROM Animal ORDER BY {orderByColumn};";
            using SqlCommand command = new SqlCommand(sqlQuery, connection);

            var reader = command.ExecuteReader();

            var animals = new List<Animal>();

            int idAnimalOrdinal = reader.GetOrdinal("IdAnimal");
            int nameOrdinal = reader.GetOrdinal("Name");

            while (reader.Read())
            {
                animals.Add(new Animal()
                {
                    IdAnimal = reader.GetInt32(idAnimalOrdinal),
                    Name = reader.GetString(nameOrdinal)
                });
            }

            return Ok(animals);
        }

        [HttpPost]
        public IActionResult AddAnimal(AddAnimal animal)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
            connection.Open();

            string sqlQuery = "INSERT INTO Animal (Name, Description, Category, Area) VALUES (@Name, @Description, @Category, @Area);";
            using SqlCommand command = new SqlCommand(sqlQuery, connection);
            command.Parameters.AddWithValue("@Name", animal.Name);
            command.Parameters.AddWithValue("@Description", (object)animal.Description ?? DBNull.Value);
            command.Parameters.AddWithValue("@Category", DBNull.Value);
            command.Parameters.AddWithValue("@Area", DBNull.Value);

            int rowsAffected = command.ExecuteNonQuery();

            if (rowsAffected > 0)
            {
                return Created("", null);
            }
            else
            {
                return StatusCode(500, "Failed to add animal");
            }
        }

        [HttpPut("{idAnimal}")]
        public IActionResult UpdateAnimal(int idAnimal, UpdateAnimal animal)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
            connection.Open();

            string sqlQuery = "UPDATE Animal SET Name = @Name, Description = @Description, Category = @Category, Area = @Area WHERE IdAnimal = @IdAnimal;";
            using SqlCommand command = new SqlCommand(sqlQuery, connection);
            command.Parameters.AddWithValue("@Name", animal.Name);
            command.Parameters.AddWithValue("@Description", (object)animal.Description ?? DBNull.Value);
            command.Parameters.AddWithValue("@Category", (object)animal.Category ?? DBNull.Value);
            command.Parameters.AddWithValue("@Area", (object)animal.Area ?? DBNull.Value);
            command.Parameters.AddWithValue("@IdAnimal", idAnimal);

            int rowsAffected = command.ExecuteNonQuery();

            if (rowsAffected > 0)
            {
                return NoContent();
            }
            else
            {
                return NotFound();
            }
        }

        [HttpDelete("{idAnimal}")]
        public IActionResult DeleteAnimal(int idAnimal)
        {
            using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
            connection.Open();

            string sqlQuery = "DELETE FROM Animal WHERE IdAnimal = @IdAnimal;";
            using SqlCommand command = new SqlCommand(sqlQuery, connection);
            command.Parameters.AddWithValue("@IdAnimal", idAnimal);

            int rowsAffected = command.ExecuteNonQuery();

            if (rowsAffected > 0)
            {
                return NoContent();
            }
            else
            {
                return NotFound();
            }
        }
    }
}

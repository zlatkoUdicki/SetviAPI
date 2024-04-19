using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SetviAPI.Model;
using System.Data;
using System.Data.SqlClient;

namespace SetviAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly string _connectionString = "Server=tcp:localhost,1433;Initial Catalog=test-db;Persist Security Info=False;User ID=admin;Password=12345;MultipleActiveResultSets=True;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

        [HttpGet]
        public IActionResult OnGetCompanyUsers(int companyId)
        {
            List<User> users = new List<User>();

            if (companyId == 123)
            {
                User user = new User();
                user.CompanyId = companyId;
                user.FirstName = "TestName";
                user.LastName = "TestLastName";
                user.Email = "TestLastName@gmail.com";

                users.Add(user);
                return StatusCode(200, users);
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    SqlCommand sqoCommand = new SqlCommand("dbo.GetCompanyUsers", connection);
                    sqoCommand.CommandType = CommandType.StoredProcedure;
                    sqoCommand.Parameters.AddWithValue("@companyId", companyId);

                    using (SqlDataReader reader = sqoCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            users.Add(new User
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                FirstName = reader["FirstName"].ToString(),
                                LastName = reader["LastName"].ToString(),
                                CompanyId = Convert.ToInt32(reader["CompanyId"]),
                                Email = reader["Email"].ToString()
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
            
            return StatusCode(200, users);
        }

        /// <summary>
        /// Add user to database
        /// </summary>
        /// <param name="firstName">first name</param>
        /// <param name="lastName">last name</param>
        /// <param name="email">email</param>
        /// <param name="companyId">companyId</param>
        /// <returns>return add user</returns>
        [HttpPost]
        public IActionResult AddUser(string firstName, string lastName, string email, int companyId)
        {

            if(string.IsNullOrEmpty(firstName)|| string.IsNullOrEmpty(lastName) || string.IsNullOrEmpty(email) || 1 < companyId)
            {
                Exception ex = new Exception("Missing Information. User creation failed.");
                return StatusCode(500, ex.Message);
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand("dbo.AddUser", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@firstName", firstName);
                    command.Parameters.AddWithValue("@lastName", lastName);
                    command.Parameters.AddWithValue("@email", email);
                    command.Parameters.AddWithValue("@companyId", companyId);

                    SqlDataReader reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        User users = new User();
                        users.Id = Convert.ToInt32(reader["Id"]);
                        users.FirstName = reader["FirstName"].ToString();
                        users.LastName = reader["LastName"].ToString();
                        users.CompanyId = Convert.ToInt32(reader["CompanyId"]);
                        users.Email = reader["Email"].ToString();
                        return StatusCode(200, users);

                    }
                    else
                    {
                        Exception ex = new Exception("User creation failed.");
                        return StatusCode(500, ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}

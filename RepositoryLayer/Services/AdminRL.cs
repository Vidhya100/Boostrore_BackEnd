﻿
using CommonLayer.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using RepositoryLayer.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RepositoryLayer.Services
{
    public class AdminRL : IAdminRL
    {
        private readonly IConfiguration iConfiguration;
        public static string Key = "vidhya@@kfxcbv@";

        public AdminRL(IConfiguration iconfiguration)
        {
            this.iConfiguration = iconfiguration;
        }

        public string AdminLogin(AdminLoginModel admin)
        {
            
            using SqlConnection con = new SqlConnection(iConfiguration["ConnectionStrings:BookStoreDB"]);
            try
            {
                SqlCommand command = new SqlCommand("spAdminLogin", con);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@EmailId", admin.EmailId);
                command.Parameters.AddWithValue("@Password", admin.Password);

                con.Open();
                var result = command.ExecuteScalar();
                if (result != null)
                {
                    string query = "SELECT AdminId FROM Admin WHERE EmailId = '" + admin.EmailId + "'";
                    SqlCommand cmd = new SqlCommand(query, con);
                    var Id = cmd.ExecuteScalar();
                    var token = GenerateSecurityToken(admin.EmailId, Id.ToString());
                    return token;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return default;
        }

        //JWT token
        public string GenerateSecurityToken(string email, string UserId)
        {

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(this.iConfiguration[("JWT:Key")]);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                         new Claim(ClaimTypes.Role, "Admin"),
                         new Claim(ClaimTypes.Email, email),
                         new Claim("UserId", UserId.ToString())
                }),
                    Expires = DateTime.UtcNow.AddMinutes(30),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);

                return tokenHandler.WriteToken(token);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}

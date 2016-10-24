using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using InstagramKiller.Model;
using System.ComponentModel.DataAnnotations;

namespace InstagramKiller.WebApi.Controllers
{
    public class UsersController : ApiController
    {
        private const string ConnectionString = @"Data Source = DESKTOP-2VEAELC; Initial Catalog = InstagramKiller; Integrated Security = true";
        private readonly IDataLayer _dataLayer;

        public UsersController()
        {
            _dataLayer = new DataLayer.Sql.DataLayer(ConnectionString);
        }

        /// <summary>
        /// Add new user
        /// </summary>
        /// <param name="user">user</param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/users/{id}")]
        public User CreateUser(User user)
        {
            return _dataLayer.AddUser(user);
        }

        /// <summary>
        /// Get user by id
        /// </summary>
        /// <param name="id">user id</param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/users/{id}")]
        public User GetUser(Guid id)
        {
            return _dataLayer.GetUser(id);
        }

        /// <summary>
        /// Delete user by id
        /// </summary>
        /// <param name="userId">user id</param>
        [HttpDelete]
        [Route("api/users/{id}")]
        public void DeleteUser(Guid userId)
        {
            _dataLayer.DeleteUser(userId);
        }
    }
}

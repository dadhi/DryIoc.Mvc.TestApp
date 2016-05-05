using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MvcMovie.Controllers
{
    public class DefaultController : Controller
    {
        private ICustomerRepository repository;
 

        public DefaultController(ICustomerRepository repository)
        {
            this.repository = repository;
 
        }

        public ActionResult Index()
        {
            var user = this.repository.GetCustomers();
 
            return this.View();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ShopCar.Models;

namespace ShopCar.Controllers
{
    public class ProductsController : Controller
    {
        private readonly testContext _db;

        public ProductsController(testContext context)
        {
            _db = context;
        }

        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Index(Shopping _User)
        {
            if (_User == null)
            {
                return Content("失敗");
            }
            else
            {
                _db.Shoppings.Add(_User);
                _db.SaveChanges();
                return Content("成功");
            }
        }

    }
}

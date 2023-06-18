using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping.Models;

namespace Shopping.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsAPIController : ControllerBase
    {
        private readonly AdventureWorksLT2016Context _context;

        public ProductsAPIController(AdventureWorksLT2016Context context)
        {
            _context = context;
        }

        // GET: api/ProductsAPI
        [HttpGet]
        public FileContentResult GetImage(int id)
        {
            IQueryable<Product> requestedPhoto = from p in _context.Products
                                                 where p.ProductId == id select p;

            var result = requestedPhoto.FirstOrDefault();
            if (result == null)
            {
                return null;
            }
            else
            {
                return File(result.ThumbNailPhoto, "image/jepg");
            }
        }
    }
}

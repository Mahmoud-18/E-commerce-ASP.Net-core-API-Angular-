using API.DTOs;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]      
        public async Task<IActionResult> GetProducts([FromQuery]FilterParamsDTO filterParams)
        {          
            var products = await _productService.GetProductsFilteredAsync(filterParams);

            return Ok(products);
        }

       
        [HttpPost("add")]
        public async Task<IActionResult> AddProduct([FromForm]AddProductDTO productDTO)
        {

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _productService.AddProductAsync(productDTO);

            return Created();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);

            return Ok(product);
        }

    }
}

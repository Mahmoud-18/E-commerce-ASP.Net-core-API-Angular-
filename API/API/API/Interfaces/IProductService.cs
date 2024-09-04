using API.Data.Models;
using API.DTOs;
using API.Helper;

namespace API.Interfaces;

public interface IProductService
{
    Task<Product> GetProductByIdAsync(int id);  
    Task<PaginationResult<ProductDTO>> GetProductsFilteredAsync(FilterParamsDTO filter);
    Task AddProductAsync(AddProductDTO addproductDTO);
}

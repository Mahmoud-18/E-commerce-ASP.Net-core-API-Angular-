using API.Data.Context;
using API.Data.Models;
using API.DTOs;
using API.Helper;
using API.Interfaces;
using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;

namespace API.Services;

public class ProductService : IProductService
{
    private readonly ECommerceDBContext _dBContext;
    private readonly IWebHostEnvironment _webHostEnvironment;    

    public ProductService(ECommerceDBContext dBContext, IWebHostEnvironment webHostEnvironment)
    {
        _dBContext = dBContext;
        _webHostEnvironment = webHostEnvironment;       
    }

    public async Task AddProductAsync(AddProductDTO addProductDTO)
    {
        Product product = new Product
        {
            Category = addProductDTO.Category,
            DiscountRate = addProductDTO.DiscountRate,
            MinimumQuantity = addProductDTO.MinimumQuantity,
            Name = addProductDTO.Name,
            Price = addProductDTO.Price,
            ProductCode = addProductDTO.ProductCode,
        };

        string uniqueFileName = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff") + "_" + addProductDTO.ProductCode + "_" + addProductDTO.ImageSource.FileName;

        var folderPath = "Images";
        string newPath = Path.Combine(_webHostEnvironment.WebRootPath, folderPath);
        if (!Directory.Exists(newPath))
        {
            Directory.CreateDirectory(newPath);
        }

        string filePath = Path.Combine(newPath, uniqueFileName); // Path to store the file
       
        using (FileStream stream = new FileStream(filePath, FileMode.Create))
        {
            await addProductDTO.ImageSource.CopyToAsync(stream); // Save the file to the server
        }

        product.ImagePath =$"/{folderPath}/{uniqueFileName}";

        await _dBContext.Products.AddAsync(product);

        await _dBContext.SaveChangesAsync();
    }  
    public async Task<PaginationResult<ProductDTO>> GetProductsFilteredAsync(FilterParamsDTO filter)
    {
        IQueryable<Product> query = _dBContext.Products;

        if (!string.IsNullOrWhiteSpace(filter.TextSearch))
        {
            query = query.Where(p => p.Name.ToLower().Trim().Contains(filter.TextSearch.ToLower().Trim())
                                   || p.Category.ToLower().Trim().Contains(filter.TextSearch.ToLower().Trim()));
        }
      

        int productsCount = query.Count();
        int RecordsSkip = (filter.PageNumber - 1) * filter.PageSize;

        if (!string.IsNullOrEmpty(filter.Sort))
        {
            query = filter.Sort switch
            {
                "name" => query.OrderBy(p => p.Name),
                "priceAsc" => query.OrderBy(p => p.Price),
                "priceDesc" => query.OrderByDescending(p => p.Price),
                _ => query.OrderBy(p => p.Name),
            };
        }

        List<ProductDTO> products = await query          
            .Skip(RecordsSkip)
            .Take(filter.PageSize)
            .Select(p => new ProductDTO
            {
                Id = p.Id,
                Category = p.Category,
                DiscountRate = p.DiscountRate,
                MinimumQuantity = p.MinimumQuantity,
                Name = p.Name,
                Price = p.Price,
                ProductCode = p.ProductCode,
                ImagePath = p.ImagePath,
            })
            .ToListAsync();

        var pager = new Pager(productsCount, filter.PageNumber, filter.PageSize);
     
        return new PaginationResult<ProductDTO>
        {
            Data = products,
            Pager = pager
        };     
    }
    public async Task<Product> GetProductByIdAsync(int id)
    {
        return await _dBContext.Products.FindAsync(id);
    }
}

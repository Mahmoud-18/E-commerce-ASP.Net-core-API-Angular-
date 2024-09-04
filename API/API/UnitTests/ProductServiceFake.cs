using API.Data.Models;
using API.DTOs;
using API.Helper;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests
{
    public class ProductServiceFake : IProductService
    {
        private readonly List<Product> _products;
        public ProductServiceFake()
        {
            _products = new List<Product>()
            {
                new(){ Id = 1, Name="Shirt-1",Category = "Clothes", DiscountRate=0.1M ,MinimumQuantity=20, Price =499 , ProductCode = "SA32" , ImagePath="/Images/t-shirt1.png" },
                new(){ Id = 2, Name="Shirt-2",Category = "Clothes", DiscountRate=0.1M ,MinimumQuantity=20, Price =499 , ProductCode = "SA33" , ImagePath="/Images/t-shirt2.png" },
                new(){ Id = 3, Name="Shirt-3",Category = "Clothes", DiscountRate=0.1M ,MinimumQuantity=30, Price =499 , ProductCode = "SA34" , ImagePath="/Images/t-shirt3.png" },
                new(){ Id = 4, Name="Shirt-4",Category = "Clothes", DiscountRate=0.3M ,MinimumQuantity=20, Price =499 , ProductCode = "SA35" , ImagePath="/Images/t-shirt4.png" },
                new(){ Id = 5, Name="Shirt-5",Category = "Clothes", DiscountRate=0.1M ,MinimumQuantity=10, Price =499 , ProductCode = "SA36" , ImagePath="/Images/t-shirt5.png" },
                new(){ Id = 6, Name="Shirt-6",Category = "Shirts", DiscountRate=0.2M ,MinimumQuantity=40, Price =499 , ProductCode = "SA39" , ImagePath="/Images/t-shirt6.png" },

            };
        }
        public Task AddProductAsync(AddProductDTO addproductDTO)
        {
            Product product = new Product()
            {
                Category = addproductDTO.Category,
                DiscountRate = addproductDTO.DiscountRate,
                MinimumQuantity = addproductDTO.MinimumQuantity,
                Price = addproductDTO.Price,
                Name = addproductDTO.Name,
                ProductCode = addproductDTO.ProductCode,
                Id = _products.Count + 1,
            };

            string uniqueFileName = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff") + "_" + addproductDTO.ProductCode + "_" + addproductDTO.ImageSource.FileName;

            product.ImagePath = $"/Images/{uniqueFileName}";

            _products.Add(product);

            return Task.CompletedTask;
        }

        public async Task<Product> GetProductByIdAsync(int id)
        {
           var product =  _products.Find(x => x.Id == id);
          
            return await Task.FromResult(product);
        }

        public async Task<PaginationResult<ProductDTO>> GetProductsFilteredAsync(FilterParamsDTO filter)
        {
            IEnumerable<Product> query = _products;

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

            List<ProductDTO> products =  query
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
                .ToList();

            var pager = new Pager(productsCount, filter.PageNumber, filter.PageSize);

            return await Task.FromResult( new PaginationResult<ProductDTO>
            {
                Data = products,
                Pager = pager
            });
        }
    }
}

using API.Data.Context;
using API.Data.Models;
using API.DTOs;
using API.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace IntegrationTests
{
    public class ProductServiceIntegrationTests : IDisposable
    {
        private readonly ECommerceDBContext _dbContext;
        private readonly ProductService _productService;
        public ProductServiceIntegrationTests()
        {
            // Set up the test database context
            var options = new DbContextOptionsBuilder<ECommerceDBContext>()
                .UseInMemoryDatabase(databaseName: "TestDB")
                .Options;
            _dbContext = new ECommerceDBContext(options);

            // Seed test data into the in-memory database
            SeedTestData();

            // Set up a mock web host environment
            var mockWebHostEnvironment = new Mock<IWebHostEnvironment>();
            mockWebHostEnvironment.Setup(m => m.WebRootPath).Returns(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"));

            // Create an instance of ProductService with the test database context and mock web host environment
            _productService = new ProductService(_dbContext, mockWebHostEnvironment.Object);
        }

        [Fact]
        public async Task AddProductAsync_ProductAddedToDatabase()
        {
            // Arrange
            var addProductDto = new AddProductDTO
            {
                ImageSource = GetMockFormFile("testImage.png", "image/png", new byte[0]),
                Category = "Test",
                ProductCode = "TP1",
                Name = "Test Product",
                Price = 99.99M,
                MinimumQuantity = 10,
                DiscountRate = 0.1M
            };

            // Act
            await _productService.AddProductAsync(addProductDto);

            // Assert
            // Verify that the product is added to the database
            var addedProduct = await _dbContext.Products.FirstOrDefaultAsync(x=>x.ProductCode == "TP1");
            Assert.NotNull(addedProduct);
            Assert.Equal(addProductDto.Name, addedProduct.Name);
            Assert.Equal(addProductDto.Category, addedProduct.Category);
            Assert.Equal(addProductDto.ProductCode, addedProduct.ProductCode);         
            Assert.Equal(addProductDto.Price, addedProduct.Price);
            Assert.Equal(addProductDto.MinimumQuantity, addedProduct.MinimumQuantity);
            Assert.Equal(addProductDto.DiscountRate, addedProduct.DiscountRate);
            
        }

        [Fact]
        public async Task GetProductByIdAsync_ProductExists_ReturnsProduct()
        {
            // Arrange
            var productId = 1; // Assuming product with ID 1 exists in the test database

            // Act
            var product = await _productService.GetProductByIdAsync(productId);

            // Assert
            Assert.NotNull(product);
            Assert.Equal(productId, product.Id);
        }

        [Fact]
        public async Task GetProductByIdAsync_ProductDoesNotExist_ReturnsNull()
        {
            // Arrange
            var productId = 10000; // Assuming product with ID 1000 doesn't exist in the test database

            // Act
            var product = await _productService.GetProductByIdAsync(productId);

            // Assert
            Assert.Null(product);
        }

        [Fact]
        public async Task GetProductsFilteredAsync_ReturnsFilteredProducts()
        {
            // Arrange
            var filter = new FilterParamsDTO
            {
                TextSearch = "Shirt", // Assuming there are products with 'Shirt' in name or category
                Sort = "priceAsc",
                PageNumber = 1,
                PageSize = 5
            };

            // Act
            var result = await _productService.GetProductsFilteredAsync(filter);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result.Data);
            Assert.True(result.Data.Count <= filter.PageSize); // Ensure correct page size
            Assert.True(result.Data.All(p => p.Name.Contains("Shirt") || p.Category.Contains("Shirt"))); // Ensure filtered correctly
            Assert.True(result.Data.SequenceEqual(result.Data.OrderBy(p => p.Price))); // Ensure sorted correctly
           
        }

        public void Dispose()
        {
            _dbContext.Dispose();
        }

        public static IFormFile GetMockFormFile(string fileName, string fileContentType, byte[] fileContent)
        {
            // Create a MemoryStream from the byte array
            var stream = new MemoryStream(fileContent);

            // Create a new FormFile with the MemoryStream as the file content
            var formFile = new FormFile(stream, 0, fileContent.Length, null, fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = fileContentType
            };

            return formFile;
        }

        private void SeedTestData()
        {
            // Seed some test products
            _dbContext.Products.AddRange(new List<Product>
            {
                new(){  Name="Shirt-1",Category = "Clothes", DiscountRate=0.1M ,MinimumQuantity=20, Price =499 , ProductCode = "SA32" , ImagePath="/Images/t-shirt1.png" },
                new(){  Name="Shirt-2",Category = "Clothes", DiscountRate=0.1M ,MinimumQuantity=20, Price =499 , ProductCode = "SA33" , ImagePath="/Images/t-shirt2.png" },
                new(){  Name="Shirt-3",Category = "Clothes", DiscountRate=0.1M ,MinimumQuantity=30, Price =499 , ProductCode = "SA34" , ImagePath="/Images/t-shirt3.png" },
                new(){  Name="Shirt-4",Category = "Clothes", DiscountRate=0.3M ,MinimumQuantity=20, Price =499 , ProductCode = "SA35" , ImagePath="/Images/t-shirt4.png" },
                new(){  Name="Shirt-5",Category = "Clothes", DiscountRate=0.1M ,MinimumQuantity=10, Price =499 , ProductCode = "SA36" , ImagePath="/Images/t-shirt5.png" },
                new(){  Name="Shirt-6",Category = "Shirts", DiscountRate=0.2M ,MinimumQuantity=40, Price = 499 , ProductCode = "SA39" , ImagePath="/Images/t-shirt6.png" },
        });

            _dbContext.SaveChanges();
        }
    }
}
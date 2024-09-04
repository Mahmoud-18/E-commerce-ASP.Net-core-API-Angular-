using API.Data.Models;
using API.DTOs;
using Microsoft.AspNetCore.Http;

namespace UnitTests
{
    public class ProductServiceUnitTests
    {
        [Fact]
        public async Task AddProductAsync_ProductAddedSuccessfully()
        {
            // Arrange
            var productService = new ProductServiceFake();

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
            await productService.AddProductAsync(addProductDto);

            // Assert
            var addedProduct = await productService.GetProductByIdAsync(7); // Assuming 6 products exist initially
            Assert.NotNull(addedProduct);
            Assert.Equal(addProductDto.Category, addedProduct.Category);
            Assert.Equal(addProductDto.ProductCode, addedProduct.ProductCode);
            Assert.Equal(addProductDto.Name, addedProduct.Name);
            Assert.Equal(addProductDto.Price, addedProduct.Price);
            Assert.Equal(addProductDto.MinimumQuantity, addedProduct.MinimumQuantity);
            Assert.Equal(addProductDto.DiscountRate, addedProduct.DiscountRate);
        }

        [Fact]
        public async Task GetProductByIdAsync_ProductExists_ReturnsProduct()
        {
            // Arrange
            var productService = new ProductServiceFake();
            var productId = 1; // Assuming product with ID 1 exists

            // Act
            var product = await productService.GetProductByIdAsync(productId);

            // Assert
            Assert.NotNull(product);
            Assert.Equal(productId, product.Id);
        }

        [Fact]
        public async Task GetProductByIdAsync_ProductDoesNotExist_ReturnsNull()
        {
            // Arrange
            var productService = new ProductServiceFake();
            var productId = 10; // Assuming product with ID 10 doesn't exist

            // Act
            var product = await productService.GetProductByIdAsync(productId);

            // Assert
            Assert.Null(product);
        }

        [Fact]
        public async Task GetProductsFilteredAsync_ReturnsFilteredProducts()
        {
            // Arrange
            var productService = new ProductServiceFake();
            var filter = new FilterParamsDTO
            {
                TextSearch = "Shirt", // Assuming there are products with 'Shirt' in name or category
                Sort = "priceAsc",
                PageNumber = 1,
                PageSize = 5
            };

            // Act
            var result = await productService.GetProductsFilteredAsync(filter);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result.Data);
            Assert.True(result.Data.Count <= filter.PageSize); // Ensure correct page size
            Assert.True(result.Data.All(p => p.Name.Contains("Shirt") || p.Category.Contains("Shirt"))); // Ensure filtered correctly
            Assert.True(result.Data.SequenceEqual(result.Data.OrderBy(p => p.Price))); // Ensure sorted correctly
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
    }
}
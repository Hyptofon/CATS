using Api.Dtos;
using Api.Modules.Errors;
using Application.Common.Interfaces.Queries;
using Application.Products.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("products")]
[Microsoft.AspNetCore.Authorization.Authorize(Policy = "MustBeActive")]
public class ProductsController(
    ISender sender,
    IProductQueries productQueries) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProductDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        var products = await productQueries.GetAllAsync(cancellationToken);
        return products.Select(ProductDto.FromDomainModel).ToList();
    }

    [HttpGet("search")]
    public async Task<ActionResult<IReadOnlyList<ProductDto>>> Search(
        [FromQuery] string? search,
        [FromQuery] int? productTypeId,
        CancellationToken cancellationToken)
    {
        var products = await productQueries.SearchAsync(search, productTypeId, cancellationToken);
        return products.Select(ProductDto.FromDomainModel).ToList();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductDto>> GetById(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        var product = await productQueries.GetByIdAsync(id, cancellationToken);

        return product.Match<ActionResult<ProductDto>>(
            p => ProductDto.FromDomainModel(p),
            () => NotFound());
    }

    [HttpPost]
    public async Task<ActionResult<ProductDto>> Create(
        [FromBody] CreateProductDto request,
        CancellationToken cancellationToken)
    {
        var command = new CreateProductCommand
        {
            Name = request.Name,
            Description = request.Description,
            ProductTypeId = request.ProductTypeId,
            ShelfLifeDays = request.ShelfLifeDays,
            ShelfLifeHours = request.ShelfLifeHours
        };

        var result = await sender.Send(command, cancellationToken);

        return result.Match<ActionResult<ProductDto>>(
            p => ProductDto.FromDomainModel(p),
            e => e.ToObjectResult());
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProductDto>> Update(
        [FromRoute] int id,
        [FromBody] UpdateProductDto request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateProductCommand
        {
            Id = id,
            Name = request.Name,
            Description = request.Description,
            ProductTypeId = request.ProductTypeId,
            ShelfLifeDays = request.ShelfLifeDays,
            ShelfLifeHours = request.ShelfLifeHours
        };

        var result = await sender.Send(command, cancellationToken);

        return result.Match<ActionResult<ProductDto>>(
            p => ProductDto.FromDomainModel(p),
            e => e.ToObjectResult());
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ProductDto>> Delete(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteProductCommand { Id = id };
        var result = await sender.Send(command, cancellationToken);

        return result.Match<ActionResult<ProductDto>>(
            p => ProductDto.FromDomainModel(p),
            e => e.ToObjectResult());
    }
}

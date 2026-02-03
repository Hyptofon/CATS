using Api.Dtos;
using Api.Modules.Errors;
using Application.Common.Interfaces.Queries;
using Application.ProductTypes.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("product-types")]
public class ProductTypesController(
    ISender sender, 
    IProductTypeQueries productTypeQueries) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProductTypeDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        var productTypes = await productTypeQueries.GetAllAsync(cancellationToken);
        return productTypes.Select(ProductTypeDto.FromDomainModel).ToList();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductTypeDto>> GetById(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        var productType = await productTypeQueries.GetByIdAsync(
            id, 
            cancellationToken);

        return productType.Match<ActionResult<ProductTypeDto>>(
            pt => ProductTypeDto.FromDomainModel(pt),
            () => NotFound());
    }

    [HttpPost]
    public async Task<ActionResult<ProductTypeDto>> Create(
        [FromBody] CreateProductTypeDto request,
        CancellationToken cancellationToken)
    {
        var command = new CreateProductTypeCommand
        {
            Name = request.Name,
            ShelfLifeDays = request.ShelfLifeDays,
            Meta = request.Meta
        };

        var result = await sender.Send(command, cancellationToken);

        return result.Match<ActionResult<ProductTypeDto>>(
            pt => ProductTypeDto.FromDomainModel(pt),
            e => e.ToObjectResult());
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProductTypeDto>> Update(
        [FromRoute] int id,
        [FromBody] UpdateProductTypeDto request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateProductTypeCommand
        {
            ProductTypeId = id,
            Name = request.Name,
            ShelfLifeDays = request.ShelfLifeDays,
            Meta = request.Meta
        };

        var result = await sender.Send(command, cancellationToken);

        return result.Match<ActionResult<ProductTypeDto>>(
            pt => ProductTypeDto.FromDomainModel(pt),
            e => e.ToObjectResult());
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ProductTypeDto>> Delete(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteProductTypeCommand(id);
        var result = await sender.Send(command, cancellationToken);

        return result.Match<ActionResult<ProductTypeDto>>(
            pt => ProductTypeDto.FromDomainModel(pt),
            e => e.ToObjectResult());
    }
}
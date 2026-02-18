using Api.Dtos;
using Api.Modules.Errors;
using Application.Common.Interfaces.Queries;
using Application.ProductTypes.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

/// <summary>
/// Управління типами продуктів (наприклад: "Молоко", "Соняшникова олія").
/// Тип продукту визначає термін придатності за замовчуванням для всіх продуктів цього типу.
/// </summary>
[ApiController]
[Route("product-types")]
[Microsoft.AspNetCore.Authorization.Authorize(Policy = "MustBeActive")]
public class ProductTypesController(
    ISender sender, 
    IProductTypeQueries productTypeQueries) : ControllerBase
{
    /// <summary>
    /// Отримати список усіх типів продуктів
    /// </summary>
    /// <returns>Масив усіх типів продуктів</returns>
    /// <response code="200">Список типів продуктів успішно повернуто</response>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ProductTypeDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProductTypeDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        var productTypes = await productTypeQueries.GetAllAsync(cancellationToken);
        return productTypes.Select(ProductTypeDto.FromDomainModel).ToList();
    }

    /// <summary>
    /// Отримати тип продукту за його ID
    /// </summary>
    /// <param name="id">Унікальний ідентифікатор типу продукту</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Тип продукту з вказаним ID</returns>
    /// <response code="200">Тип продукту знайдено</response>
    /// <response code="404">Тип продукту з таким ID не існує</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProductTypeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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

    /// <summary>
    /// Створити новий тип продукту
    /// </summary>
    /// <param name="request">Дані нового типу продукту</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Створений тип продукту</returns>
    /// <response code="200">Тип продукту успішно створено</response>
    /// <response code="409">Тип продукту з такою назвою вже існує</response>
    [HttpPost]
    [ProducesResponseType(typeof(ProductTypeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProductTypeDto>> Create(
        [FromBody] CreateProductTypeDto request,
        CancellationToken cancellationToken)
    {
        var command = new CreateProductTypeCommand
        {
            Name = request.Name,
            ShelfLifeDays = request.ShelfLifeDays,
            ShelfLifeHours = request.ShelfLifeHours,
            Meta = request.Meta
        };

        var result = await sender.Send(command, cancellationToken);

        return result.Match<ActionResult<ProductTypeDto>>(
            pt => ProductTypeDto.FromDomainModel(pt),
            e => e.ToObjectResult());
    }

    /// <summary>
    /// Оновити існуючий тип продукту
    /// </summary>
    /// <param name="id">ID типу продукту, який оновлюється</param>
    /// <param name="request">Нові дані типу продукту</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Оновлений тип продукту</returns>
    /// <response code="200">Тип продукту успішно оновлено</response>
    /// <response code="404">Тип продукту з таким ID не існує</response>
    /// <response code="409">Тип продукту з такою назвою вже існує</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ProductTypeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
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
            ShelfLifeHours = request.ShelfLifeHours,
            Meta = request.Meta
        };

        var result = await sender.Send(command, cancellationToken);

        return result.Match<ActionResult<ProductTypeDto>>(
            pt => ProductTypeDto.FromDomainModel(pt),
            e => e.ToObjectResult());
    }

    /// <summary>
    /// Видалити тип продукту
    /// </summary>
    /// <param name="id">ID типу продукту для видалення</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Видалений тип продукту</returns>
    /// <response code="200">Тип продукту успішно видалено</response>
    /// <response code="404">Тип продукту з таким ID не існує</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ProductTypeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
using Api.Dtos;
using Api.Modules.Errors;
using Application.Common.Interfaces.Queries;
using Application.Products.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

/// <summary>
/// Управління продуктами. Продукт — це конкретний товар (наприклад "Молоко Галичина 2.5%"),
/// який належить до певного типу продукту і може бути залитий у сумісний контейнер.
/// </summary>
[ApiController]
[Route("products")]
[Microsoft.AspNetCore.Authorization.Authorize(Policy = "MustBeActive")]
public class ProductsController(
    ISender sender,
    IProductQueries productQueries) : ControllerBase
{
    /// <summary>
    /// Отримати список усіх продуктів
    /// </summary>
    /// <returns>Масив усіх продуктів з інформацією про їх тип</returns>
    /// <response code="200">Список продуктів успішно повернуто</response>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ProductDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProductDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        var products = await productQueries.GetAllAsync(cancellationToken);
        return products.Select(ProductDto.FromDomainModel).ToList();
    }

    /// <summary>
    /// Пошук продуктів за назвою та/або типом продукту
    /// </summary>
    /// <param name="search">Текст для пошуку за назвою (часткове входження)</param>
    /// <param name="productTypeId">Фільтр по ID типу продукту</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Масив продуктів, що відповідають критеріям</returns>
    /// <response code="200">Результати пошуку повернуто</response>
    [HttpGet("search")]
    [ProducesResponseType(typeof(IReadOnlyList<ProductDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProductDto>>> Search(
        [FromQuery] string? search,
        [FromQuery] int? productTypeId,
        CancellationToken cancellationToken)
    {
        var products = await productQueries.SearchAsync(search, productTypeId, cancellationToken);
        return products.Select(ProductDto.FromDomainModel).ToList();
    }

    /// <summary>
    /// Отримати продукт за його ID
    /// </summary>
    /// <param name="id">Унікальний ідентифікатор продукту</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Продукт з вказаним ID</returns>
    /// <response code="200">Продукт знайдено</response>
    /// <response code="404">Продукт з таким ID не існує</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDto>> GetById(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        var product = await productQueries.GetByIdAsync(id, cancellationToken);

        return product.Match<ActionResult<ProductDto>>(
            p => ProductDto.FromDomainModel(p),
            () => NotFound());
    }

    /// <summary>
    /// Створити новий продукт
    /// </summary>
    /// <param name="request">Дані нового продукту</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Створений продукт</returns>
    /// <response code="200">Продукт успішно створено</response>
    /// <response code="400">Невалідні дані (наприклад, неіснуючий тип продукту)</response>
    /// <response code="409">Продукт з такою назвою вже існує</response>
    [HttpPost]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
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

    /// <summary>
    /// Оновити існуючий продукт
    /// </summary>
    /// <param name="id">ID продукту, який оновлюється</param>
    /// <param name="request">Нові дані продукту</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Оновлений продукт</returns>
    /// <response code="200">Продукт успішно оновлено</response>
    /// <response code="404">Продукт з таким ID не існує</response>
    /// <response code="409">Продукт з такою назвою вже існує</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
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

    /// <summary>
    /// Видалити продукт
    /// </summary>
    /// <param name="id">ID продукту для видалення</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Видалений продукт</returns>
    /// <response code="200">Продукт успішно видалено</response>
    /// <response code="404">Продукт з таким ID не існує</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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

using Api.Dtos;
using Api.Modules.Errors;
using Application.Common.Interfaces.Queries;
using Application.ContainerTypes.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

/// <summary>
/// Управління типами контейнерів (наприклад: "Металева бочка", "Пластикова каністра").
/// Тип контейнера визначає, які продукти можна в нього заливати.
/// </summary>
[ApiController]
[Route("container-types")]
[Microsoft.AspNetCore.Authorization.Authorize(Policy = "MustBeActive")]
public class ContainerTypesController(
    ISender sender, 
    IContainerTypeQueries containerTypeQueries) : ControllerBase
{
    /// <summary>
    /// Отримати список усіх типів контейнерів
    /// </summary>
    /// <returns>Масив усіх типів контейнерів з їх допустимими типами продуктів</returns>
    /// <response code="200">Список типів контейнерів успішно повернуто</response>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ContainerTypeDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ContainerTypeDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        var containerTypes = await containerTypeQueries.GetAllAsync(cancellationToken);
        return containerTypes.Select(ContainerTypeDto.FromDomainModel).ToList();
    }

    /// <summary>
    /// Отримати тип контейнера за його ID
    /// </summary>
    /// <param name="id">Унікальний ідентифікатор типу контейнера</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Тип контейнера з вказаним ID</returns>
    /// <response code="200">Тип контейнера знайдено</response>
    /// <response code="404">Тип контейнера з таким ID не існує</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ContainerTypeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContainerTypeDto>> GetById(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        var containerType = await containerTypeQueries.GetByIdAsync(
            id, 
            cancellationToken);

        return containerType.Match<ActionResult<ContainerTypeDto>>(
            ct => ContainerTypeDto.FromDomainModel(ct),
            () => NotFound());
    }

    /// <summary>
    /// Створити новий тип контейнера
    /// </summary>
    /// <param name="request">Дані нового типу контейнера</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Створений тип контейнера</returns>
    /// <response code="200">Тип контейнера успішно створено</response>
    /// <response code="409">Тип контейнера з такою назвою вже існує</response>
    [HttpPost]
    [ProducesResponseType(typeof(ContainerTypeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ContainerTypeDto>> Create(
        [FromBody] CreateContainerTypeDto request,
        CancellationToken cancellationToken)
    {
        var command = new CreateContainerTypeCommand
        {
            Name = request.Name,
            CodePrefix = request.CodePrefix,
            DefaultUnit = request.DefaultUnit,
            Meta = request.Meta,
            AllowedProductTypeIds = request.AllowedProductTypeIds
        };

        var result = await sender.Send(command, cancellationToken);

        return result.Match<ActionResult<ContainerTypeDto>>(
            ct => ContainerTypeDto.FromDomainModel(ct),
            e => e.ToObjectResult());
    }

    /// <summary>
    /// Оновити існуючий тип контейнера
    /// </summary>
    /// <param name="id">ID типу контейнера, який оновлюється</param>
    /// <param name="request">Нові дані типу контейнера</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Оновлений тип контейнера</returns>
    /// <response code="200">Тип контейнера успішно оновлено</response>
    /// <response code="404">Тип контейнера з таким ID не існує</response>
    /// <response code="409">Тип контейнера з такою назвою вже існує</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ContainerTypeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ContainerTypeDto>> Update(
        [FromRoute] int id,
        [FromBody] UpdateContainerTypeDto request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateContainerTypeCommand
        {
            ContainerTypeId = id,
            Name = request.Name,
            CodePrefix = request.CodePrefix,
            DefaultUnit = request.DefaultUnit,
            Meta = request.Meta,
            AllowedProductTypeIds = request.AllowedProductTypeIds
        };

        var result = await sender.Send(command, cancellationToken);

        return result.Match<ActionResult<ContainerTypeDto>>(
            ct => ContainerTypeDto.FromDomainModel(ct),
            e => e.ToObjectResult());
    }

    /// <summary>
    /// Видалити тип контейнера
    /// </summary>
    /// <param name="id">ID типу контейнера для видалення</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Видалений тип контейнера</returns>
    /// <response code="200">Тип контейнера успішно видалено</response>
    /// <response code="404">Тип контейнера з таким ID не існує</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ContainerTypeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContainerTypeDto>> Delete(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteContainerTypeCommand(id);
        var result = await sender.Send(command, cancellationToken);

        return result.Match<ActionResult<ContainerTypeDto>>(
            ct => ContainerTypeDto.FromDomainModel(ct),
            e => e.ToObjectResult());
    }
}

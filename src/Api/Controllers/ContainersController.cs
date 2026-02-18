using Api.Dtos;
using Api.Modules.Errors;
using Application.Common.Interfaces.Queries;
using Application.Containers.Commands;
using Application.Containers.Queries.SearchContainers;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

/// <summary>
/// Управління контейнерами (тарою). Контейнер — це фізична одиниця зберігання (бочка, каністра тощо).
/// Підтримує CRUD, наповнення/спорожнення, пошук та перегляд історії наповнень.
/// </summary>
[ApiController]
[Route("containers")]
[Microsoft.AspNetCore.Authorization.Authorize(Policy = "MustBeActive")]
public class ContainersController(
    ISender sender, 
    IContainerQueries containerQueries) : ControllerBase
{
    /// <summary>
    /// Отримати список усіх контейнерів
    /// </summary>
    /// <returns>Масив усіх контейнерів з їх поточним станом</returns>
    /// <response code="200">Список контейнерів успішно повернуто</response>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ContainerDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ContainerDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        var containers = await containerQueries.GetAllAsync(cancellationToken);
        return containers.Select(ContainerDto.FromDomainModel).ToList();
    }

    /// <summary>
    /// Пошук контейнерів за різними критеріями (фільтрація)
    /// </summary>
    /// <param name="searchDto">Параметри пошуку (всі необов'язкові, можна комбінувати)</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Масив контейнерів, що відповідають критеріям пошуку</returns>
    /// <response code="200">Результати пошуку повернуто</response>
    [HttpGet("search")]
    [ProducesResponseType(typeof(IReadOnlyList<ContainerDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ContainerDto>>> Search(
        [FromQuery] SearchContainersDto searchDto,
        CancellationToken cancellationToken)
    {
        var query = new SearchContainersQuery(
            searchDto.SearchTerm, 
            searchDto.ContainerTypeId, 
            searchDto.Status,
            searchDto.ProductionDate,
            searchDto.CurrentProductId,
            searchDto.CurrentProductTypeId,
            searchDto.LastProductId,
            searchDto.ShowExpired,
            searchDto.FilledToday
        );
        var containers = await sender.Send(query, cancellationToken);
        return containers.Select(ContainerDto.FromDomainModel).ToList();
    }

    /// <summary>
    /// Отримати контейнер за його ID
    /// </summary>
    /// <param name="id">Унікальний ідентифікатор контейнера</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Контейнер з вказаним ID</returns>
    /// <response code="200">Контейнер знайдено</response>
    /// <response code="404">Контейнер з таким ID не існує</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ContainerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContainerDto>> GetById(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        var container = await containerQueries.GetByIdAsync(id, cancellationToken);

        return container.Match<ActionResult<ContainerDto>>(
            c => ContainerDto.FromDomainModel(c),
            () => NotFound());
    }

    /// <summary>
    /// Отримати контейнер за його унікальним кодом (наприклад "МБ-0001")
    /// </summary>
    /// <param name="code">Унікальний код контейнера</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Контейнер з вказаним кодом</returns>
    /// <response code="200">Контейнер знайдено</response>
    /// <response code="404">Контейнер з таким кодом не існує</response>
    [HttpGet("code/{code}")]
    [ProducesResponseType(typeof(ContainerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContainerDto>> GetByCode(
        [FromRoute] string code,
        CancellationToken cancellationToken)
    {
        var container = await containerQueries.GetByCodeAsync(code, cancellationToken);

        return container.Match<ActionResult<ContainerDto>>(
            c => ContainerDto.FromDomainModel(c),
            () => NotFound());
    }

    /// <summary>
    /// Створити новий контейнер
    /// </summary>
    /// <param name="request">Дані нового контейнера</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Створений контейнер</returns>
    /// <response code="200">Контейнер успішно створено</response>
    /// <response code="400">Невалідні дані (наприклад, неіснуючий тип контейнера)</response>
    /// <response code="409">Контейнер з таким кодом вже існує</response>
    [HttpPost]
    [ProducesResponseType(typeof(ContainerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ContainerDto>> Create(
        [FromBody] CreateContainerDto request,
        CancellationToken cancellationToken)
    {
        var command = new CreateContainerCommand
        {
            Code = request.Code,
            Name = request.Name,
            Volume = request.Volume,
            Unit = request.Unit,
            ContainerTypeId = request.ContainerTypeId,
            Meta = request.Meta
        };

        var result = await sender.Send(command, cancellationToken);

        return result.Match<ActionResult<ContainerDto>>(
            c => ContainerDto.FromDomainModel(c),
            e => e.ToObjectResult());
    }

    /// <summary>
    /// Оновити існуючий контейнер
    /// </summary>
    /// <param name="id">ID контейнера, який оновлюється</param>
    /// <param name="request">Нові дані контейнера (код змінити не можна)</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Оновлений контейнер</returns>
    /// <response code="200">Контейнер успішно оновлено</response>
    /// <response code="404">Контейнер з таким ID не існує</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ContainerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContainerDto>> Update(
        [FromRoute] int id,
        [FromBody] UpdateContainerDto request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateContainerCommand
        {
            ContainerId = id,
            Name = request.Name,
            Volume = request.Volume,
            Unit = request.Unit,
            ContainerTypeId = request.ContainerTypeId,
            Meta = request.Meta
        };

        var result = await sender.Send(command, cancellationToken);

        return result.Match<ActionResult<ContainerDto>>(
            c => ContainerDto.FromDomainModel(c),
            e => e.ToObjectResult());
    }

    /// <summary>
    /// Видалити контейнер
    /// </summary>
    /// <param name="id">ID контейнера для видалення</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Видалений контейнер</returns>
    /// <response code="200">Контейнер успішно видалено</response>
    /// <response code="404">Контейнер з таким ID не існує</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ContainerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContainerDto>> Delete(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteContainerCommand(id);
        var result = await sender.Send(command, cancellationToken);

        return result.Match<ActionResult<ContainerDto>>(
            c => ContainerDto.FromDomainModel(c),
            e => e.ToObjectResult());
    }

    /// <summary>
    /// Наповнити контейнер продуктом. Контейнер повинен бути порожнім (статус Empty).
    /// Продукт повинен бути сумісним з типом контейнера.
    /// </summary>
    /// <param name="id">ID контейнера, який наповнюється</param>
    /// <param name="request">Дані наповнення (продукт, кількість, дати)</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Наповнений контейнер з оновленим статусом Filled</returns>
    /// <response code="200">Контейнер успішно наповнено</response>
    /// <response code="400">Контейнер вже наповнений або продукт не сумісний з типом контейнера</response>
    /// <response code="404">Контейнер або продукт не знайдено</response>
    [HttpPost("{id:int}/fill")]
    [ProducesResponseType(typeof(ContainerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContainerDto>> Fill(
        [FromRoute] int id,
        [FromBody] FillContainerDto request,
        CancellationToken cancellationToken)
    {
        var command = new FillContainerCommand
        {
            ContainerId = id,
            ProductId = request.ProductId,
            Quantity = request.Quantity,
            Unit = request.Unit,
            ProductionDate = request.ProductionDate,
            ExpirationDate = request.ExpirationDate
        };

        var result = await sender.Send(command, cancellationToken);

        return result.Match<ActionResult<ContainerDto>>(
            c => ContainerDto.FromDomainModel(c),
            e => e.ToObjectResult());
    }

    /// <summary>
    /// Спорожнити контейнер. Контейнер повинен бути наповненим (статус Filled).
    /// Поточне наповнення зберігається в історії.
    /// </summary>
    /// <param name="id">ID контейнера, який спорожнюється</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Спорожнений контейнер з оновленим статусом Empty</returns>
    /// <response code="200">Контейнер успішно спорожнено</response>
    /// <response code="400">Контейнер вже порожній</response>
    /// <response code="404">Контейнер не знайдено</response>
    [HttpPost("{id:int}/empty")]
    [ProducesResponseType(typeof(ContainerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public new async Task<ActionResult<ContainerDto>> Empty(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        var command = new EmptyContainerCommand
        {
            ContainerId = id
        };

        var result = await sender.Send(command, cancellationToken);

        return result.Match<ActionResult<ContainerDto>>(
            c => ContainerDto.FromDomainModel(c),
            e => e.ToObjectResult());
    }

    /// <summary>
    /// Оновити дані поточного наповнення контейнера (кількість, дати тощо).
    /// Контейнер повинен бути наповненим.
    /// </summary>
    /// <param name="id">ID контейнера</param>
    /// <param name="request">Оновлені дані наповнення</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Контейнер з оновленими даними наповнення</returns>
    /// <response code="200">Наповнення успішно оновлено</response>
    /// <response code="400">Контейнер порожній або дані невалідні</response>
    /// <response code="404">Контейнер не знайдено</response>
    [HttpPut("{id:int}/fill")]
    [ProducesResponseType(typeof(ContainerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContainerDto>> UpdateFill(
        [FromRoute] int id,
        [FromBody] UpdateContainerFillDto request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateContainerFillCommand
        {
            ContainerId = id,
            ProductId = request.ProductId,
            Quantity = request.Quantity,
            Unit = request.Unit,
            ProductionDate = request.ProductionDate,
            ExpirationDate = request.ExpirationDate
        };

        var result = await sender.Send(command, cancellationToken);

        return result.Match<ActionResult<ContainerDto>>(
            c => ContainerDto.FromDomainModel(c),
            e => e.ToObjectResult());
    }

    /// <summary>
    /// Пошук по історії наповнень контейнерів (по всіх контейнерах)
    /// </summary>
    /// <param name="request">Фільтри пошуку (всі необов'язкові)</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Масив записів наповнень, що відповідають критеріям</returns>
    /// <response code="200">Результати пошуку повернуто</response>
    [HttpGet("fills/search")]
    [ProducesResponseType(typeof(IReadOnlyList<ContainerFillDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ContainerFillDto>>> SearchFills(
        [FromQuery] SearchContainerFillsDto request,
        CancellationToken cancellationToken)
    {
        var query = new Application.Containers.Queries.SearchContainerFills.SearchContainerFillsQuery(
            request.ProductId,
            request.ProductTypeId,
            request.ContainerId,
            request.FromDate,
            request.ToDate,
            request.OnlyActive
        );

        var fills = await sender.Send(query, cancellationToken);

        return fills.Select(ContainerFillDto.FromDomainModel).ToList();
    }

    /// <summary>
    /// Отримати повну історію наповнень/спорожнень конкретного контейнера
    /// </summary>
    /// <param name="id">ID контейнера</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Масив усіх наповнень контейнера (від найновіших до найстаріших)</returns>
    /// <response code="200">Історію повернуто</response>
    [HttpGet("{id:int}/history")]
    [ProducesResponseType(typeof(IReadOnlyList<ContainerFillDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ContainerFillDto>>> GetHistory(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        var query = new Application.Containers.Queries.GetContainerHistoryQuery
        {
            ContainerId = id
        };

        var fills = await sender.Send(query, cancellationToken);

        return fills.Select(ContainerFillDto.FromDomainModel).ToList();
    }
}

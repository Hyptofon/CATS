using Api.Dtos;
using Api.Modules.Errors;
using Application.Common.Interfaces.Queries;
using Application.Containers.Commands;
using Application.Containers.Queries.SearchContainers;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("containers")]
public class ContainersController(
    ISender sender, 
    IContainerQueries containerQueries) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ContainerDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        var containers = await containerQueries.GetAllAsync(cancellationToken);
        return containers.Select(ContainerDto.FromDomainModel).ToList();
    }

    [HttpGet("search")]
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

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ContainerDto>> GetById(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        var container = await containerQueries.GetByIdAsync(id, cancellationToken);

        return container.Match<ActionResult<ContainerDto>>(
            c => ContainerDto.FromDomainModel(c),
            () => NotFound());
    }

    [HttpGet("code/{code}")]
    public async Task<ActionResult<ContainerDto>> GetByCode(
        [FromRoute] string code,
        CancellationToken cancellationToken)
    {
        var container = await containerQueries.GetByCodeAsync(code, cancellationToken);

        return container.Match<ActionResult<ContainerDto>>(
            c => ContainerDto.FromDomainModel(c),
            () => NotFound());
    }

    [HttpPost]
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

    [HttpPut("{id:int}")]
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

    [HttpDelete("{id:int}")]
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

    [HttpPost("{id:int}/fill")]
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

    [HttpPost("{id:int}/empty")]
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

    [HttpPut("{id:int}/fill")]
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

    [HttpGet("fills/search")]
    public async Task<ActionResult<IReadOnlyList<ContainerFillDto>>> SearchFills(
        [FromQuery] int? productId,
        [FromQuery] int? productTypeId,
        [FromQuery] int? containerId,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] bool? onlyActive,
        CancellationToken cancellationToken)
    {
        var query = new Application.Containers.Queries.SearchContainerFills.SearchContainerFillsQuery(
            productId,
            productTypeId,
            containerId,
            fromDate,
            toDate,
            onlyActive
        );

        var fills = await sender.Send(query, cancellationToken);

        return fills.Select(f => new ContainerFillDto
        {
            Id = f.Id,
            ContainerId = f.ContainerId,
            ContainerCode = f.Container?.Code,
            ProductId = f.ProductId,
            ProductName = f.Product?.Name ?? string.Empty,
            Quantity = f.Quantity,
            Unit = f.Unit,
            ProductionDate = f.ProductionDate,
            FilledDate = f.FilledDate,
            ExpirationDate = f.ExpirationDate,
            EmptiedDate = f.EmptiedDate,
            FilledByUserId = f.FilledByUserId,
            EmptiedByUserId = f.EmptiedByUserId
        }).ToList();
    }

    [HttpGet("{id:int}/history")]
    public async Task<ActionResult<IReadOnlyList<ContainerFillDto>>> GetHistory(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        var query = new Application.Containers.Queries.GetContainerHistoryQuery
        {
            ContainerId = id
        };

        var fills = await sender.Send(query, cancellationToken);

        var dtos = fills.Select(f => new ContainerFillDto
        {
            Id = f.Id,
            ContainerId = f.ContainerId,
            ContainerCode = f.Container?.Code,
            ProductId = f.ProductId,
            ProductName = f.Product?.Name ?? string.Empty,
            Quantity = f.Quantity,
            Unit = f.Unit,
            ProductionDate = f.ProductionDate,
            FilledDate = f.FilledDate,
            ExpirationDate = f.ExpirationDate,
            EmptiedDate = f.EmptiedDate,
            FilledByUserId = f.FilledByUserId,
            EmptiedByUserId = f.EmptiedByUserId
        }).ToList();

        return dtos;
    }
}

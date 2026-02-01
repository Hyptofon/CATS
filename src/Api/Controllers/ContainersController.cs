using Api.Dtos;
using Api.Modules.Errors;
using Application.Common.Interfaces.Queries;
using Application.Containers.Commands;
using Application.Containers.Queries.SearchContainers;
using Domain.Containers;
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
        [FromQuery] string? searchTerm,
        [FromQuery] Guid? containerTypeId,
        [FromQuery] string? status,
        CancellationToken cancellationToken)
    {
        var query = new SearchContainersQuery(
            searchTerm, 
            containerTypeId, 
            status
        );
        var containers = await sender.Send(query, cancellationToken);
        return containers.Select(ContainerDto.FromDomainModel).ToList();
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ContainerDto>> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var container = await containerQueries.GetByIdAsync(
            new ContainerId(id), 
            cancellationToken);

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
            ContainerTypeId = request.ContainerTypeId,
            Meta = request.Meta
        };

        var result = await sender.Send(command, cancellationToken);

        return result.Match<ActionResult<ContainerDto>>(
            c => ContainerDto.FromDomainModel(c),
            e => e.ToObjectResult());
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ContainerDto>> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateContainerDto request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateContainerCommand
        {
            ContainerId = id,
            Name = request.Name,
            Volume = request.Volume,
            ContainerTypeId = request.ContainerTypeId,
            Meta = request.Meta
        };

        var result = await sender.Send(command, cancellationToken);

        return result.Match<ActionResult<ContainerDto>>(
            c => ContainerDto.FromDomainModel(c),
            e => e.ToObjectResult());
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ContainerDto>> Delete(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteContainerCommand(id);
        var result = await sender.Send(command, cancellationToken);

        return result.Match<ActionResult<ContainerDto>>(
            c => ContainerDto.FromDomainModel(c),
            e => e.ToObjectResult());
    }
}
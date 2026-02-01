using Api.Dtos;
using Api.Modules.Errors;
using Application.Common.Interfaces.Queries;
using Application.ContainerTypes.Commands;
using Domain.ContainerTypes;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("container-types")]
public class ContainerTypesController(
    ISender sender, 
    IContainerTypeQueries containerTypeQueries) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ContainerTypeDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        var containerTypes = await containerTypeQueries.GetAllAsync(cancellationToken);
        return containerTypes.Select(ContainerTypeDto.FromDomainModel).ToList();
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ContainerTypeDto>> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var containerType = await containerTypeQueries.GetByIdAsync(
            new ContainerTypeId(id), 
            cancellationToken);

        return containerType.Match<ActionResult<ContainerTypeDto>>(
            ct => ContainerTypeDto.FromDomainModel(ct),
            () => NotFound());
    }

    [HttpPost]
    public async Task<ActionResult<ContainerTypeDto>> Create(
        [FromBody] CreateContainerTypeDto request,
        CancellationToken cancellationToken)
    {
        var command = new CreateContainerTypeCommand
        {
            Name = request.Name,
            Meta = request.Meta
        };

        var result = await sender.Send(command, cancellationToken);

        return result.Match<ActionResult<ContainerTypeDto>>(
            ct => ContainerTypeDto.FromDomainModel(ct),
            e => e.ToObjectResult());
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ContainerTypeDto>> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateContainerTypeDto request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateContainerTypeCommand
        {
            ContainerTypeId = id,
            Name = request.Name,
            Meta = request.Meta
        };

        var result = await sender.Send(command, cancellationToken);

        return result.Match<ActionResult<ContainerTypeDto>>(
            ct => ContainerTypeDto.FromDomainModel(ct),
            e => e.ToObjectResult());
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ContainerTypeDto>> Delete(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteContainerTypeCommand(id);
        var result = await sender.Send(command, cancellationToken);

        return result.Match<ActionResult<ContainerTypeDto>>(
            ct => ContainerTypeDto.FromDomainModel(ct),
            e => e.ToObjectResult());
    }
}
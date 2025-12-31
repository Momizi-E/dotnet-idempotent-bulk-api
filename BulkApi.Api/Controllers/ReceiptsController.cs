using BulkApi.Application.DTOs;
using BulkApi.Application.Receipts;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BulkApi.Api.Controllers;

[ApiController]
[Route("api/receipts")]
public sealed class ReceiptsController : ControllerBase
{
    private readonly IMediator _mediator;
    public ReceiptsController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    public async Task<ActionResult<ReceiptResponse>> Create(
        [FromBody] CreateReceiptRequest request,
        [FromHeader(Name = "Idempotency-Key")] string? key,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Title)) return BadRequest("Title is required.");
        if (request.Amount <= 0) return BadRequest("Amount must be greater than 0.");
        if (string.IsNullOrWhiteSpace(request.Currency)) return BadRequest("Currency is required.");

        var res = await _mediator.Send(new CreateReceiptCommand(request, key), ct);
        return CreatedAtAction(nameof(GetById), new { id = res.Id }, res);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ReceiptResponse>> GetById(Guid id, CancellationToken ct)
    {
        var res = await _mediator.Send(new GetReceiptByIdQuery(id), ct);
        return res is null ? NotFound() : Ok(res);
    }
}

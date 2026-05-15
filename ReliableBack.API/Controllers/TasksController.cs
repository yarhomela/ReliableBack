using MediatR;
using Microsoft.AspNetCore.Mvc;
using ReliableBack.Application.Tasks;
using ReliableBack.Application.Tasks.Commands.EnqueueTask;
using ReliableBack.Application.Tasks.Queries.GetTaskById;
using ReliableBack.Application.Tasks.Queries.GetTaskList;
using ReliableBack.Domain.Tasks;

namespace ReliableBack.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly IMediator _mediator;

    public TasksController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> EnqueueTask([FromBody] EnqueueTaskCommand command, CancellationToken cancellationToken)
    {
        var taskId = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = taskId }, taskId);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var task = await _mediator.Send(new GetTaskByIdQuery(id), cancellationToken);

        return task is null
            ? NotFound()
            : Ok(task);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TaskDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] JobStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetTaskListQuery(status, page, pageSize);

        var tasks = await _mediator.Send(query, cancellationToken);
        return Ok(tasks);
    }
}
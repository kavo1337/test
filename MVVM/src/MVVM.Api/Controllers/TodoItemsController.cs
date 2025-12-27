using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVVM.Api.Data;
using MVVM.Api.Entities;
using MVVM.Shared.Dtos;

namespace MVVM.Api.Controllers;

[ApiController]
[Route("api/todos")]
public sealed class TodoItemsController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public TodoItemsController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TodoItemDto>>> GetAll(CancellationToken cancellationToken)
    {
        var items = await _dbContext.TodoItems
            .AsNoTracking()
            .OrderBy(item => item.Id)
            .Select(item => new TodoItemDto
            {
                Id = item.Id,
                Title = item.Title,
                IsDone = item.IsDone
            })
            .ToListAsync(cancellationToken);

        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TodoItemDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var item = await _dbContext.TodoItems
            .AsNoTracking()
            .Where(todo => todo.Id == id)
            .Select(todo => new TodoItemDto
            {
                Id = todo.Id,
                Title = todo.Title,
                IsDone = todo.IsDone
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (item is null)
        {
            return NotFound();
        }

        return Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<TodoItemDto>> Create(TodoItemCreateDto input, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(input.Title))
        {
            return BadRequest("Название задачи обязательно.");
        }

        var trimmedTitle = input.Title.Trim();
        if (trimmedTitle.Length > 200)
        {
            return BadRequest("Название задачи не должно превышать 200 символов.");
        }

        var entity = new TodoItem
        {
            Title = trimmedTitle,
            IsDone = false
        };

        _dbContext.TodoItems.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var result = new TodoItemDto
        {
            Id = entity.Id,
            Title = entity.Title,
            IsDone = entity.IsDone
        };

        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, TodoItemUpdateDto input, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(input.Title))
        {
            return BadRequest("Название задачи обязательно.");
        }

        var trimmedTitle = input.Title.Trim();
        if (trimmedTitle.Length > 200)
        {
            return BadRequest("Название задачи не должно превышать 200 символов.");
        }

        var entity = await _dbContext.TodoItems.FirstOrDefaultAsync(todo => todo.Id == id, cancellationToken);
        if (entity is null)
        {
            return NotFound();
        }

        entity.Title = trimmedTitle;
        entity.IsDone = input.IsDone;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.TodoItems.FirstOrDefaultAsync(todo => todo.Id == id, cancellationToken);
        if (entity is null)
        {
            return NotFound();
        }

        _dbContext.TodoItems.Remove(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}

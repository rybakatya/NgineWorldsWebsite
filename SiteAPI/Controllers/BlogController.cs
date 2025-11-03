using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SiteAPI.Data;
using SiteAPI.Models;
using System.Text.RegularExpressions;

[ApiController]
[Route("api/[controller]")]
public class BlogController : ControllerBase
{
    private readonly AppDbContext _db;
    public BlogController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] int page = 1, [FromQuery] int size = 10)
    {
        var q = _db.BlogPosts

            .OrderByDescending(p => p.CreatedUtc)
            .Select(p => new { p.Id, p.Title, p.Slug, p.Description, p.CreatedUtc });

        var items = await q.Skip((page - 1) * size).Take(size).ToListAsync();
        return Ok(items);
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug(string slug)
    {
        var post = await _db.BlogPosts.FirstOrDefaultAsync(p => p.Slug == slug);
        return post is null ? NotFound() : Ok(post);
    }

    [Authorize(Roles = "Admin,Moderator")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] BlogPost dto)
    {
        dto.Id = 0;
        dto.Slug = Slugify(dto.Slug ?? dto.Title); // ensure unique separately
        dto.CreatedUtc = DateTime.UtcNow;

        _db.BlogPosts.Add(dto);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetBySlug), new { slug = dto.Slug }, dto);
    }

    private static string Slugify(string input)
        => Regex.Replace(input.ToLowerInvariant().Trim(), @"[^a-z0-9]+", "-").Trim('-');
}

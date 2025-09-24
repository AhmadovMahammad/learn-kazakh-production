using LearnKazakh.Core.Repositories;
using LearnKazakh.Core.UnitOfWork;
using LearnKazakh.Domain.Entities;
using LearnKazakh.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LearnKazakh.API.Controllers;

[Route("api/section")]
[ApiController]
public class SectionController(IUnitOfWork unitOfWork) : ControllerBase
{
    private readonly ISectionRepository _sectionRepository = unitOfWork?.SectionRepository ??
            throw new ArgumentNullException(nameof(unitOfWork.SectionRepository));

    private readonly ICategoryRepository _categoryRepository = unitOfWork?.CategoryRepository ??
            throw new ArgumentNullException(nameof(unitOfWork.CategoryRepository));

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<DetailedSectionDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSections()
    {
        try
        {
            List<DetailedSectionDto> sections = await _sectionRepository.GetAll()
                .Include(s => s.Category)
                .Include(s => s.Contents)
                .AsNoTracking()
                .OrderBy(s => s.CategoryId)
                .ThenBy(s => s.Order)
                .Select(s => new DetailedSectionDto
                {
                    Id = s.Id,
                    Title = s.Title,
                    Order = s.Order,
                    CategoryId = s.CategoryId,
                    CategoryName = s.Category != null ? s.Category.Name : string.Empty,
                    ContentCount = s.Contents.Count,
                    CreatedAt = s.CreatedAt,
                    CreatedBy = s.CreatedBy,
                    LastModifiedAt = s.LastModifiedAt,
                    LastModifiedBy = s.LastModifiedBy
                }).ToListAsync();

            return Ok(ApiResponse<List<DetailedSectionDto>>.SuccessResult(sections));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiPagedResponse<DetailedSectionDto>.ErrorResult(ex, "An error occurred while retrieving sections"));
        }
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<DetailedSectionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<DetailedSectionDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSection(Guid id)
    {
        try
        {
            Section? section = await _sectionRepository.GetAsync
            (
                s => s.Id == id,
                include: s => s.Include(s => s.Category).Include(s => s.Contents)
            );

            if (section == null)
            {
                return NotFound(ApiResponse<DetailedSectionDto>.ErrorResult(
                    new KeyNotFoundException("Section not found"),
                    "Section not found"));
            }

            DetailedSectionDto sectionDto = new DetailedSectionDto
            {
                Id = section.Id,
                Title = section.Title,
                Order = section.Order,
                CategoryId = section.CategoryId,
                CategoryName = section.Category?.Name ?? string.Empty,
                ContentCount = section.Contents.Count,
                CreatedAt = section.CreatedAt,
                CreatedBy = section.CreatedBy,
                LastModifiedAt = section.LastModifiedAt,
                LastModifiedBy = section.LastModifiedBy
            };

            return Ok(ApiResponse<DetailedSectionDto>.SuccessResult(sectionDto));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<DetailedSectionDto>.ErrorResult(ex, "An error occurred while retrieving the section"));
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Instructor")]
    [ProducesResponseType(typeof(ApiResponse<DetailedSectionDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<DetailedSectionDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateSection([FromBody] CreateSectionDto createSectionDto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(createSectionDto.Title))
            {
                return BadRequest(ApiResponse<DetailedSectionDto>.ErrorResult(
                    new ArgumentException("Section title is required"),
                    "Invalid section data"));
            }

            Category? category = await _categoryRepository.GetAsync(c => c.Id == createSectionDto.CategoryId);
            if (category == null)
            {
                return BadRequest(ApiResponse<DetailedSectionDto>.ErrorResult(
                    new KeyNotFoundException("Category not found"),
                    "Invalid category"));
            }

            Section section = new Section
            {
                Title = createSectionDto.Title,
                Order = createSectionDto.Order,
                CategoryId = createSectionDto.CategoryId
            };

            await _sectionRepository.CreateAsync(section);
            await unitOfWork.SaveChangesAsync();

            DetailedSectionDto result = new DetailedSectionDto
            {
                Id = section.Id,
                Title = section.Title,
                Order = section.Order,
                CategoryId = section.CategoryId,
                CategoryName = category.Name,
                ContentCount = 0,
                CreatedAt = section.CreatedAt,
                CreatedBy = section.CreatedBy
            };

            return CreatedAtAction(nameof(GetSection), ApiResponse<DetailedSectionDto>.SuccessResult(result, "Section created successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<DetailedSectionDto>.ErrorResult(ex, "Section creation failed"));
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Instructor")]
    [ProducesResponseType(typeof(ApiResponse<DetailedSectionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<DetailedSectionDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<DetailedSectionDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateSection(Guid id, [FromBody] UpdateSectionDto updateSectionDto)
    {
        try
        {
            if (id != updateSectionDto.Id)
            {
                return BadRequest(ApiResponse<DetailedSectionDto>.ErrorResult(
                    new ArgumentException("ID mismatch"),
                    "Invalid request"));
            }

            Section? section = await _sectionRepository.GetAsync
            (
                predicate: s => s.Id == id,
                include: s => s.Include(s => s.Category)
            );

            if (section == null)
            {
                return NotFound(ApiResponse<DetailedSectionDto>.ErrorResult(
                    new KeyNotFoundException("Section not found"),
                    "Section not found"));
            }

            if (section.CategoryId != updateSectionDto.CategoryId)
            {
                Category? category = await _categoryRepository.GetAsync(c => c.Id == updateSectionDto.CategoryId);
                if (category == null)
                {
                    return BadRequest(ApiResponse<DetailedSectionDto>.ErrorResult(
                        new KeyNotFoundException("Category not found"),
                        "Invalid category"));
                }
            }

            section.Title = updateSectionDto.Title;
            section.Order = updateSectionDto.Order;
            section.CategoryId = updateSectionDto.CategoryId;

            await _sectionRepository.UpdateAsync(section);
            await unitOfWork.SaveChangesAsync();

            section = await _sectionRepository.GetAsync
            (
                s => s.Id == id,
                include: s => s.Include(s => s.Category).Include(s => s.Contents)
            );

            DetailedSectionDto result = new DetailedSectionDto
            {
                Id = section!.Id,
                Title = section.Title,
                Order = section.Order,
                CategoryId = section.CategoryId,
                CategoryName = section.Category?.Name ?? string.Empty,
                ContentCount = section.Contents.Count,
                CreatedAt = section.CreatedAt,
                CreatedBy = section.CreatedBy,
                LastModifiedAt = section.LastModifiedAt,
                LastModifiedBy = section.LastModifiedBy
            };

            return Ok(ApiResponse<DetailedSectionDto>.SuccessResult(result, "Section updated successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<DetailedSectionDto>.ErrorResult(ex, "Section update failed"));
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSection(Guid id)
    {
        try
        {
            Section? section = await _sectionRepository.GetAsync(s => s.Id == id);
            if (section == null)
            {
                return NotFound(ApiResponse<object>.ErrorResult(
                    new KeyNotFoundException("Section not found"),
                    "Section not found"));
            }

            await _sectionRepository.DeleteAsync(section);
            await unitOfWork.SaveChangesAsync();

            return Ok(ApiResponse<object>.SuccessResult(new object(), "Section deleted successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResult(ex, "Section deletion failed"));
        }
    }
}
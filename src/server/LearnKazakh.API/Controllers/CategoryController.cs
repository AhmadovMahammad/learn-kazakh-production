using LearnKazakh.Core.Repositories;
using LearnKazakh.Core.UnitOfWork;
using LearnKazakh.Domain.Entities;
using LearnKazakh.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LearnKazakh.API.Controllers;

[Route("api/category")]
[ApiController]
public class CategoryController(IUnitOfWork unitOfWork) : Controller
{
    private readonly ICategoryRepository _categoryRepository = unitOfWork.CategoryRepository ??
        throw new ArgumentNullException(nameof(unitOfWork.CategoryRepository), "Category repository is not initialized.");

    // GET: api/category
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<CategoryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCategories()
    {
        try
        {
            List<CategoryDto> categories = await _categoryRepository.GetAll()
                .Include(c => c.Sections)
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    Icon = c.Icon,
                    Sections = c.Sections.OrderBy(s => s.Order).Select(s => new SectionDto
                    {
                        Id = s.Id,
                        Title = s.Title,
                    }).ToList()
                }).ToListAsync();

            return Ok(ApiResponse<List<CategoryDto>>.SuccessResult(categories));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<CategoryDto>.ErrorResult(ex, "An error occurred while retrieving categories"));
        }
    }

    // GET: api/category/{id}
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<DetailedCategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<DetailedCategoryDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCategory(Guid id)
    {
        try
        {
            Category? category = await _categoryRepository.GetAsync
            (
                predicate: c => c.Id == id,
                include: c => c.Include(c => c.Sections).ThenInclude(s => s.Contents)
            );

            if (category == null)
            {
                return NotFound(ApiResponse<DetailedCategoryDto>.ErrorResult(
                    new KeyNotFoundException("Category not found"),
                    "Category not found"));
            }

            DetailedCategoryDto categoryDto = new DetailedCategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                Icon = category.Icon,
                SectionCount = category.Sections.Count,
                TotalContentCount = category.Sections.Sum(s => s.Contents.Count),
                Sections = [..category.Sections.Select(s=> new DetailedSectionDto
                {
                    Id = s.Id,
                    Title = s.Title,
                    Order = s.Order,
                    CategoryId = s.CategoryId,
                    CategoryName = category.Name,
                    ContentCount = s.Contents.Count,
                    CreatedAt = s.CreatedAt,
                    CreatedBy = s.CreatedBy,
                    LastModifiedAt = s.LastModifiedAt,
                    LastModifiedBy = s.LastModifiedBy
                }).OrderBy(s=> s.Order)],

                CreatedAt = category.CreatedAt,
                CreatedBy = category.CreatedBy,
                LastModifiedAt = category.LastModifiedAt,
                LastModifiedBy = category.LastModifiedBy
            };

            return Ok(ApiResponse<DetailedCategoryDto>.SuccessResult(categoryDto));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<DetailedCategoryDto>.ErrorResult(ex, "An error occurred while retrieving the category"));
        }
    }

    // POST: api/category
    [HttpPost]
    [Authorize(Roles = "Admin,Instructor")]
    [ProducesResponseType(typeof(ApiResponse<DetailedCategoryDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<DetailedCategoryDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto createCategoryDto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(createCategoryDto.Name))
            {
                return BadRequest(ApiResponse<DetailedCategoryDto>.ErrorResult(
                    new ArgumentException("Category name is required"),
                    "Invalid category data"));
            }

            Category? existingCategory = await _categoryRepository.GetAsync(c => c.Name.Equals(createCategoryDto.Name, StringComparison.CurrentCultureIgnoreCase));
            if (existingCategory != null)
            {
                return BadRequest(ApiResponse<DetailedCategoryDto>.ErrorResult(
                    new InvalidOperationException("Category name already exists"),
                    "Category name must be unique"));
            }

            Category category = new Category
            {
                Name = createCategoryDto.Name,
                Description = createCategoryDto.Description,
                Icon = createCategoryDto.Icon
            };

            await _categoryRepository.CreateAsync(category);
            await unitOfWork.SaveChangesAsync();

            DetailedCategoryDto result = new DetailedCategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                Icon = category.Icon,
                SectionCount = 0,
                TotalContentCount = 0,
                Sections = [],
                CreatedAt = category.CreatedAt,
                CreatedBy = category.CreatedBy
            };

            return CreatedAtAction(nameof(GetCategory), new { id = category.Id },
                ApiResponse<DetailedCategoryDto>.SuccessResult(result, "Category created successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<DetailedCategoryDto>.ErrorResult(ex, "Category creation failed"));
        }
    }

    // PUT: api/category/{id}
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Instructor")]
    [ProducesResponseType(typeof(ApiResponse<DetailedCategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<DetailedCategoryDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<DetailedCategoryDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] UpdateCategoryDto updateCategoryDto)
    {
        try
        {
            if (id != updateCategoryDto.Id)
            {
                return BadRequest(ApiResponse<DetailedCategoryDto>.ErrorResult(
                    new ArgumentException("ID mismatch"),
                    "Invalid request"));
            }

            Category? category = await _categoryRepository.GetAsync(c => c.Id == id);
            if (category == null)
            {
                return NotFound(ApiResponse<DetailedCategoryDto>.ErrorResult(
                    new KeyNotFoundException("Category not found"),
                    "Category not found"));
            }

            if (!category.Name.Equals(updateCategoryDto.Name, StringComparison.CurrentCultureIgnoreCase))
            {
                Category? existingCategory = await _categoryRepository.GetAsync(c => c.Name.Equals(updateCategoryDto.Name, StringComparison.CurrentCultureIgnoreCase) && c.Id != id);
                if (existingCategory != null)
                {
                    return BadRequest(ApiResponse<DetailedCategoryDto>.ErrorResult(
                        new InvalidOperationException("Category name already exists"),
                        "Category name must be unique"));
                }
            }

            category.Name = updateCategoryDto.Name;
            category.Description = updateCategoryDto.Description;
            category.Icon = updateCategoryDto.Icon;

            await _categoryRepository.UpdateAsync(category);
            await unitOfWork.SaveChangesAsync();

            DetailedCategoryDto result = new DetailedCategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                Icon = category.Icon,
                SectionCount = category.Sections.Count,
                TotalContentCount = category.Sections.Sum(s => s.Contents.Count),
                Sections = [.. category.Sections.Select(s => new DetailedSectionDto
                {
                    Id = s.Id,
                    Title = s.Title,
                    Order = s.Order,
                    CategoryId = s.CategoryId,
                    CategoryName = category.Name,
                    ContentCount = s.Contents.Count,
                    CreatedAt = s.CreatedAt,
                    CreatedBy = s.CreatedBy,
                    LastModifiedAt = s.LastModifiedAt,
                    LastModifiedBy = s.LastModifiedBy
                }).OrderBy(s => s.Order)],

                CreatedAt = category.CreatedAt,
                CreatedBy = category.CreatedBy,
                LastModifiedAt = category.LastModifiedAt,
                LastModifiedBy = category.LastModifiedBy
            };

            return Ok(ApiResponse<DetailedCategoryDto>.SuccessResult(result, "Category updated successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<DetailedCategoryDto>.ErrorResult(ex, "Category update failed"));
        }
    }

    // DELETE: api/category/{id}
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteCategory(Guid id)
    {
        try
        {
            Category? category = await _categoryRepository.GetAsync
            (
                predicate: c => c.Id == id,
                include: c => c.Include(c => c.Sections)
            );

            if (category == null)
            {
                return NotFound(ApiResponse<object>.ErrorResult(
                    new KeyNotFoundException("Category not found"),
                    "Category not found"));
            }

            if (category.Sections.Count != 0)
            {
                return BadRequest(ApiResponse<object>.ErrorResult(
                    new InvalidOperationException("Cannot delete category that contains sections"),
                    "Category is not empty"));
            }

            await _categoryRepository.DeleteAsync(category);
            await unitOfWork.SaveChangesAsync();

            return Ok(ApiResponse<object>.SuccessResult(new object(), "Category deleted successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResult(ex, "Category deletion failed"));
        }
    }
}

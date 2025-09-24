using LearnKazakh.Core.Repositories;
using LearnKazakh.Core.UnitOfWork;
using LearnKazakh.Domain.Entities;
using LearnKazakh.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LearnKazakh.API.Controllers;

[Route("api/content")]
[ApiController]
public class ContentController(IUnitOfWork unitOfWork) : ControllerBase
{
    private const int PageSize = 20;
    private readonly IContentRepository _contentRepository = unitOfWork?.ContentRepository ?? throw new ArgumentNullException(nameof(unitOfWork.ContentRepository));
    private readonly ISectionRepository _sectionRepository = unitOfWork?.SectionRepository ?? throw new ArgumentNullException(nameof(unitOfWork.SectionRepository));

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<DetailedContentDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetContents()
    {
        try
        {
            List<DetailedContentDto> contents = await _contentRepository.GetAll()
                .Include(c => c.Section)
                .ThenInclude(s => s!.Category)
                .AsNoTracking()
                .OrderBy(c => c.SectionId)
                .ThenBy(c => c.Order)
                .Select(c => new DetailedContentDto
                {
                    Id = c.Id,
                    Order = c.Order,
                    ContentText = c.ContentText,
                    ContentMarkdown = c.ContentMarkdown,
                    ContentHtml = c.ContentHtml,
                    SectionId = c.SectionId,
                    SectionTitle = c.Section != null ? c.Section.Title : null,
                    CategoryName = c.Section != null && c.Section.Category != null ? c.Section.Category.Name : null,
                    CreatedAt = c.CreatedAt,
                    CreatedBy = c.CreatedBy,
                    LastModifiedAt = c.LastModifiedAt,
                    LastModifiedBy = c.LastModifiedBy
                }).ToListAsync();

            return Ok(ApiResponse<List<DetailedContentDto>>.SuccessResult(contents));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiPagedResponse<DetailedContentDto>.ErrorResult(ex, "An error occurred while retrieving contents"));
        }
    }

    [HttpGet("section/{sectionId}")]
    [ProducesResponseType(typeof(ApiResponse<SectionContentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SectionContentDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSectionContent(Guid sectionId)
    {
        Section? section = await GetSectionWithRelatedDataAsync(sectionId);
        if (section == null)
        {
            return NotFound(new ApiResponse<SectionContentDto>
            {
                Success = false,
                Message = "Section not found."
            });
        }

        SectionContentDto sectionContentDto = await MapToContentDtoAsync(section);
        return Ok(new ApiResponse<SectionContentDto>
        {
            Success = true,
            Data = sectionContentDto,
        });
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<DetailedContentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<DetailedContentDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetContent(Guid id)
    {
        try
        {
            Content? content = await _contentRepository.GetAsync(c => c.Id == id,
                include: c => c.Include(c => c.Section).ThenInclude(s => s!.Category));

            if (content == null)
            {
                return NotFound(ApiResponse<DetailedContentDto>.ErrorResult(
                    new KeyNotFoundException("Content not found"),
                    "Content not found"));
            }

            DetailedContentDto contentDto = new DetailedContentDto
            {
                Id = content.Id,
                Order = content.Order,
                ContentText = content.ContentText,
                ContentMarkdown = content.ContentMarkdown,
                ContentHtml = content.ContentHtml,
                SectionId = content.SectionId,
                SectionTitle = content.Section?.Title,
                CategoryName = content.Section?.Category?.Name,
                CreatedAt = content.CreatedAt,
                CreatedBy = content.CreatedBy,
                LastModifiedAt = content.LastModifiedAt,
                LastModifiedBy = content.LastModifiedBy
            };

            return Ok(ApiResponse<DetailedContentDto>.SuccessResult(contentDto));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<DetailedContentDto>.ErrorResult(ex, "An error occurred while retrieving the content"));
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Instructor")]
    [ProducesResponseType(typeof(ApiResponse<DetailedContentDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<DetailedContentDto>), StatusCodes.Status400BadRequest)]
    public async Task<ApiResponse<DetailedContentDto>> CreateContent([FromBody] CreateContentDto createContentDto)
    {
        if (createContentDto == null || string.IsNullOrWhiteSpace(createContentDto.ContentText))
        {
            return ApiResponse<DetailedContentDto>.ErrorResult(
                new ArgumentException("Invalid content data provided."),
                "Content creation failed.");
        }

        Section? section = await _sectionRepository.GetAsync(s => s.Id == createContentDto.SectionId,
            include: s => s.Include(s => s.Category));
        if (section == null)
        {
            return ApiResponse<DetailedContentDto>.ErrorResult(
                new KeyNotFoundException("Section not found."),
                "Content creation failed.");
        }

        Content content = new Content
        {
            SectionId = section.Id,
            Order = createContentDto.Order,
            ContentText = createContentDto.ContentText,
            ContentMarkdown = createContentDto.ContentMarkdown,
            ContentHtml = createContentDto.ContentHtml
        };

        await _contentRepository.CreateAsync(content);
        await unitOfWork.SaveChangesAsync();

        DetailedContentDto result = new DetailedContentDto
        {
            Id = content.Id,
            Order = content.Order,
            ContentText = content.ContentText,
            ContentMarkdown = content.ContentMarkdown,
            ContentHtml = content.ContentHtml,
            SectionId = content.SectionId,
            SectionTitle = section.Title,
            CategoryName = section.Category?.Name,
            CreatedAt = content.CreatedAt,
            CreatedBy = content.CreatedBy
        };

        return ApiResponse<DetailedContentDto>.SuccessResult(result, "Content created successfully.");
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Instructor")]
    [ProducesResponseType(typeof(ApiResponse<DetailedContentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<DetailedContentDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<DetailedContentDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateContent(Guid id, [FromBody] UpdateContentDto updateContentDto)
    {
        try
        {
            if (id != updateContentDto.Id)
            {
                return BadRequest(ApiResponse<DetailedContentDto>.ErrorResult(
                    new ArgumentException("ID mismatch"),
                    "Invalid request"));
            }

            Content? content = await _contentRepository.GetAsync(c => c.Id == id,
                include: c => c.Include(c => c.Section).ThenInclude(s => s!.Category));

            if (content == null)
            {
                return NotFound(ApiResponse<DetailedContentDto>.ErrorResult(
                    new KeyNotFoundException("Content not found"),
                    "Content not found"));
            }

            Section? section = content.Section;
            if (content.SectionId != updateContentDto.SectionId)
            {
                section = await _sectionRepository.GetAsync(s => s.Id == updateContentDto.SectionId,
                    include: s => s.Include(s => s.Category));
                if (section == null)
                {
                    return BadRequest(ApiResponse<DetailedContentDto>.ErrorResult(
                        new KeyNotFoundException("Section not found"),
                        "Invalid section"));
                }
            }

            content.Order = updateContentDto.Order;
            content.ContentText = updateContentDto.ContentText;
            content.ContentMarkdown = updateContentDto.ContentMarkdown;
            content.ContentHtml = updateContentDto.ContentHtml;
            content.SectionId = updateContentDto.SectionId;

            await _contentRepository.UpdateAsync(content);
            await unitOfWork.SaveChangesAsync();

            DetailedContentDto result = new DetailedContentDto
            {
                Id = content.Id,
                Order = content.Order,
                ContentText = content.ContentText,
                ContentMarkdown = content.ContentMarkdown,
                ContentHtml = content.ContentHtml,
                SectionId = content.SectionId,
                SectionTitle = section?.Title,
                CategoryName = section?.Category?.Name,
                CreatedAt = content.CreatedAt,
                CreatedBy = content.CreatedBy,
                LastModifiedAt = content.LastModifiedAt,
                LastModifiedBy = content.LastModifiedBy
            };

            return Ok(ApiResponse<DetailedContentDto>.SuccessResult(result, "Content updated successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<DetailedContentDto>.ErrorResult(ex, "Content update failed"));
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Instructor")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteContent(Guid id)
    {
        try
        {
            Content? content = await _contentRepository.GetAsync(c => c.Id == id);
            if (content == null)
            {
                return NotFound(ApiResponse<object>.ErrorResult(
                    new KeyNotFoundException("Content not found"),
                    "Content not found"));
            }

            await _contentRepository.DeleteAsync(content);
            await unitOfWork.SaveChangesAsync();

            return Ok(ApiResponse<object>.SuccessResult(new object(), "Content deleted successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResult(ex, "Content deletion failed"));
        }
    }

    // Private helper methods (existing)
    private async Task<Section?> GetSectionWithRelatedDataAsync(Guid sectionId)
    {
        return await _sectionRepository.GetAsync(
            predicate: s => s.Id == sectionId,
            include: s => s.Include(s => s.Contents).Include(s => s.Category));
    }

    private async Task<SectionContentDto> MapToContentDtoAsync(Section section)
    {
        SectionContentDto sectionContentDto = new SectionContentDto
        {
            Contents = [],
            Category = section.Category?.Name ?? string.Empty,
            Title = section.Title,
        };

        foreach (Content content in section.Contents.OrderBy(c => c.Order))
        {
            sectionContentDto.Contents.Add(new ContentDto
            {
                Id = content.Id,
                ContentText = content.ContentText,
                ContentMarkdown = content.ContentMarkdown,
                ContentHtml = content.ContentHtml,
            });
        }

        await SetNavigationPropertiesAsync(sectionContentDto, section);
        return sectionContentDto;
    }

    private async Task SetNavigationPropertiesAsync(SectionContentDto sectionContentDto, Section currentSection)
    {
        Section? previousSection = await _sectionRepository
            .GetAll(s => s.CategoryId == currentSection.CategoryId && s.Order < currentSection.Order)
            .OrderByDescending(s => s.Order)
            .Include(s => s.Contents)
            .FirstOrDefaultAsync();

        Section? nextSection = await _sectionRepository
            .GetAll(s => s.CategoryId == currentSection.CategoryId && s.Order > currentSection.Order)
            .OrderBy(s => s.Order)
            .Include(s => s.Contents)
            .FirstOrDefaultAsync();

        // presume that if previous section != null, then it has to have at least one content
        if (previousSection != null && previousSection.Contents.Count != 0)
        {
            sectionContentDto.HasPreviousSection = true;
            sectionContentDto.PreviousSectionId = previousSection.Id;
            sectionContentDto.PreviousSectionText = previousSection.Title;
        }

        // but for next section, we need to check if it has contents
        if (nextSection != null && nextSection.Contents.Count != 0)
        {
            sectionContentDto.HasNextSection = true;
            sectionContentDto.NextSectionId = nextSection.Id;
            sectionContentDto.NextSectionText = nextSection.Title;
        }
    }
}
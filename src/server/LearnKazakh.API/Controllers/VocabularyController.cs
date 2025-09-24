using LearnKazakh.Core.Repositories;
using LearnKazakh.Core.UnitOfWork;
using LearnKazakh.Domain.Entities;
using LearnKazakh.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LearnKazakh.API.Controllers;

[Route("api/vocabulary")]
[ApiController]
public class VocabularyController(IUnitOfWork unitOfWork) : ControllerBase
{
    private const int PageSize = 12;
    private readonly IVocabularyRepository _vocabularyRepository = unitOfWork?.VocabularyRepository ??
            throw new ArgumentNullException(nameof(unitOfWork.VocabularyRepository));

    [HttpGet]
    [ProducesResponseType(typeof(ApiPagedResponse<VocabularyDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetVocabulary([FromQuery] int offset = 0)
    {
        offset = Math.Max(0, offset);

        try
        {
            IQueryable<Vocabulary> query = _vocabularyRepository.GetAll()
                .Include(v => v.VocabularyExamples)
                .AsNoTracking();

            int totalCount = await query.CountAsync();
            bool hasMore = totalCount > offset + PageSize;

            List<VocabularyDto> vocabulary = await query
                .OrderBy(v => v.WordKazakh)
                .Skip(offset)
                .Take(PageSize)
                .Select(v => new VocabularyDto
                {
                    Id = v.Id,
                    WordKazakh = v.WordKazakh,
                    WordAzerbaijani = v.WordAzerbaijani,
                    Pronounciation = v.Pronounciation,
                    AudioUrl = v.AudioUrl,
                    Type = v.Type.ToString(),
                    Examples = v.VocabularyExamples.Select(e => new VocabularyExampleDto
                    {
                        Id = e.Id,
                        SentenceKazakh = e.SentenceKazakh,
                        SentenceTranslation = e.SentenceTranslation,
                        AudioUrl = e.AudioUrl
                    }).ToList()
                }).ToListAsync();

            PagedData<VocabularyDto> pagedData = new PagedData<VocabularyDto>
            {
                Items = vocabulary,
                TotalCount = totalCount,
                NextOffset = offset + PageSize,
                HasMore = hasMore
            };

            return Ok(ApiPagedResponse<VocabularyDto>.SuccessResult(pagedData));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiPagedResponse<VocabularyDto>.ErrorResult(ex, "An error occurred while processing your request"));
        }
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<DetailedVocabularyDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<DetailedVocabularyDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetVocabulary(Guid id)
    {
        try
        {
            Vocabulary? vocabulary = await _vocabularyRepository.GetAsync
            (
                predicate: v => v.Id == id,
                include: v => v.Include(v => v.VocabularyExamples)
            );

            if (vocabulary == null)
            {
                return NotFound(ApiResponse<DetailedVocabularyDto>.ErrorResult(
                    new KeyNotFoundException("Vocabulary not found"),
                    "Vocabulary not found"));
            }

            DetailedVocabularyDto vocabularyDto = new DetailedVocabularyDto
            {
                Id = vocabulary.Id,
                WordKazakh = vocabulary.WordKazakh,
                WordAzerbaijani = vocabulary.WordAzerbaijani,
                Pronounciation = vocabulary.Pronounciation,
                AudioUrl = vocabulary.AudioUrl,
                Type = vocabulary.Type,
                Examples = vocabulary.VocabularyExamples.Select(e => new VocabularyExampleDto
                {
                    Id = e.Id,
                    SentenceKazakh = e.SentenceKazakh,
                    SentenceTranslation = e.SentenceTranslation,
                    AudioUrl = e.AudioUrl
                }).ToList(),
                CreatedAt = vocabulary.CreatedAt,
                CreatedBy = vocabulary.CreatedBy,
                LastModifiedAt = vocabulary.LastModifiedAt,
                LastModifiedBy = vocabulary.LastModifiedBy
            };

            return Ok(ApiResponse<DetailedVocabularyDto>.SuccessResult(vocabularyDto));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<DetailedVocabularyDto>.ErrorResult(ex, "An error occurred while retrieving the vocabulary"));
        }
    }

    [HttpGet("stats")]
    [ProducesResponseType(typeof(ApiResponse<VocabularyStatsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetVocabularyStats()
    {
        try
        {
            var stats = await _vocabularyRepository.GetAll()
                .GroupBy(v => v.Type)
                .Select(g => new { Type = g.Key, Count = g.Count() })
                .ToListAsync();

            VocabularyStatsDto result = new VocabularyStatsDto
            {
                TotalWords = stats.Sum(s => s.Count),
                Categories = stats.ToDictionary(s => s.Type.ToString(), s => s.Count)
            };

            return Ok(ApiResponse<VocabularyStatsDto>.SuccessResult(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<VocabularyStatsDto>.ErrorResult(ex, "An error occurred while retrieving vocabulary statistics"));
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Instructor")]
    [ProducesResponseType(typeof(ApiResponse<DetailedVocabularyDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<DetailedVocabularyDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateVocabulary([FromBody] CreateVocabularyDto createVocabularyDto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(createVocabularyDto.WordKazakh) || string.IsNullOrWhiteSpace(createVocabularyDto.WordAzerbaijani))
            {
                return BadRequest(ApiResponse<DetailedVocabularyDto>.ErrorResult(
                    new ArgumentException("Kazakh and Azerbaijani words are required"),
                    "Invalid vocabulary data"));
            }

            Vocabulary? existingVocabulary = await _vocabularyRepository.GetAsync(v => v.WordKazakh.Equals(createVocabularyDto.WordKazakh, StringComparison.CurrentCultureIgnoreCase));
            if (existingVocabulary != null)
            {
                return BadRequest(ApiResponse<DetailedVocabularyDto>.ErrorResult(
                    new InvalidOperationException("Kazakh word already exists"),
                    "Word must be unique"));
            }

            Vocabulary vocabulary = new Vocabulary
            {
                WordKazakh = createVocabularyDto.WordKazakh,
                WordAzerbaijani = createVocabularyDto.WordAzerbaijani,
                Pronounciation = createVocabularyDto.Pronounciation,
                AudioUrl = createVocabularyDto.AudioUrl,
                Type = createVocabularyDto.Type
            };

            await _vocabularyRepository.CreateAsync(vocabulary);
            await unitOfWork.SaveChangesAsync();

            DetailedVocabularyDto result = new DetailedVocabularyDto
            {
                Id = vocabulary.Id,
                WordKazakh = vocabulary.WordKazakh,
                WordAzerbaijani = vocabulary.WordAzerbaijani,
                Pronounciation = vocabulary.Pronounciation,
                AudioUrl = vocabulary.AudioUrl,
                Type = vocabulary.Type,
                Examples = [],
                CreatedAt = vocabulary.CreatedAt,
                CreatedBy = vocabulary.CreatedBy
            };

            return CreatedAtAction(nameof(GetVocabulary), ApiResponse<DetailedVocabularyDto>.SuccessResult(result, "Vocabulary created successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<DetailedVocabularyDto>.ErrorResult(ex, "Vocabulary creation failed"));
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Instructor")]
    [ProducesResponseType(typeof(ApiResponse<DetailedVocabularyDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<DetailedVocabularyDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<DetailedVocabularyDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateVocabulary(Guid id, [FromBody] UpdateVocabularyDto updateVocabularyDto)
    {
        try
        {
            if (id != updateVocabularyDto.Id)
            {
                return BadRequest(ApiResponse<DetailedVocabularyDto>.ErrorResult(
                    new ArgumentException("ID mismatch"),
                    "Invalid request"));
            }

            Vocabulary? vocabulary = await _vocabularyRepository.GetAsync
            (
                predicate: v => v.Id == id,
                include: v => v.Include(v => v.VocabularyExamples)
            );

            if (vocabulary == null)
            {
                return NotFound(ApiResponse<DetailedVocabularyDto>.ErrorResult(
                    new KeyNotFoundException("Vocabulary not found"),
                    "Vocabulary not found"));
            }

            // Check if new Kazakh word already exists (excluding current vocabulary)
            if (!vocabulary.WordKazakh.Equals(updateVocabularyDto.WordKazakh, StringComparison.CurrentCultureIgnoreCase))
            {
                Vocabulary? existingVocabulary = await _vocabularyRepository.GetAsync(v => v.WordKazakh.Equals(updateVocabularyDto.WordKazakh, StringComparison.CurrentCultureIgnoreCase) && v.Id != id);
                if (existingVocabulary != null)
                {
                    return BadRequest(ApiResponse<DetailedVocabularyDto>.ErrorResult(
                        new InvalidOperationException("Kazakh word already exists"),
                        "Word must be unique"));
                }
            }

            vocabulary.WordKazakh = updateVocabularyDto.WordKazakh;
            vocabulary.WordAzerbaijani = updateVocabularyDto.WordAzerbaijani;
            vocabulary.Pronounciation = updateVocabularyDto.Pronounciation;
            vocabulary.AudioUrl = updateVocabularyDto.AudioUrl;
            vocabulary.Type = updateVocabularyDto.Type;

            await _vocabularyRepository.UpdateAsync(vocabulary);
            await unitOfWork.SaveChangesAsync();

            DetailedVocabularyDto result = new DetailedVocabularyDto
            {
                Id = vocabulary.Id,
                WordKazakh = vocabulary.WordKazakh,
                WordAzerbaijani = vocabulary.WordAzerbaijani,
                Pronounciation = vocabulary.Pronounciation,
                AudioUrl = vocabulary.AudioUrl,
                Type = vocabulary.Type,
                Examples = vocabulary.VocabularyExamples.Select(e => new VocabularyExampleDto
                {
                    Id = e.Id,
                    SentenceKazakh = e.SentenceKazakh,
                    SentenceTranslation = e.SentenceTranslation,
                    AudioUrl = e.AudioUrl
                }).ToList(),
                CreatedAt = vocabulary.CreatedAt,
                CreatedBy = vocabulary.CreatedBy,
                LastModifiedAt = vocabulary.LastModifiedAt,
                LastModifiedBy = vocabulary.LastModifiedBy
            };

            return Ok(ApiResponse<DetailedVocabularyDto>.SuccessResult(result, "Vocabulary updated successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<DetailedVocabularyDto>.ErrorResult(ex, "Vocabulary update failed"));
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Instructor")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteVocabulary(Guid id)
    {
        try
        {
            Vocabulary? vocabulary = await _vocabularyRepository.GetAsync(v => v.Id == id);
            if (vocabulary == null)
            {
                return NotFound(ApiResponse<object>.ErrorResult(
                    new KeyNotFoundException("Vocabulary not found"),
                    "Vocabulary not found"));
            }

            await _vocabularyRepository.DeleteAsync(vocabulary);
            await unitOfWork.SaveChangesAsync();

            return Ok(ApiResponse<object>.SuccessResult(new object(), "Vocabulary deleted successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResult(ex, "Vocabulary deletion failed"));
        }
    }
}
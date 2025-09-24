using LearnKazakh.Core.Repositories;
using LearnKazakh.Core.UnitOfWork;
using LearnKazakh.Domain.Entities;
using LearnKazakh.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LearnKazakh.API.Controllers;

[Route("api/vocabulary-example")]
[ApiController]
public class VocabularyExampleController(IUnitOfWork unitOfWork) : ControllerBase
{
    private readonly IVocabularyExampleRepository _vocabularyExampleRepository = unitOfWork?.VocabularyExampleRepository ??
            throw new ArgumentNullException(nameof(unitOfWork.VocabularyExampleRepository));

    private readonly IVocabularyRepository _vocabularyRepository = unitOfWork?.VocabularyRepository ??
            throw new ArgumentNullException(nameof(unitOfWork.VocabularyRepository));

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<DetailedVocabularyExampleDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetVocabularyExamples()
    {
        try
        {
            List<DetailedVocabularyExampleDto> examples = await _vocabularyExampleRepository.GetAll()
                .Include(ve => ve.Vocabulary)
                .AsNoTracking()
                .OrderBy(ve => ve.SentenceKazakh)
                .Select(ve => new DetailedVocabularyExampleDto
                {
                    Id = ve.Id,
                    SentenceKazakh = ve.SentenceKazakh,
                    SentenceTranslation = ve.SentenceTranslation,
                    AudioUrl = ve.AudioUrl,
                    CreatedAt = ve.CreatedAt,
                    CreatedBy = ve.CreatedBy,
                    LastModifiedAt = ve.LastModifiedAt,
                    LastModifiedBy = ve.LastModifiedBy
                }).ToListAsync();

            return Ok(ApiResponse<List<DetailedVocabularyExampleDto>>.SuccessResult(examples));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiPagedResponse<DetailedVocabularyExampleDto>.ErrorResult(ex, "An error occurred while retrieving vocabulary examples"));
        }
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<DetailedVocabularyExampleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<DetailedVocabularyExampleDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetVocabularyExample(Guid id)
    {
        try
        {
            VocabularyExample? example = await _vocabularyExampleRepository.GetAsync(ve => ve.Id == id);
            if (example == null)
            {
                return NotFound(ApiResponse<DetailedVocabularyExampleDto>.ErrorResult(
                    new KeyNotFoundException("Vocabulary example not found"),
                    "Vocabulary example not found"));
            }

            DetailedVocabularyExampleDto exampleDto = new DetailedVocabularyExampleDto
            {
                Id = example.Id,
                SentenceKazakh = example.SentenceKazakh,
                SentenceTranslation = example.SentenceTranslation,
                AudioUrl = example.AudioUrl,
                CreatedAt = example.CreatedAt,
                CreatedBy = example.CreatedBy,
                LastModifiedAt = example.LastModifiedAt,
                LastModifiedBy = example.LastModifiedBy
            };

            return Ok(ApiResponse<DetailedVocabularyExampleDto>.SuccessResult(exampleDto));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<DetailedVocabularyExampleDto>.ErrorResult(ex, "An error occurred while retrieving the vocabulary example"));
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Instructor")]
    [ProducesResponseType(typeof(ApiResponse<DetailedVocabularyExampleDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<DetailedVocabularyExampleDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateVocabularyExample([FromBody] CreateVocabularyExampleDto createExampleDto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(createExampleDto.SentenceKazakh)
                || string.IsNullOrWhiteSpace(createExampleDto.SentenceTranslation))
            {
                return BadRequest(ApiResponse<DetailedVocabularyExampleDto>.ErrorResult(
                    new ArgumentException("Kazakh sentence and translation are required"),
                    "Invalid example data"));
            }

            Vocabulary? vocabulary = null;
            if (createExampleDto.VocabularyId.HasValue)
            {
                vocabulary = await _vocabularyRepository.GetAsync(v => v.Id == createExampleDto.VocabularyId.Value);
                if (vocabulary == null)
                {
                    return BadRequest(ApiResponse<DetailedVocabularyExampleDto>.ErrorResult(
                        new KeyNotFoundException("Vocabulary not found"),
                        "Invalid vocabulary"));
                }
            }

            VocabularyExample example = new VocabularyExample
            {
                SentenceKazakh = createExampleDto.SentenceKazakh,
                SentenceTranslation = createExampleDto.SentenceTranslation,
                AudioUrl = createExampleDto.AudioUrl,
                VocabularyId = createExampleDto.VocabularyId
            };

            await _vocabularyExampleRepository.CreateAsync(example);
            await unitOfWork.SaveChangesAsync();

            DetailedVocabularyExampleDto result = new DetailedVocabularyExampleDto
            {
                Id = example.Id,
                SentenceKazakh = example.SentenceKazakh,
                SentenceTranslation = example.SentenceTranslation,
                AudioUrl = example.AudioUrl,
                CreatedAt = example.CreatedAt,
                CreatedBy = example.CreatedBy
            };

            return CreatedAtAction(nameof(GetVocabularyExample), new { id = example.Id },
                ApiResponse<DetailedVocabularyExampleDto>.SuccessResult(result, "Vocabulary example created successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<DetailedVocabularyExampleDto>.ErrorResult(ex, "Vocabulary example creation failed"));
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Instructor")]
    [ProducesResponseType(typeof(ApiResponse<DetailedVocabularyExampleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<DetailedVocabularyExampleDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<DetailedVocabularyExampleDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateVocabularyExample(Guid id, [FromBody] UpdateVocabularyExampleDto updateExampleDto)
    {
        try
        {
            if (id != updateExampleDto.Id)
            {
                return BadRequest(ApiResponse<DetailedVocabularyExampleDto>.ErrorResult(
                    new ArgumentException("ID mismatch"),
                    "Invalid request"));
            }

            VocabularyExample? example = await _vocabularyExampleRepository.GetAsync(ve => ve.Id == id);
            if (example == null)
            {
                return NotFound(ApiResponse<DetailedVocabularyExampleDto>.ErrorResult(
                    new KeyNotFoundException("Vocabulary example not found"),
                    "Vocabulary example not found"));
            }

            Vocabulary? vocabulary = null;
            if (updateExampleDto.VocabularyId.HasValue && updateExampleDto.VocabularyId != example.VocabularyId)
            {
                vocabulary = await _vocabularyRepository.GetAsync(v => v.Id == updateExampleDto.VocabularyId.Value);
                if (vocabulary == null)
                {
                    return BadRequest(ApiResponse<DetailedVocabularyExampleDto>.ErrorResult(
                        new KeyNotFoundException("Vocabulary not found"),
                        "Invalid vocabulary"));
                }
            }
            else
            {
                vocabulary = example.Vocabulary;
            }

            example.SentenceKazakh = updateExampleDto.SentenceKazakh;
            example.SentenceTranslation = updateExampleDto.SentenceTranslation;
            example.AudioUrl = updateExampleDto.AudioUrl;
            example.VocabularyId = updateExampleDto.VocabularyId;

            await _vocabularyExampleRepository.UpdateAsync(example);
            await unitOfWork.SaveChangesAsync();

            DetailedVocabularyExampleDto result = new DetailedVocabularyExampleDto
            {
                Id = example.Id,
                SentenceKazakh = example.SentenceKazakh,
                SentenceTranslation = example.SentenceTranslation,
                AudioUrl = example.AudioUrl,
                CreatedAt = example.CreatedAt,
                CreatedBy = example.CreatedBy,
                LastModifiedAt = example.LastModifiedAt,
                LastModifiedBy = example.LastModifiedBy
            };

            return Ok(ApiResponse<DetailedVocabularyExampleDto>.SuccessResult(result, "Vocabulary example updated successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<DetailedVocabularyExampleDto>.ErrorResult(ex, "Vocabulary example update failed"));
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Instructor")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteVocabularyExample(Guid id)
    {
        try
        {
            VocabularyExample? example = await _vocabularyExampleRepository.GetAsync(ve => ve.Id == id);
            if (example == null)
            {
                return NotFound(ApiResponse<object>.ErrorResult(
                    new KeyNotFoundException("Vocabulary example not found"),
                    "Vocabulary example not found"));
            }

            await _vocabularyExampleRepository.DeleteAsync(example);
            await unitOfWork.SaveChangesAsync();

            return Ok(ApiResponse<object>.SuccessResult(new object(), "Vocabulary example deleted successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResult(ex, "Vocabulary example deletion failed"));
        }
    }
}
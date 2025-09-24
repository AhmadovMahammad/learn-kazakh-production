using LearnKazakh.Core.Repositories;
using LearnKazakh.Core.UnitOfWork;
using LearnKazakh.Domain.Entities;
using LearnKazakh.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LearnKazakh.API.Controllers;

[Route("api/role")]
[ApiController]
public class RoleController(IUnitOfWork unitOfWork) : ControllerBase
{
    private const int PageSize = 50;
    private readonly IRoleRepository _roleRepository = unitOfWork?.RoleRepository ??
            throw new ArgumentNullException(nameof(unitOfWork.RoleRepository));

    [HttpGet]
    [ProducesResponseType(typeof(ApiPagedResponse<RoleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRoles([FromQuery] int offset = 0)
    {
        offset = Math.Max(0, offset);

        try
        {
            IQueryable<Role> query = _roleRepository.GetAll().AsNoTracking();

            int totalCount = await query.CountAsync();
            bool hasMore = totalCount > offset + PageSize;

            List<RoleDto> roles = await query
                .OrderBy(r => r.Name)
                .Skip(offset)
                .Take(PageSize)
                .Select(r => new RoleDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    Description = r.Description,
                    CreatedAt = r.CreatedAt,
                    CreatedBy = r.CreatedBy,
                    LastModifiedAt = r.LastModifiedAt,
                    LastModifiedBy = r.LastModifiedBy
                }).ToListAsync();

            PagedData<RoleDto> pagedData = new PagedData<RoleDto>
            {
                Items = roles,
                TotalCount = totalCount,
                NextOffset = offset + PageSize,
                HasMore = hasMore
            };

            return Ok(ApiPagedResponse<RoleDto>.SuccessResult(pagedData));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiPagedResponse<RoleDto>.ErrorResult(ex, "An error occurred while retrieving roles"));
        }
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<RoleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<RoleDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRole(Guid id)
    {
        try
        {
            Role? role = await _roleRepository.GetAsync(r => r.Id == id);

            if (role == null)
            {
                return NotFound(ApiResponse<RoleDto>.ErrorResult(
                    new KeyNotFoundException("Role not found"),
                    "Role not found"));
            }

            RoleDto roleDto = new RoleDto
            {
                Id = role.Id,
                Name = role.Name,
                Description = role.Description,
                CreatedAt = role.CreatedAt,
                CreatedBy = role.CreatedBy,
                LastModifiedAt = role.LastModifiedAt,
                LastModifiedBy = role.LastModifiedBy
            };

            return Ok(ApiResponse<RoleDto>.SuccessResult(roleDto));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<RoleDto>.ErrorResult(ex, "An error occurred while retrieving the role"));
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<RoleDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<RoleDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleDto createRoleDto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(createRoleDto.Name))
            {
                return BadRequest(ApiResponse<RoleDto>.ErrorResult(
                    new ArgumentException("Role name is required"),
                    "Invalid role data"));
            }

            Role? existingRole = await _roleRepository.GetAsync(r => r.Name.Equals(createRoleDto.Name, StringComparison.CurrentCultureIgnoreCase));
            if (existingRole != null)
            {
                return BadRequest(ApiResponse<RoleDto>.ErrorResult(
                    new InvalidOperationException("Role name already exists"),
                    "Role name must be unique"));
            }

            Role role = new Role
            {
                Name = createRoleDto.Name,
                Description = createRoleDto.Description
            };

            await _roleRepository.CreateAsync(role);
            await unitOfWork.SaveChangesAsync();

            RoleDto result = new RoleDto
            {
                Id = role.Id,
                Name = role.Name,
                Description = role.Description,
                CreatedAt = role.CreatedAt,
                CreatedBy = role.CreatedBy
            };

            return CreatedAtAction(nameof(GetRole), ApiResponse<RoleDto>.SuccessResult(result, "Role created successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<RoleDto>.ErrorResult(ex, "Role creation failed"));
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<RoleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<RoleDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<RoleDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateRole(Guid id, [FromBody] UpdateRoleDto updateRoleDto)
    {
        try
        {
            if (id != updateRoleDto.Id)
            {
                return BadRequest(ApiResponse<RoleDto>.ErrorResult(
                    new ArgumentException("ID mismatch"),
                    "Invalid request"));
            }

            Role? role = await _roleRepository.GetAsync(r => r.Id == id);
            if (role == null)
            {
                return NotFound(ApiResponse<RoleDto>.ErrorResult(
                    new KeyNotFoundException("Role not found"),
                    "Role not found"));
            }

            Role? existingRole = await _roleRepository.GetAsync(r => r.Name.Equals(updateRoleDto.Name, StringComparison.CurrentCultureIgnoreCase) && r.Id != id);
            if (existingRole != null)
            {
                return BadRequest(ApiResponse<RoleDto>.ErrorResult(
                    new InvalidOperationException("Role name already exists"),
                    "Role name must be unique"));
            }

            role.Name = updateRoleDto.Name;
            role.Description = updateRoleDto.Description;

            await _roleRepository.UpdateAsync(role);
            await unitOfWork.SaveChangesAsync();

            RoleDto result = new RoleDto
            {
                Id = role.Id,
                Name = role.Name,
                Description = role.Description,
                CreatedAt = role.CreatedAt,
                CreatedBy = role.CreatedBy,
                LastModifiedAt = role.LastModifiedAt,
                LastModifiedBy = role.LastModifiedBy
            };

            return Ok(ApiResponse<RoleDto>.SuccessResult(result, "Role updated successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<RoleDto>.ErrorResult(ex, "Role update failed"));
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteRole(Guid id)
    {
        try
        {
            Role? role = await _roleRepository.GetAsync
            (
                predicate: r => r.Id == id,
                include: r => r.Include(r => r.UserRoles)
            );

            if (role == null)
            {
                return NotFound(ApiResponse<object>.ErrorResult(
                    new KeyNotFoundException("Role not found"),
                    "Role not found"));
            }

            if (role.UserRoles.Count != 0)
            {
                return BadRequest(ApiResponse<object>.ErrorResult(
                    new InvalidOperationException("Cannot delete role that is assigned to users"),
                    "Role is in use"));
            }

            await _roleRepository.DeleteAsync(role);
            await unitOfWork.SaveChangesAsync();

            return Ok(ApiResponse<object>.SuccessResult(new object(), "Role deleted successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResult(ex, "Role deletion failed"));
        }
    }
}
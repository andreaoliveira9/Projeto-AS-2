/*
 * Copyright (c) .NET Foundation and Contributors
 *
 * This software may be modified and distributed under the terms
 * of the MIT license. See the LICENSE file for details.
 *
 * https://github.com/piranhacms/piranha.core
 *
 */

#nullable enable

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Piranha.Audit.Models;
using Piranha.Audit.Services;

namespace Piranha.Audit.Controllers;

/// <summary>
/// API Controller for audit functionality.
/// Provides endpoints to query audit records and state change history.
/// </summary>
[ApiController]
[Route("api/audit")]
[Authorize]
public class AuditController : ControllerBase
{
    private readonly IAuditService _auditService;
    private readonly ILogger<AuditController> _logger;

    /// <summary>
    /// Default constructor.
    /// </summary>
    /// <param name="auditService">The audit service</param>
    /// <param name="logger">The logger</param>
    public AuditController(IAuditService auditService, ILogger<AuditController> logger)
    {
        _auditService = auditService;
        _logger = logger;
    }

    /// <summary>
    /// Gets the state change history for a specific content.
    /// </summary>
    /// <param name="contentId">The content ID</param>
    /// <returns>List of state change records ordered by timestamp (newest first)</returns>
    [HttpGet("content/{contentId:guid}/history")]
    [ProducesResponseType(typeof(IEnumerable<StateChangeRecordDto>), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<IEnumerable<StateChangeRecordDto>>> GetContentHistory(Guid contentId)
    {
        try
        {
            if (contentId == Guid.Empty)
            {
                return BadRequest("Content ID cannot be empty");
            }

            _logger.LogInformation("Retrieving audit history for content {ContentId}", contentId);

            var records = await _auditService.GetStateChangeHistoryAsync(contentId);
            var dtos = records
                .OrderByDescending(r => r.Timestamp)
                .Select(MapToDto)
                .ToList();

            if (!dtos.Any())
            {
                _logger.LogInformation("No audit records found for content {ContentId}", contentId);
                return NotFound($"No audit history found for content {contentId}");
            }

            _logger.LogInformation("Retrieved {Count} audit records for content {ContentId}", dtos.Count, contentId);
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit history for content {ContentId}", contentId);
            return StatusCode(500, "An error occurred while retrieving audit history");
        }
    }

    /// <summary>
    /// Gets a summary of audit information for a specific content.
    /// </summary>
    /// <param name="contentId">The content ID</param>
    /// <returns>Audit summary information</returns>
    [HttpGet("content/{contentId:guid}/summary")]
    [ProducesResponseType(typeof(AuditSummaryDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<AuditSummaryDto>> GetContentSummary(Guid contentId)
    {
        try
        {
            if (contentId == Guid.Empty)
            {
                return BadRequest("Content ID cannot be empty");
            }

            _logger.LogInformation("Retrieving audit summary for content {ContentId}", contentId);

            var records = await _auditService.GetStateChangeHistoryAsync(contentId);
            var recordsList = records.OrderByDescending(r => r.Timestamp).ToList();

            if (!recordsList.Any())
            {
                _logger.LogInformation("No audit records found for content {ContentId}", contentId);
                return NotFound($"No audit history found for content {contentId}");
            }

            var summary = new AuditSummaryDto
            {
                ContentId = contentId,
                TotalChanges = recordsList.Count,
                LastChange = recordsList.FirstOrDefault()?.Timestamp,
                LastChangedBy = recordsList.FirstOrDefault()?.approvedBy,
                CurrentState = recordsList.FirstOrDefault()?.ToState,
                SuccessfulChanges = recordsList.Count(r => r.Success),
                FailedChanges = recordsList.Count(r => !r.Success)
            };

            _logger.LogInformation("Generated audit summary for content {ContentId}: {TotalChanges} total changes", 
                contentId, summary.TotalChanges);
            
            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit summary for content {ContentId}", contentId);
            return StatusCode(500, "An error occurred while retrieving audit summary");
        }
    }

    /// <summary>
    /// Maps a StateChangeRecord to StateChangeRecordDto.
    /// </summary>
    /// <param name="record">The state change record</param>
    /// <returns>The DTO representation</returns>
    private static StateChangeRecordDto MapToDto(StateChangeRecord record)
    {
        return new StateChangeRecordDto
        {
            Id = record.Id,
            ContentId = record.ContentId,
            ContentName = record.ContentName,
            FromState = record.FromState,
            ToState = record.ToState,
            transitionDescription = record.transitionDescription,
            Username = record.approvedBy,
            Timestamp = record.Timestamp,
            Comments = record.Comments,
            Success = record.Success,
            ErrorMessage = record.ErrorMessage
        };
    }
}

/// <summary>
/// DTO for state change record data.
/// </summary>
public class StateChangeRecordDto
{
    /// <summary>
    /// Gets/sets the record ID.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets/sets the content ID.
    /// </summary>
    public Guid ContentId { get; set; }

    /// <summary>
    /// Gets/sets the content type (e.g., "Page", "Post").
    /// </summary>
    public string ContentName { get; set; } = string.Empty;

    /// <summary>
    /// Gets/sets the previous state.
    /// </summary>
    public string FromState { get; set; } = string.Empty;

    /// <summary>
    /// Gets/sets the new state.
    /// </summary>
    public string ToState { get; set; } = string.Empty;

    /// <summary>
    /// Gets/sets the transition rule id that triggered this change.
    /// </summary>
    public string transitionDescription { get; set; } = string.Empty;

    /// <summary>
    /// Gets/sets the username for quick reference.
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Gets/sets when the state change occurred.
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Gets/sets optional comments.
    /// </summary>
    public string? Comments { get; set; }

    /// <summary>
    /// Gets/sets whether the action was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets/sets the error message if the action failed.
    /// </summary>
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// DTO for audit summary information.
/// </summary>
public class AuditSummaryDto
{
    /// <summary>
    /// Gets/sets the content ID.
    /// </summary>
    public Guid ContentId { get; set; }

    /// <summary>
    /// Gets/sets the total number of changes.
    /// </summary>
    public int TotalChanges { get; set; }

    /// <summary>
    /// Gets/sets the last change timestamp.
    /// </summary>
    public DateTime? LastChange { get; set; }

    /// <summary>
    /// Gets/sets who made the last change.
    /// </summary>
    public string? LastChangedBy { get; set; }

    /// <summary>
    /// Gets/sets the current state.
    /// </summary>
    public string? CurrentState { get; set; }

    /// <summary>
    /// Gets/sets the number of successful changes.
    /// </summary>
    public int SuccessfulChanges { get; set; }

    /// <summary>
    /// Gets/sets the number of failed changes.
    /// </summary>
    public int FailedChanges { get; set; }
}

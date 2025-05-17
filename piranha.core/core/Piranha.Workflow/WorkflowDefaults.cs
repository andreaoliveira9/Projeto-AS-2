/*
 * Copyright (c) .NET Foundation and Contributors
 *
 * This software may be modified and distributed under the terms
 * of the MIT license. See the LICENSE file for details.
 *
 */

using System;
using System.Collections.Generic;
using Piranha.Workflow.Models;

namespace Piranha.Workflow
{
    /// <summary>
    /// Contains default workflow definitions that can be used as starting points.
    /// </summary>
    public static class WorkflowDefaults
    {
        /// <summary>
        /// Creates a standard editorial workflow with Draft, Review, Approved, and Published states.
        /// </summary>
        /// <returns>A standard editorial workflow definition</returns>
        public static WorkflowDefinition CreateStandardEditorialWorkflow()
        {
            var draftState = new WorkflowState
            {
                Id = Guid.NewGuid(),
                Name = "Draft",
                Description = "Content is being drafted and is not ready for review.",
                IsInitial = true,
                IsTerminal = false,
                SortOrder = 1,
                Color = "#6c757d" // Gray
            };

            var reviewState = new WorkflowState
            {
                Id = Guid.NewGuid(),
                Name = "In Review",
                Description = "Content is ready for review by editors.",
                IsInitial = false,
                IsTerminal = false,
                SortOrder = 2,
                Color = "#ffc107" // Yellow
            };

            var approvedState = new WorkflowState
            {
                Id = Guid.NewGuid(),
                Name = "Approved",
                Description = "Content has been approved but is not yet published.",
                IsInitial = false, 
                IsTerminal = false,
                SortOrder = 3,
                Color = "#28a745" // Green
            };

            var publishedState = new WorkflowState
            {
                Id = Guid.NewGuid(),
                Name = "Published",
                Description = "Content is published and publicly available.",
                IsInitial = false,
                IsTerminal = true,
                SortOrder = 4,
                Color = "#007bff" // Blue
            };

            var transitions = new List<TransitionRule>
            {
                // From Draft to In Review
                new TransitionRule
                {
                    Id = Guid.NewGuid(),
                    Name = "Submit for Review",
                    Description = "Submit the draft content for editorial review.",
                    FromStateId = draftState.Id,
                    ToStateId = reviewState.Id,
                    AllowedRoles = new List<string> { "Author", "Editor", "Administrator" },
                    RequiresValidation = false,
                    SortOrder = 1
                },
                
                // From In Review back to Draft
                new TransitionRule
                {
                    Id = Guid.NewGuid(),
                    Name = "Request Changes",
                    Description = "Return the content to draft status for revisions.",
                    FromStateId = reviewState.Id,
                    ToStateId = draftState.Id,
                    AllowedRoles = new List<string> { "Editor", "Administrator" },
                    RequiresValidation = false,
                    SortOrder = 1
                },
                
                // From In Review to Approved
                new TransitionRule
                {
                    Id = Guid.NewGuid(),
                    Name = "Approve",
                    Description = "Approve the content for publication.",
                    FromStateId = reviewState.Id,
                    ToStateId = approvedState.Id,
                    AllowedRoles = new List<string> { "Editor", "Administrator" },
                    RequiresValidation = false,
                    SortOrder = 2
                },
                
                // From Approved to Published
                new TransitionRule
                {
                    Id = Guid.NewGuid(),
                    Name = "Publish",
                    Description = "Publish the approved content.",
                    FromStateId = approvedState.Id,
                    ToStateId = publishedState.Id,
                    AllowedRoles = new List<string> { "Editor", "Administrator" },
                    RequiresValidation = false,
                    SortOrder = 1
                },
                
                // From Approved back to Draft
                new TransitionRule
                {
                    Id = Guid.NewGuid(),
                    Name = "Revert to Draft",
                    Description = "Return the content to draft status for major revisions.",
                    FromStateId = approvedState.Id,
                    ToStateId = draftState.Id,
                    AllowedRoles = new List<string> { "Editor", "Administrator" },
                    RequiresValidation = false,
                    SortOrder = 2
                },
                
                // From Published back to Draft
                new TransitionRule
                {
                    Id = Guid.NewGuid(),
                    Name = "Unpublish and Edit",
                    Description = "Unpublish the content and return to draft status for editing.",
                    FromStateId = publishedState.Id,
                    ToStateId = draftState.Id,
                    AllowedRoles = new List<string> { "Editor", "Administrator" },
                    RequiresValidation = false,
                    SortOrder = 1
                }
            };

            var workflow = new WorkflowDefinition
            {
                Id = Guid.NewGuid(),
                Name = "Standard Editorial Workflow",
                Description = "A standard editorial workflow with draft, review, approval, and publication stages.",
                States = new List<WorkflowState> { draftState, reviewState, approvedState, publishedState },
                TransitionRules = transitions,
                ContentTypes = new List<string> { "Page", "Post" },
                IsActive = true,
                Created = DateTime.Now,
                LastModified = DateTime.Now
            };

            return workflow;
        }

        /// <summary>
        /// Creates a simple workflow with just Draft and Published states.
        /// </summary>
        /// <returns>A simple workflow definition</returns>
        public static WorkflowDefinition CreateSimpleWorkflow()
        {
            var draftState = new WorkflowState
            {
                Id = Guid.NewGuid(),
                Name = "Draft",
                Description = "Content is being drafted and is not ready for publication.",
                IsInitial = true,
                IsTerminal = false,
                SortOrder = 1,
                Color = "#6c757d" // Gray
            };

            var publishedState = new WorkflowState
            {
                Id = Guid.NewGuid(),
                Name = "Published",
                Description = "Content is published and publicly available.",
                IsInitial = false,
                IsTerminal = true,
                SortOrder = 2,
                Color = "#007bff" // Blue
            };

            var transitions = new List<TransitionRule>
            {
                // From Draft to Published
                new TransitionRule
                {
                    Id = Guid.NewGuid(),
                    Name = "Publish",
                    Description = "Publish the draft content.",
                    FromStateId = draftState.Id,
                    ToStateId = publishedState.Id,
                    AllowedRoles = new List<string> { "Author", "Editor", "Administrator" },
                    RequiresValidation = false,
                    SortOrder = 1
                },
                
                // From Published back to Draft
                new TransitionRule
                {
                    Id = Guid.NewGuid(),
                    Name = "Unpublish",
                    Description = "Unpublish the content and return to draft status.",
                    FromStateId = publishedState.Id,
                    ToStateId = draftState.Id,
                    AllowedRoles = new List<string> { "Author", "Editor", "Administrator" },
                    RequiresValidation = false,
                    SortOrder = 1
                }
            };

            var workflow = new WorkflowDefinition
            {
                Id = Guid.NewGuid(),
                Name = "Simple Workflow",
                Description = "A simple workflow with just draft and published states.",
                States = new List<WorkflowState> { draftState, publishedState },
                TransitionRules = transitions,
                ContentTypes = new List<string> { "Page", "Post" },
                IsActive = true,
                Created = DateTime.Now,
                LastModified = DateTime.Now
            };

            return workflow;
        }
    }
} 
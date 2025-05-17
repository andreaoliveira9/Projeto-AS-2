/*
 * Copyright (c) .NET Foundation and Contributors
 *
 * This software may be modified and distributed under the terms
 * of the MIT license. See the LICENSE file for details.
 *
 */

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Piranha.Workflow.Models;
using Piranha.Workflow.Services;

namespace Piranha.Workflow
{
    /// <summary>
    /// Helper class to demonstrate workflow functionality.
    /// </summary>
    public class TestWorkflow
    {
        /// <summary>
        /// Run a test of the workflow functionality.
        /// </summary>
        public static async Task RunWorkflowDemoAsync()
        {
            // Set up DI
            var services = new ServiceCollection();
            services.AddPiranhaWorkflow();
            var serviceProvider = services.BuildServiceProvider();

            // Get the workflow service
            var workflowService = serviceProvider.GetRequiredService<IWorkflowService>();

            Console.WriteLine("=== Workflow Demo ===");
            Console.WriteLine();

            // Create a standard editorial workflow
            var workflowDef = WorkflowDefaults.CreateStandardEditorialWorkflow();
            await workflowService.SaveWorkflowDefinitionAsync(workflowDef);
            Console.WriteLine($"Created workflow: {workflowDef.Name}");
            Console.WriteLine($"Workflow ID: {workflowDef.Id}");
            Console.WriteLine("States:");
            foreach (var state in workflowDef.States)
            {
                Console.WriteLine($" - {state.Name} (Initial: {state.IsInitial}, Terminal: {state.IsTerminal})");
            }
            Console.WriteLine();

            // Create a mock content item (in a real scenario, this would be a page or post)
            var contentId = Guid.NewGuid();
            var contentType = "Page";
            Console.WriteLine($"Created mock content item ID: {contentId}");

            // Create a workflow instance for the content
            var instance = await workflowService.CreateWorkflowInstanceAsync(contentId, contentType, workflowDef.Id);
            Console.WriteLine($"Created workflow instance ID: {instance.Id}");
            
            var initialState = workflowDef.GetState(instance.CurrentStateId);
            if (initialState != null)
            {
                Console.WriteLine($"Initial state: {initialState.Name}");
            }
            Console.WriteLine();

            // Get the workflow extension for the content
            var extension = await workflowService.GetContentExtensionAsync(contentId);
            Console.WriteLine($"Content extension - Current state: {extension.CurrentStateName}");
            Console.WriteLine();

            // Get available transitions
            var transitions = await workflowService.GetAvailableTransitionsAsync(contentId);
            Console.WriteLine("Available transitions:");
            foreach (var transition in transitions)
            {
                var fromState = workflowDef.GetState(transition.FromStateId);
                var toState = workflowDef.GetState(transition.ToStateId);
                
                if (fromState != null && toState != null)
                {
                    Console.WriteLine($" - {transition.Name}: {fromState.Name} -> {toState.Name}");
                }
            }
            Console.WriteLine();

            // Perform a transition
            var transitionToUse = transitions.GetEnumerator();
            transitionToUse.MoveNext();
            var userId = Guid.NewGuid();
            var username = "test.user@example.com";
            
            instance = await workflowService.PerformTransitionAsync(
                contentId, 
                transitionToUse.Current.Id, 
                userId, 
                username, 
                "Submitting for review"
            );
            
            Console.WriteLine($"Performed transition: {transitionToUse.Current.Name}");
            
            var newState = workflowDef.GetState(instance.CurrentStateId);
            if (newState != null)
            {
                Console.WriteLine($"New state: {newState.Name}");
            }
            Console.WriteLine();

            // Get the updated content extension
            extension = await workflowService.GetContentExtensionAsync(contentId);
            Console.WriteLine($"Updated content extension - Current state: {extension.CurrentStateName}");
            Console.WriteLine($"Last updated by: {extension.LastUpdatedByUsername}");
            Console.WriteLine();

            // Check the instance history
            Console.WriteLine("Workflow history:");
            foreach (var record in instance.History)
            {
                string fromStateName = "Initial";
                if (record.FromStateId != Guid.Empty)
                {
                    var fromState = workflowDef.GetState(record.FromStateId);
                    if (fromState != null)
                    {
                        fromStateName = fromState.Name;
                    }
                }
                
                string toStateName = "Unknown";
                var toState = workflowDef.GetState(record.ToStateId);
                if (toState != null)
                {
                    toStateName = toState.Name;
                }
                
                Console.WriteLine($" - {record.Timestamp.ToString("g")}: {fromStateName} -> {toStateName}");
                if (!string.IsNullOrEmpty(record.Comment))
                {
                    Console.WriteLine($"   Comment: {record.Comment}");
                }
            }
            Console.WriteLine();

            // Get new available transitions
            transitions = await workflowService.GetAvailableTransitionsAsync(contentId);
            Console.WriteLine("Available transitions from current state:");
            foreach (var transition in transitions)
            {
                var fromState = workflowDef.GetState(transition.FromStateId);
                var toState = workflowDef.GetState(transition.ToStateId);
                
                if (fromState != null && toState != null)
                {
                    Console.WriteLine($" - {transition.Name}: {fromState.Name} -> {toState.Name}");
                }
            }
            Console.WriteLine();

            Console.WriteLine("=== End of Demo ===");
        }
    }
} 
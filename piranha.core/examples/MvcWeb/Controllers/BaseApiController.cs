using Microsoft.AspNetCore.Mvc;

namespace MvcWeb.Controllers;

public abstract class BaseApiController : ControllerBase
{
    private readonly IWebHostEnvironment _environment;

    protected BaseApiController(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    /// <summary>
    /// Helper method to check if anonymous access is allowed for testing
    /// </summary>
    protected bool AllowAnonymousForTesting()
    {
        return _environment.IsDevelopment() || 
               _environment.EnvironmentName.Equals("Testing", StringComparison.OrdinalIgnoreCase);
    }
    
    /// <summary>
    /// Helper method to check if user has workflow permissions
    /// </summary>
    protected bool HasWorkflowPermission()
    {
        // In development/testing, allow anonymous access
        if (AllowAnonymousForTesting())
            return true;
            
        // Check if user is authenticated and has either Admin or Workflows permission
        return User.Identity?.IsAuthenticated == true && 
               (User.HasClaim("Permission", "PiranhaAdmin") || 
                User.HasClaim("Permission", "PiranhaWorkflows"));
    }

    /// <summary>
    /// Helper method to check if user has content permissions
    /// </summary>
    protected bool HasContentPermission()
    {
        // In development/testing, allow anonymous access
        if (AllowAnonymousForTesting())
            return true;
            
        // Check if user is authenticated and has content permissions
        return User.Identity?.IsAuthenticated == true && 
               (User.HasClaim("Permission", "PiranhaAdmin") || 
                User.HasClaim("Permission", "PiranhaPages") ||
                User.HasClaim("Permission", "PiranhaPosts"));
    }

    /// <summary>
    /// Helper method to check if user has media permissions
    /// </summary>
    protected bool HasMediaPermission()
    {
        // In development/testing, allow anonymous access
        if (AllowAnonymousForTesting())
            return true;
            
        // Check if user is authenticated and has media permissions
        return User.Identity?.IsAuthenticated == true && 
               (User.HasClaim("Permission", "PiranhaAdmin") || 
                User.HasClaim("Permission", "PiranhaMedia"));
    }
}
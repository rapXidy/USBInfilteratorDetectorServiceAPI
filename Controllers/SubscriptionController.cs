using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using USBInfilteratorDetectorServiceAPI.Auth;
using USBInfilteratorDetectorServiceAPI.Services.DTOs;
using USBInfilteratorDetectorServiceAPI.Services.Subscriptions;

namespace USBInfilteratorDetectorServiceAPI.Controllers;

/// <summary>
/// Subscription status endpoint for the current organization.
/// </summary>
[ApiController]
[Route("api/subscription")]
[Authorize]
public class SubscriptionController : ControllerBase
{
    private readonly SubscriptionService _subscriptionService;

    public SubscriptionController(SubscriptionService subscriptionService)
    {
        _subscriptionService = subscriptionService;
    }

    /// <summary>
    /// Returns the current plan and feature flags.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<SubscriptionResponse>> Get(CancellationToken cancellationToken)
        => Ok(await _subscriptionService.GetCurrentAsync(User.GetOrganizationId(), cancellationToken));
}

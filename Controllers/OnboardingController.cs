using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using USBInfilteratorDetectorServiceAPI.Services.DTOs;
using USBInfilteratorDetectorServiceAPI.Services.Onboarding;

namespace USBInfilteratorDetectorServiceAPI.Controllers;

/// <summary>
/// Public organization onboarding endpoints.
/// </summary>
[ApiController]
[Route("api/onboarding")]
[EnableRateLimiting("auth")]
public class OnboardingController : ControllerBase
{
    private readonly OnboardingService _onboardingService;

    public OnboardingController(OnboardingService onboardingService)
    {
        _onboardingService = onboardingService;
    }

    /// <summary>
    /// Starts organization onboarding and creates the first Admin auditor.
    /// </summary>
    [HttpPost("organization")]
    public async Task<ActionResult<OnboardingStatusResponse>> Start([FromBody] StartOnboardingRequest request, CancellationToken cancellationToken)
        => Ok(await _onboardingService.StartAsync(request, cancellationToken));

    /// <summary>
    /// Updates a draft organization profile.
    /// </summary>
    [HttpPut("organization/{id:guid}")]
    public async Task<ActionResult<OnboardingStatusResponse>> Update(Guid id, [FromBody] UpdateOnboardingRequest request, CancellationToken cancellationToken)
        => Ok(await _onboardingService.UpdateDraftAsync(id, request, cancellationToken));

    /// <summary>
    /// Returns onboarding status for an organization.
    /// </summary>
    [HttpGet("organization/{id:guid}")]
    public async Task<ActionResult<OnboardingStatusResponse>> GetStatus(Guid id, CancellationToken cancellationToken)
        => Ok(await _onboardingService.GetStatusAsync(id, cancellationToken));

    /// <summary>
    /// Completes onboarding and returns CorpCode plus Worker API key once.
    /// </summary>
    [HttpPost("organization/{id:guid}/complete")]
    public async Task<ActionResult<CompleteOnboardingResponse>> Complete(Guid id, CancellationToken cancellationToken)
        => Ok(await _onboardingService.CompleteAsync(id, cancellationToken));
}

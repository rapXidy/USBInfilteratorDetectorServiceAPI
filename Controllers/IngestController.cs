using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using USBInfilteratorDetectorServiceAPI.Auth;
using USBInfilteratorDetectorServiceAPI.Services.DTOs;
using USBInfilteratorDetectorServiceAPI.Services.Ingest;

namespace USBInfilteratorDetectorServiceAPI.Controllers;

/// <summary>
/// Worker Service ingestion endpoints authenticated by per-organization API key.
/// </summary>
[ApiController]
[Route("api/ingest")]
[Authorize(AuthenticationSchemes = Auth.ApiKeyAuthenticationHandler.SchemeName)]
public class IngestController : ControllerBase
{
    private readonly IngestService _ingestService;

    public IngestController(IngestService ingestService)
    {
        _ingestService = ingestService;
    }

    /// <summary>
    /// Accepts a batch of structured Worker payloads.
    /// </summary>
    [HttpPost("batch")]
    public async Task<ActionResult<IngestBatchResponse>> Batch([FromBody] IngestBatchRequest request, CancellationToken cancellationToken)
        => Ok(await _ingestService.IngestBatchAsync(User.GetOrganizationId(), request, cancellationToken));
}

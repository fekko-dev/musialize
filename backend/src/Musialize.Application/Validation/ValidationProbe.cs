using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Validation;

namespace Musialize.Application.Validation;

/// <summary>
/// Proves that request types in Application are validated before an endpoint runs (#12).
/// Delete it once a real request type has its own validation test.
/// </summary>
[ValidatableType]
public sealed record ValidationProbe([Required] string? Name);

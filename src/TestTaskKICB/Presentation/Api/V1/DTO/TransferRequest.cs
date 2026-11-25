using System.Text.Json;

namespace TestTaskKICB.Presentation.Api.V1.DTO;

public record TransferRequest(
    long SourceAccountId,
    long DestinationAccountId,
    JsonElement Amount
    );

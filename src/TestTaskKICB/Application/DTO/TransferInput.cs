namespace TestTaskKICB.Application.DTO;

public record TransferInput(
    long SourceAccountId,
    long DestinationAccountId,
    decimal Amount
    );

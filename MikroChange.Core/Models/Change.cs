using MikroChange.Core.Enums;


namespace MikroChange.Core.Models;


public record Change(
    ChangeStatus Status,
    Device? Device,
    string? ErrorMessage
);

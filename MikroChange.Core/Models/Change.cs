using MikroChange.Core.Enums;


namespace MikroChange.Core.Models;


public record Change(
    Device Device,
    ChangeStatus Status,
    string? ErrorMessage
);

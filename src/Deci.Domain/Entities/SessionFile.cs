using System.ComponentModel.DataAnnotations;

namespace Deci.Domain.Entities;

public class SessionFile
{
    public int Id { get; set; }

    public int SessionLogId { get; set; }
    public SessionLog SessionLog { get; set; } = null!;

    [MaxLength(32)]
    public string Kind { get; set; } = "attendance";

    [MaxLength(512)]
    public string StoredPath { get; set; } = "";

    [MaxLength(256)]
    public string OriginalName { get; set; } = "";
}

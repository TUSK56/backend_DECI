using System.ComponentModel.DataAnnotations;

namespace Deci.Domain.Entities;

public class MasterGroupCode
{
    public int Id { get; set; }

    [MaxLength(128)]
    public string Code { get; set; } = "";

    public bool IsActive { get; set; } = true;
}

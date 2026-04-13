using System.ComponentModel.DataAnnotations;

namespace Deci.Domain.Entities;

public class MasterTrainer
{
    public int Id { get; set; }

    [MaxLength(256)]
    public string Name { get; set; } = "";

    public bool IsActive { get; set; } = true;
}

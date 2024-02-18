using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Monochrome.Module.Core.Models
{
    [Index(nameof(CreatedById), IsUnique = false)]
    [Index(nameof(UpdatedById), IsUnique = false)]
    public abstract class BaseModel<T>: IEntityDefinition<T>
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public T Id { get; set; }

        [ConcurrencyCheck]
        public string ConcurrencyStamp { get; set; } = Guid.NewGuid().ToString("N").ToUpper();

        [ForeignKey(nameof(CreatedBy))]
        public string CreatedById { get; set; }

        public User CreatedBy { get; set; }

        public DateTimeOffset DateCreated { get; set; }

        [ForeignKey(nameof(UpdatedBy))]
        public string UpdatedById { get; set; }

        public User UpdatedBy { get; set; }

        public DateTimeOffset? LastUpdated { get; set; }
    }

    public abstract class BaseModel : BaseModel<long>
    {
       
    }
}
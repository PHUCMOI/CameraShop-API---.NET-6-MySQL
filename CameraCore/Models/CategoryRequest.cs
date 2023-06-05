using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CameraCore.Models
{
    public partial class CategoryRequest
    {
        [Required] public string Name { get; set; }
    }
}

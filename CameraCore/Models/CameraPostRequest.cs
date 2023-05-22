using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CameraCore.Models
{
    public partial class CameraPostRequest
    {
        public int? CategoryId { get; set; }
        public string Name { get; set; }
        public string Brand { get; set; }
        public string Description { get; set; }
        public decimal? Price { get; set; }
        public string Img { get; set; }
        public int? Quantity { get; set; }
    }
}

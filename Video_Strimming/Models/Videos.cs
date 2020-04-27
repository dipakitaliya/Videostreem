using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Video_Strimming.Models
{
    public class Videos
    {
        public string Name { get; set; }
        public string ext { get; set; }
        public byte[] video { get; set; }
        public long Id { get; set; }
    }
}
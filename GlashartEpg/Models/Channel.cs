using System.Collections.Generic;

namespace GlashartEpg.Models
{
    public class Channel
    {
        public string Name { get; set; }
        public List<Program> Programs { get; set; } 
    }
}
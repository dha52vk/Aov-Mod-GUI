using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aov_Mod_GUI.Models
{
    public class TrackTemplates
    {
        public required List<TrackTemplate> trackTemplates;
    }

    public class TrackTemplate
    {
        public required string Name { get; set; }
        public required string Xml { get; set; }
    }
}

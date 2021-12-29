using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace FITS.Entities.Common.Common
{
    [XmlRoot()]
    public class EntityConfig  {
        [XmlElement()]
        public List<EntityConfigItem> ConfigItems { get; set; }
    }
    
    public class EntityConfigItem
    {
        [XmlAttribute()]
        public String EntityType { get; set; }

        [XmlAttribute()]
        public String EntityName { get; set; }

        [XmlAttribute()]
        public String QueryName { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace bluebean.ConfigDataExportTool.Config
{
    [XmlRoot("Configure")]
    public class ToolConfig
    {
        [XmlElement("NameSpace")]
        public string NameSpace { get; set; }
 
    }
}

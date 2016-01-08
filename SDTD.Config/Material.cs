using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace SDTD.Config
{
    class Material
    {
        Material()
        {

        }

        public XElement AsXElement()
        {
            return new XElement("material",
                                new XAttribute("name", this.Name));
        }

        /// <summary>
        /// NOTE: This is the id attribute in the XML
        /// </summary>
        public String Name { get; }
    }
}

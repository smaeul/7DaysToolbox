using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace SDTD.Config
{
    public class Block
    {
        Block()
        {

        }

        public XElement AsXElement()
        {
            return new XElement("block",
                                new XAttribute("id", this.ID),
                                new XAttribute("name", this.Name));
        }

        public UInt32 ID { get; }
        public String Name { get; }
    }
}

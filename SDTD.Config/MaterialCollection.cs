using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace SDTD.Config
{
    class MaterialCollection
    {
        MaterialCollection()
        {
            this.materials = new Dictionary<string, Material>();
        }

        public void Add(Material material)
        {
            this.materials.Add(material.Name, material);
        }

        public XElement AsXElement()
        {
            return new XElement("materials",
                                this.materials.Values.OrderBy(m => m.Name).Select(m => m.AsXElement()));
        }

        private Dictionary<String, Material> materials;
    }
}

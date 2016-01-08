using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace SDTD.Config
{
    class BlockCollection
    {
        BlockCollection()
        {
            this.blocks = new Dictionary<string, Block>();
        }

        public void Add(Block block)
        {
            this.blocks.Add(block.Name, block);
        }

        public XElement AsXElement()
        {
            return new XElement("blocks",
                                this.blocks.Values.OrderBy(b => b.ID).Select(b => b.AsXElement()));
        }

        private Dictionary<String, Block> blocks;
    }
}

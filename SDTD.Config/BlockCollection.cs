using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace SDTD.Config
{
    /// <summary>
    /// Represents a collection of blocks; usually, the entire contents of blocks.xml.
    /// </summary>
    public class BlockCollection
    {
        /// <summary>
        /// Adds an existing block to the collection.
        /// </summary>
        /// <param name="block">The Block object to add.</param>
        private void Add(Block block)
        {
            this.blocks.Add(block.Name, block);
            block.Collection = this;
        }

        /// <summary>
        /// Adds a new block to the collection from its XML description.
        /// </summary>
        /// <param name="block">A &lt;block&gt; element in a game-compatible blocks.xml format.</param>
        private void Add(XElement block)
        {
            this.Add(new Block(block));
        }

        /// <summary>
        /// Creates and populates a BlockCollection from a blocks.xml-formatted XML document.
        /// </summary>
        /// <param name="document">A game-compatible blocks.xml-formatted XDocument.</param>
        /// <returns>The filled BlockCollection.</returns>
        public static BlockCollection Load(XDocument document)
        {
            BlockCollection collection = new BlockCollection();
            foreach (XElement block in document.Root.Elements("block")) {
                collection.Add(block);
            }
            return collection;
        }

        /// <summary>
        /// Generates an XDocument containing data for all of the blocks in the collection.
        /// </summary>
        /// <returns>A game-compatible blocks.xml-formatted XDocument.</returns>
        public XDocument ToXDocument()
        {
            return new XDocument(
                new XDeclaration("1.0", "utf-8", "true"),
                new XElement("blocks",
                    this.blocks.Values.OrderBy(b => b.ID).Select(b => b.ToXElement())));
        }

        /// <summary>
        /// The number of blocks in the collection.
        /// </summary>
        public Int32 Count {
            get { return blocks.Count; }
        }

        public Block this[String name] {
            get { return this.blocks.ContainsKey(name) ? this.blocks[name] : null; }
        }

        /// <summary>
        /// All of the blocks in the collection, keyed by name.
        /// </summary>
        private Dictionary<String, Block> blocks = new Dictionary<String, Block>();
    }
}

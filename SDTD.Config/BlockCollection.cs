﻿namespace SDTD.Config
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;

    /// <summary>
    /// Represents a collection of blocks; usually, the entire contents of blocks.xml.
    /// </summary>
    public class BlockCollection : ObservableBaseCollection<Block>, IElementCollection
    {
        /// <summary>
        /// Creates a new block collection, with references to items and materials resolved from the given collections
        /// </summary>
        /// <param name="items">The collection of items to resolve references with.</param>
        /// <param name="materials">The collection of materials to resolve references with.</param>
        public BlockCollection(ItemCollection items, MaterialCollection materials) : base()
        {
            if (items == null) { throw new ArgumentNullException(nameof(items)); }
            if (materials == null) { throw new ArgumentNullException(nameof(materials)); }
            this.Items = items;
            this.Materials = materials;
        }

        public new ItemCollection Items { get; }
        public MaterialCollection Materials { get; }

        /// <summary>
        /// Loads all blocks in the XML file into the collection.
        /// </summary>
        /// <param name="filename">The XML file to read from.</param>
        public void Load(String filename)
        {
            this.Load(XDocument.Load(filename));
        }

        /// <summary>
        /// Loads all blocks in the XDocument into the collection.
        /// </summary>
        /// <param name="document">The XDocument to read from.</param>
        public void Load(XDocument document)
        {
            Dictionary<String, XElement> loadedBlocks = new Dictionary<string, XElement>();
            foreach (XElement element in document.Root.Elements(_elementName)) {
                UInt32 id;
                String name = element.Attribute("name")?.Value;
                if (UInt32.TryParse(element.Attribute("id")?.Value, out id) && name != null) {
                    this.Add(new Block(this, id, name));
                    loadedBlocks.Add(name, element);
                }
            }
            // Requires two passes because blocks reference other blocks
            foreach (KeyValuePair<String, XElement> blockKVP in loadedBlocks) {
                foreach (XElement property in blockKVP.Value.Elements("property")) {
                    this[blockKVP.Key].AddProperty(property);
                }
                foreach (XElement drop in blockKVP.Value.Elements("drop")) {
                    this[blockKVP.Key].AddDrop(drop);
                }
            }
        }

        /// <summary>
        /// Saves the data for all blocks in the collection to a game-compatible XML file.
        /// </summary>
        /// <param name="filename">The path of the file to write to.</param>
        public void Save(String filename)
        {
            XDocument document = new XDocument();
            // The game chokes on the UTF8 BOM
            StreamWriter writer = new StreamWriter(filename, false, new System.Text.UTF8Encoding(false));
            this.Save(document);
            document.Save(writer);
        }

        /// <summary>
        /// Saves the data for all blocks in the collection to a game-compatible XDocument.
        /// </summary>
        /// <param name="document">The XDocument to write to (clearing existing data in it).</param>
        public void Save(XDocument document)
        {
            document.Declaration = new XDeclaration("1.0", "utf-8", "true");
            if (document.Root != null) { document.Root.Remove(); }
            document.Add(new XElement("blocks"));
            document.Root.Add(this.OrderBy(b => b.ID).Select(b => b.ToXElement()));
        }
    }
}

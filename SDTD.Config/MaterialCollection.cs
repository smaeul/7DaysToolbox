using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace SDTD.Config
{
    /// <summary>
    /// Represents a collection of materials; usually, the entire contents of materials.xml.
    /// </summary>
    public class MaterialCollection
    {
        /// <summary>
        /// Adds an existing material to the collection.
        /// </summary>
        /// <param name="material">The Material object to add.</param>
        private void Add(Material material)
        {
            this.materials.Add(material.Name, material);
            material.Collection = this;
        }

        /// <summary>
        /// Adds a new material to the collection from its XML description.
        /// </summary>
        /// <param name="material">A &lt;material&gt; element in a game-compatible materials.xml format.</param>
        private void Add(XElement material)
        {
            this.Add(new Material(material));
        }

        /// <summary>
        /// Creates and populates a MaterialCollection from a materials.xml-formatted XML document.
        /// </summary>
        /// <param name="document">A game-compatible materials.xml-formatted XDocument.</param>
        /// <returns>The filled MaterialCollection.</returns>
        public static MaterialCollection Load(XDocument document)
        {
            MaterialCollection collection = new MaterialCollection();
            foreach (XElement material in document.Root.Elements("material")) {
                collection.Add(material);
            }
            return collection;
        }

        /// <summary>
        /// Generates an XDocument containing data for all of the materials in the collection.
        /// </summary>
        /// <returns>A game-compatible materials.xml-formatted XDocument.</returns>
        public XDocument ToXDocument()
        {
            return new XDocument(
                new XDeclaration("1.0", "utf-8", "true"),
                new XElement("materials",
                    this.materials.Values.OrderBy(m => m.Name).Select(m => m.ToXElement())));
        }

        /// <summary>
        /// The number of materials in the collection.
        /// </summary>
        public Int32 Count {
            get { return materials.Count; }
        }

        public Material this[String name] {
            get { return this.materials.ContainsKey(name) ? this.materials[name] : null; }
        }

        /// <summary>
        /// All of the materials in the collection, keyed by name.
        /// </summary>
        private Dictionary<String, Material> materials = new Dictionary<String, Material>();
    }
}

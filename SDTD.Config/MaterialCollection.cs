namespace SDTD.Config
{
    using System;
    using System.Linq;
    using System.Xml.Linq;

    /// <summary>
    /// Represents a collection of materials; usually, the entire contents of materials.xml.
    /// </summary>
    public class MaterialCollection : ObservableBaseCollection<Material>, IElementCollection
    {
        /// <summary>
        /// Creates a new, empty collection of items.
        /// </summary>
        public MaterialCollection() : base()
        {
            // No additional implementation.
        }

        /// <summary>
        /// Loads all materials in the XDocument into the collection.
        /// </summary>
        /// <param name="document">The XDocument to read from.</param>
        public void Load(XDocument document)
        {
            foreach (XElement element in document.Root.Elements(_elementName)) {
                String name = element.Attribute("id")?.Value;
                if (name != null) {
                    this.Add(new Material(this, name));
                    foreach (XElement property in element.Elements("property")) {
                        this[name].AddProperty(property);
                    }
                }
            }
        }

        /// <summary>
        /// Saves the data for all materials in the collection to a game-compatible XDocument.
        /// </summary>
        /// <param name="document">The XDocument to write to (clearing existing data in it).</param>
        public void Save(XDocument document)
        {
            document.Declaration = new XDeclaration("1.0", "utf-8", "true");
            if (document.Root != null) { document.Root.Remove(); }
            document.Add(new XElement("materials"));
            document.Root.Add(this.OrderBy(m => m.Name).Select(b => b.ToXElement()));
        }
    }
}

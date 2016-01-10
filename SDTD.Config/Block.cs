using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace SDTD.Config
{
    /// <summary>
    /// Represents a single block.
    /// </summary>
    public class Block
    {
        private class Drop
        {
            public Drop(Block block, XElement drop)
            {
                this.Count = drop.Attribute("count")?.Value ?? "1";
                this.Name = drop.Attribute("name")?.Value ?? block.Name;
                this.Probability = Convert.ToSingle(drop.Attribute("prob")?.Value ?? "1");
                this.StickChance = Convert.ToSingle(drop.Attribute("stick_chance")?.Value ?? "0");
                this.ToolCategory = drop.Attribute("tool_category")?.Value;
            }

            public String Count { get; set; }
            // XXX: Item may be a block or item
            public String Name { get; set; }
            public Single Probability { get; set; }
            public Single StickChance { get; set; }
            public String ToolCategory { get; set; }
        }

        /// <summary>
        /// Initializes a block from its XML description.
        /// </summary>
        /// <param name="block">A &lt;block&gt; element in a game-compatible blocks.xml format.</param>
        public Block(XElement block)
        {
            try {
                this.ID = Convert.ToUInt32(block.Attribute("id").Value);
                this.Name = block.Attribute("name").Value;
            } catch {
                throw new ArgumentException("The given XElement does not represent a valid block.");
            }
            this.Fill(block);
        }

        /// <summary>
        /// Adds a drop to the block from its XML description.
        /// </summary>
        /// <param name="drop">A &lt;drop&gt; element in a game-compatible blocks.xml format.</param>
        private void AddDrop(XElement drop)
        {
            String eventName = drop.Attribute("event")?.Value;
            if (eventName == null) {
                throw new ArgumentException("The given drop is missing an event name.");
            }
            if (!this.events.ContainsKey(eventName)) {
                this.events.Add(eventName, new List<Drop>());
            }
            this.events[eventName].Add(new Drop(this, drop));
        }

        /// <summary>
        /// Adds a property to the block from its XML description.
        /// </summary>
        /// <remarks>
        /// This function works on either a name/value property element, or a class property element.
        /// In the case of a class, it calls the other overload for each property in the class.
        /// </remarks>
        /// <param name="property">A &lt;property&gt; element in a game-compatible blocks.xml format.</param>
        private void AddProperty(XElement property)
        {
            String className = property.Attribute("class")?.Value;
            if (className != null) {
                foreach (XElement subprop in property.Elements("property")) {
                    this.AddProperty(subprop, className);
                }
            } else {
                this.AddProperty(property, null);
            }
        }

        /// <summary>
        /// Adds a property to the block from its XML description.
        /// </summary>
        /// <remarks><para>
        /// Except for RepairItems (see below), this function flattens all classed properties to the class.name format
        /// This makes them easier to work with (no need for custom classes), and it makes the generated XML file
        /// smaller. With two exceptions, any classed property can be represented in either way (nested elements or
        /// class.name). Map.Color does not work when nested, and RepairItems.* does not work when flattened. It
        /// appears that at one point, the Map class had multiple properties, and therefore was nested. See the
        /// commented out part of block #25 forestGround as of A13.6.
        /// </para><para>
        /// The way RepairItems is implemented is just dumb. Putting the item name in the name of the property creates
        /// an unbounded number of possible properties. It also requires specific code to parse it, which is why it
        /// cannot be flattened. I've copied the names from the UpgradeBlock.* properties for internal representation.
        /// The only way their layout makes sense is if there are multiple repair paths, which there is currently no
        /// UI or control for. If that was to be implemented, it would go along with multiple upgrade, so both would
        /// be changed at the same time anyway.
        /// </para><para>
        /// This function ignores param1/2 except where special-cased. This has not caused any obvious problems.
        /// * For CanPickup, param1 is the item that is picked up. This is completely replaced by PickupTarget.
        /// * For Model, param1 is (almost) always the same value, and removing it doesn't seem to change anything.
        /// * For PlantGrowing.GrowOnTop, one variant of corn may stop growing. This may need to be special-cased.
        /// </para></remarks>
        /// <param name="property">A &lt;property&gt; element with name, value, [param1/2] attributes.</param>
        /// <param name="className">The class this property belongs to, or null for top-level properties.</param>
        private void AddProperty(XElement property, String className)
        {
            String name = property.Attribute("name")?.Value;
            String value = property.Attribute("value")?.Value;
            if (name == null) {
                throw new ArgumentException("The given property is missing a name.");
            }
            if (name == "CanPickup" && property.Attribute("param1") != null) {
                this.SetProperty("PickupTarget", property.Attribute("param1").Value);
            }
            if (className != null) {
                if (className == "RepairItems") {
                    this.SetProperty("RepairBlock.Item", name);
                    this.SetProperty("RepairBlock.ItemCount", value);
                } else {
                    this.SetProperty(className + "." + name, value);
                }
            } else {
                this.SetProperty(name, value);
            }
        }

        /// <summary>
        /// Fills a block with properties and drop events from the given XML.
        /// </summary>
        /// <remarks>
        /// The id and name attributes of the given block element are ignored.
        /// </remarks>
        /// <param name="block">A &lt;block&gt; element in a game-compatible blocks.xml format.</param>
        private void Fill(XElement block)
        {
            foreach (XElement property in block.Elements("property")) {
                this.AddProperty(property);
            }
            foreach (XElement drop in block.Elements("drop")) {
                this.AddDrop(drop);
            }
        }

        /// <summary>
        /// Gets the value of the specified property as a string, from this block or the block it extends.
        /// </summary>
        /// <param name="name">The name of the property to retrieve.</param>
        /// <returns>The value of the property as a string, or null if the property has the default value.</returns>
        public String GetProperty(String name)
        {
            if (this.properties.ContainsKey(name)) {
                return this.properties[name];
            } else if (name == "Extends") {
                // Explicitly do not recurse when getting the parent block's name.
                return null;
            } else {
                return this.Extends?.GetProperty(name);
            }
        }

        /// <summary>
        /// Sets a property on this block, removing it if it matches the extended block, or if the value is null.
        /// </summary>
        /// <param name="name">The name of the property to modify.</param>
        /// <param name="value">The value to set the property to, or null to inherit/remove from this block.</param>
        private void SetProperty(String name, String value)
        {
            if (name == null) {
                throw new ArgumentNullException();
            }
            if (value == null) {
                // XXX: Does value == null mean use the default or inherit? This code inherits if Extends != null
                if (this.properties.ContainsKey(name)) {
                    this.properties.Remove(name);
                }
            } else {
                if (this.properties.ContainsKey(name)) {
                    if (this.Extends?.GetProperty(name) == value) {
                        this.properties.Remove(name);
                    } else {
                        this.properties[name] = value;
                    }
                } else {
                    if (!(this.Extends?.GetProperty(name) == value)) {
                        this.properties.Add(name, value);
                    }
                }
            }
            if (name == "Extends") {
                // Rewrite all of the other properties to pick up the new inherited value
                foreach (String property in this.properties.Keys.Where(k => k != "Extends").ToArray()) {
                    this.SetProperty(property, this.properties[property]);
                }
            }
        }

        /// <summary>
        /// Generates a game-compatible &lt;block&gt; element containing all data associated with this block.
        /// </summary>
        /// <returns>The &lt;block&gt; element representing this block.</returns>
        public XElement ToXElement()
        {
            XElement element = new XElement("block",
                new XAttribute("id", this.ID),
                new XAttribute("name", this.Name));
            foreach (KeyValuePair<String, String> propertyKVP in this.properties.OrderBy(p => p.Key)) {
                XElement propertyElement = new XElement("property",
                    new XAttribute("name", propertyKVP.Key),
                    new XAttribute("value", propertyKVP.Value));
                switch (propertyKVP.Key) {
                case "RepairBlock.Item":
                    propertyElement = new XElement("property",
                        new XAttribute("class", "RepairItems"),
                        new XElement("property",
                            new XAttribute("name", propertyKVP.Value),
                            new XAttribute("value", this.properties["RepairBlock.ItemCount"])));
                    break;
                case "RepairBlock.ItemCount":
                    continue;
                }
                element.Add(propertyElement);
            }
            foreach (KeyValuePair<String, List<Drop>> eventKVP in this.events.OrderBy(e => e.Key)) {
                foreach (Drop drop in eventKVP.Value) {
                    XElement dropElement = new XElement("drop",
                        new XAttribute("event", eventKVP.Key),
                        new XAttribute("count", drop.Count));
                    if (dropElement.Attribute("count").Value != "0") {
                        if (drop.Name != "null" && drop.Name != this.Name) {
                            dropElement.Add(new XAttribute("name", drop.Name));
                        }
                        if (drop.Probability != 1F) {
                            dropElement.Add(new XAttribute("prob", drop.Probability));
                        }
                        if (drop.StickChance != 0F) {
                            dropElement.Add(new XAttribute("stick_chance", drop.StickChance));
                        }
                        if (drop.ToolCategory != null) {
                            dropElement.Add(new XAttribute("tool_category", drop.ToolCategory));
                        }
                    }
                    element.Add(dropElement);
                }
            }
            return element;
        }

        /// <summary>
        /// The collection of blocks that this block belongs to.
        /// </summary>
        public BlockCollection Collection { get; set; }

        /// <summary>
        /// The numeric identifier of the block.
        /// </summary>
        public UInt32 ID { get; }

        /// <summary>
        /// The (untranslated) internal name of the block.
        /// </summary>
        public String Name { get; }

        /// <summary>
        /// The block that this block inherits from.
        /// </summary>
        public Block Extends {
            get {
                try {
                    return this.Collection[this.GetProperty("Extends")];
                } catch {
                    return null;
                }
            }
        }

        /// <summary>
        /// All of the events associated with the block (keyed by event name), as a list of drops.
        /// </summary>
        private Dictionary<String, List<Drop>> events = new Dictionary<String, List<Drop>>();

        /// <summary>
        /// All of the properties of the block (keyed by name), as strings.
        /// </summary>
        private Dictionary<String, String> properties = new Dictionary<String, String>();
    }
}

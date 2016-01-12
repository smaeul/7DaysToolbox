namespace SDTD.ConfigTest
{
    using SDTD.Config;
    using System;
    using System.IO;
    using System.Xml.Linq;

    public partial class Form : System.Windows.Forms.Form
    {
        private String dataPath = @"C:\Program Files (x86)\Steam\steamapps\common\7 Days To Die\Data\Config";

        private BlockCollection blocks;
        private ItemCollection items;
        private MaterialCollection materials;

        public Form()
        {
            items = new ItemCollection();
            materials = new MaterialCollection();
            blocks = new BlockCollection(items, materials);

            InitializeComponent();
        }

        private void Form_Load(Object sender, EventArgs e)
        {
            statusBox.AppendText(String.Format("Reading data from \"{0}\"...{1}", dataPath, Environment.NewLine));
            XDocument blocksXML = XDocument.Load(dataPath + Path.DirectorySeparatorChar + "blocks.xml");
            XDocument itemsXML = XDocument.Load(dataPath + Path.DirectorySeparatorChar + "items.xml");
            XDocument materialsXML = XDocument.Load(dataPath + Path.DirectorySeparatorChar + "materials.xml");

            items.Load(itemsXML);
            materials.Load(materialsXML);
            blocks.Load(blocksXML);
            statusBox.AppendText(String.Format("  Found {0} blocks.{1}", blocks.Count, Environment.NewLine));
            statusBox.AppendText(String.Format("  Found {0} items.{1}", items.Count, Environment.NewLine));
            statusBox.AppendText(String.Format("  Found {0} materials.{1}", materials.Count, Environment.NewLine));

            statusBox.AppendText("XML for wood frame:" + Environment.NewLine);
            statusBox.AppendText(blocks["woodFrame"].ToXElement().ToString());
            statusBox.AppendText(Environment.NewLine);
            statusBox.AppendText("XML for wall candle (has CanPickup property with param1):" + Environment.NewLine);
            statusBox.AppendText(blocks["candleWall"].ToXElement().ToString());
            statusBox.AppendText(Environment.NewLine);
            statusBox.AppendText("XML for material of rebarFrame (extends solidRebarFrame):" + Environment.NewLine);
            statusBox.AppendText(materials[blocks["rebarFrame"].GetProperty("Material")].ToXElement().ToString());
            statusBox.AppendText(Environment.NewLine);
        }
    }
}

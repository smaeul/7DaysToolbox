using SDTD.Config;
using System;
using System.IO;
using System.Xml.Linq;

namespace SDTD.ConfigTest
{
    public partial class Form : System.Windows.Forms.Form
    {
        public Form()
        {
            InitializeComponent();
        }

        private void Form_Load(Object sender, EventArgs e)
        {
            statusBox.AppendText(String.Format("Reading data from \"{0}\"...{1}", dataPath, Environment.NewLine));
            XDocument blocksXML = XDocument.Load(dataPath + Path.DirectorySeparatorChar + "blocks.xml");
            XDocument itemsXML = XDocument.Load(dataPath + Path.DirectorySeparatorChar + "items.xml");
            XDocument materialsXML = XDocument.Load(dataPath + Path.DirectorySeparatorChar + "materials.xml");
            XDocument recipesXML = XDocument.Load(dataPath + Path.DirectorySeparatorChar + "recipes.xml");

            blocks = BlockCollection.Load(blocksXML);
            statusBox.AppendText(String.Format("  Found {0} blocks.{1}", blocks.Count, Environment.NewLine));
            items = ItemCollection.Load(itemsXML);
            statusBox.AppendText(String.Format("  Found {0} items.{1}", items.Count, Environment.NewLine));
            materials = MaterialCollection.Load(materialsXML);
            statusBox.AppendText(String.Format("  Found {0} materials.{1}", materials.Count, Environment.NewLine));
            recipes = RecipeCollection.Load(recipesXML);
            statusBox.AppendText(String.Format("  Found {0} recipes.{1}", recipes.Count, Environment.NewLine));

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

        private String dataPath = @"C:\Program Files (x86)\Steam\steamapps\common\7 Days To Die\Data\Config";

        private BlockCollection blocks;
        private ItemCollection items;
        private MaterialCollection materials;
        private RecipeCollection recipes;
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WebScriptHook.Framework;
using WebScriptHook.Standalone.Plugins;

namespace WebScriptHook.Standalone
{
    public partial class Form1 : Form
    {
        string clientName = Guid.NewGuid().ToString();
        WebScriptHookComponent wshComponent;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            wshComponent = new WebScriptHookComponent(clientName, new RemoteSettings("localhost", "25555", "/pushws"));
            wshComponent.PluginManager.RegisterPlugin(new PrintToTextBox());
            wshComponent.Start();
        }
    }
}

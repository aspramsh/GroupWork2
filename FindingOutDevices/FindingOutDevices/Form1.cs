using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FindingOutDevices
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        /// <summary>
        /// A button for starting the app
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            Action<List<string>> UpdateUI = UpdateTextbox;
            Manager manager = new Manager();
            manager.Run(UpdateUI);
        }
        /// <summary>
        /// A method that updates textbox
        /// </summary>
        /// <param name="users"></param>
        public void UpdateTextbox(List<string> users)
        {
            foreach (string data in users)
            {
                this.Invoke((MethodInvoker)(() =>
                {
                    richTextBox1.Text = "";
                    richTextBox1.Text += data + "\n";
                }));
            }
        }
    }
}

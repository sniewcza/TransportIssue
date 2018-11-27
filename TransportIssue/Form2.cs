using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TransportIssue
{
    public partial class Form2 : Form
    {
        public string Text {  set => this.richTextBox1.Text = value; }
        public Form2()
        {
            InitializeComponent();
            
        }
    }
}

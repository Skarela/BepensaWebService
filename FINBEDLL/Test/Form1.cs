using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Test
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        public string organizacion;
        public string url;
        private void button1_Click(object sender, EventArgs e)
        {
            FINBEDLL.Conecta login = new FINBEDLL.Conecta();

            organizacion = login.conectar(6, 3);
            MessageBox.Show(organizacion);
        }
    }
}

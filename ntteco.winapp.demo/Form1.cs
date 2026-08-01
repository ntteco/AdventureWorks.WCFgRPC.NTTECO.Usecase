using AdventureWorks.GrpcClient;
using ntteco.winapp.demo.PersonServiceReference;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ntteco.winapp.demo
{
    public partial class Form1 : DevExpress.XtraEditors.XtraForm
    {
        public Form1()
        {
            InitializeComponent();
        }

        private async void simpleButton1_Click(object sender, EventArgs e)
        {
            var client = new PersonServiceClient();
            var person = client.GetPerson(1);

            MessageBox.Show($"{person.FirstName} {person.LastName}");

            ///
            var pc = new ProductClient("localhost:5000");
            var p = await pc.GetProductAsync(680);
            MessageBox.Show($"Product: {p.Name}");
            await pc.ShutdownAsync();

    }
}
}

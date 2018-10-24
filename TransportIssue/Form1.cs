using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using TransportIssue.Utilities;
namespace TransportIssue
{
    public partial class Form1 : Form
    {
        TransportIssueSolver _solver;
        double[,] _costs;
        double[] _recipents;
        double[] _providers;
        public Form1()
        {
            InitializeComponent();
            
        }

        private void button1_Click(object sender, System.EventArgs e)
        {
            try
            {
                _costs = ParseCosts();
                _recipents = ParseRecipents();
                _providers = ParseProviders();

                Solve();
            }
            catch(Exception ex)
            {
                
                MessageBox.Show($"Ups \n {ex.Message}");
            }
           
        }

        private double[,] ParseCosts()
        {
            double[,] costs = new double[3, 3];

            costs[0, 0] = Convert.ToDouble(textBox1.Text);
            costs[0, 1] = Convert.ToDouble(textBox2.Text);
            costs[0, 2] = Convert.ToDouble(textBox3.Text);
            costs[1, 0] = Convert.ToDouble(textBox4.Text);
            costs[1, 1] = Convert.ToDouble(textBox5.Text);
            costs[1, 2] = Convert.ToDouble(textBox6.Text);
            costs[2, 0] = Convert.ToDouble(textBox7.Text);
            costs[2, 1] = Convert.ToDouble(textBox8.Text);
            costs[2, 2] = Convert.ToDouble(textBox9.Text);

            return costs;
        }

        private double[] ParseProviders()
        {
            double[] providers = new double[3];

            providers[0] = Convert.ToDouble(textBox13.Text);
            providers[1] = Convert.ToDouble(textBox14.Text);
            providers[2] = Convert.ToDouble(textBox15.Text);

            return providers;
        }

        private double[] ParseRecipents()
        {
            double[] recipents = new double[3];

            recipents[0] = Convert.ToDouble(textBox10.Text);
            recipents[1] = Convert.ToDouble(textBox11.Text);
            recipents[2] = Convert.ToDouble(textBox12.Text);

            return recipents;
        }

        private async void Solve()
        {
            _solver = new TransportIssueSolver();

            double baseZ=0;
            double optimalZ=0;
            await Task.Run(() =>
            {
                var baseSolution = _solver.FindaBaseSolution(_costs, _providers, _recipents);


                baseZ = _solver.getZ(_costs, baseSolution);
               
                var reversedCostsTable = _solver.InverseCostsMatrix(_costs, baseSolution);

                while (true)
                {
                    var dualVariables = _solver.BuildDualVariables(reversedCostsTable);

                    var alfa = dualVariables.Item1;
                    var beta = dualVariables.Item2;

                    var reversedBaseSolution = _solver.InverseBaseSolutionMatrix(baseSolution, _costs);
                    var optimalityTable = _solver.BuildOptimalityIndexTable(reversedBaseSolution, alfa, beta);

                    if (_solver.isOptimal(optimalityTable))
                    {
                        break;
                    }

                    var cyclePoints = _solver.BuildCycle(optimalityTable);
                    baseSolution = _solver.SolveTheCycle(cyclePoints, baseSolution);
                }

                optimalZ = _solver.getZ(_costs, baseSolution);
            });

            label1.Text = $"Bazowe Z: {baseZ}";
            label2.Text = $"Optymalne Z {optimalZ}";
        }
    }
}

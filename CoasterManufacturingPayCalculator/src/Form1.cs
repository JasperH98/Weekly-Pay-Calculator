using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using CsvHelper;

namespace CoasterManufacturingPayCalculator
{
    /// <summary>
    /// Representss the main form for calculating employee weekly pa.
    /// Provides functionality to calculate gross pay, tax, superannutation, and net pay,
    /// and allows exporting pay slips to a CSV file.
    /// </summary>
    public partial class WeeklyPayCalculator : Form
    {
        /// <summary>
        /// Gets or sets the last error message encountered during operation.
        /// </summary>
        public string LastErrorMessage { get; set; }

        private decimal grossPay;
        private decimal tax;
        private decimal super;
        private decimal netPay;

        /// <summary>
        /// Initialises a new instance of the <see cref="WeeklyPayCalculator"/> class.
        /// Loads employee data from a CSV file and populates the employee details list.
        /// </summary>
        public WeeklyPayCalculator()
        {
            InitializeComponent();

            super = 0.115m; // Default superannuation rate

            using (var reader = new StreamReader("employee.csv"))
            {
                List<PaySlip> paySlips = new List<PaySlip>();

                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(',');

                    decimal hourlyRate = decimal.TryParse(values[3], out hourlyRate) ? hourlyRate : 0;
                    bool taxThreshold = values[4].Trim().ToUpper() == "Y";

                    PaySlip paySlip = new PaySlip(int.Parse(values[0]), values[1], values[2], hourlyRate, taxThreshold);

                    paySlip.LoadTaxRates(taxThreshold);

                    paySlips.Add(paySlip);
                }

                listEmployeeDetails.DataSource = paySlips;
                listEmployeeDetails.DisplayMember = "FullDetails";
            }
        }

        /// <summary>
        /// Handles the Calculate button click event.
        /// Computes the gross pay, tax, superannuation, and net pay for the selected Employee
        /// based on the number of hours worked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCalculate_Click(object sender, EventArgs e)
        {
            PaySlip selectedEmployee = listEmployeeDetails.SelectedItem as PaySlip;

            if (selectedEmployee == null)
            {
                MessageBox.Show("Please select an employee.");
                return;
            }

            if (!decimal.TryParse(txtHoursWorked.Text, out decimal hoursWorked))
            {
                LastErrorMessage = "Please enter valid hours worked.";
                MessageBox.Show(LastErrorMessage);
                return;
            }

            selectedEmployee.LoadTaxRates(selectedEmployee.TaxThreshold);

            PayCalculator payCalculator;

            if (selectedEmployee.TaxThreshold)
            {
                payCalculator = new PayCalculatorWithThreshold(selectedEmployee.HourlyRate, super, selectedEmployee.TaxRates);
            }
            else
            {
                payCalculator = new PayCalculatorNoThreshold(selectedEmployee.HourlyRate, super, selectedEmployee.TaxRates);
            }

            grossPay = payCalculator.CalculateGrossPay(hoursWorked);
            tax = payCalculator.CalculateTax(grossPay);
            super = payCalculator.CalculateSuper(grossPay);
            netPay = payCalculator.CalculateNetPay(hoursWorked);
            string newline = Environment.NewLine;

            txtPaymentSummary.Text = $"Employee ID: {selectedEmployee.EmployeeID}" + newline +
                $"Name: {selectedEmployee.FullDetails}" + newline +
                $"Gross Pay: {grossPay:C}" + newline +
                $"Tax: {tax:C}" + newline +
                $"Superannuation: {super:C}" + newline +
                $"Net Pay: {netPay:C}";
        }

        /// <summary>
        /// Handles the Save button click event.
        /// Exports the calculated pay slip details to a CSV file with a timestamped filename.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSave_Click(object sender, EventArgs e)
        {
            PaySlip selectedEmployee = listEmployeeDetails.SelectedItem as PaySlip;

            if (selectedEmployee == null)
            {
                MessageBox.Show("Please select an employee.");
                return;
            }

            string dateTimeNow = DateTime.Now.ToString("yyyyMMdd-HHmmss");
            string fileName = $"Pay-{selectedEmployee.EmployeeID}-{selectedEmployee.FullDetails}-{dateTimeNow}.csv";

            var records = new List<dynamic>
            {
                new
                {
                    EmployeeId = selectedEmployee.EmployeeID,
                    FullName = selectedEmployee.FullDetails,
                    HoursWorked = txtHoursWorked.Text,
                    HourlyRate = selectedEmployee.HourlyRate,
                    TaxThreshhold = selectedEmployee.TaxThreshold ? "Y" : "N",
                    GrossPay = grossPay,
                    Tax = tax,
                    NetPay = netPay,
                    Superannuation = super
                }
            };

            using (var writer = new StreamWriter(fileName))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(records);
            }

            MessageBox.Show("Payment data has been saved successfully");
        }

        /// <summary>
        /// Handles the Logout button click event.
        /// Logs the user out and returns to the login form, clearing the login fields.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLogout_Click(object sender, EventArgs e)
        {
            LogIn logIn = new LogIn();
            logIn.ResetFields();
            logIn.Show();

            this.Hide();
        }
    }
    
}

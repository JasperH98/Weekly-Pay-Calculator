using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;

namespace CoasterManufacturingPayCalculator
{

    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// Initializes the form and application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new LogIn());
        }
    }

    /// <summary>
    /// Represents the tax brackets lower and upper limit as well as tax rate a and tax rate b.
    /// </summary>
    public class TaxRate
    {
        /// <summary>
        /// Gets or sets the lower limit of the tax bracket.
        /// </summary>
        public decimal LowerLimit { get; set; }
        /// <summary>
        /// Gets and sets the upper limit of the tax bracket.
        /// </summary>
        public decimal UpperLimit { get; set; }
        /// <summary>
        /// Gets or sets the primary tax rate for the bracket.
        /// </summary>
        public decimal RateA { get; set; }
        /// <summary>
        /// Gets or sets the additional tax rate for amounts exceeding the upper limit.
        /// </summary>
        public decimal RateB { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TaxRate"/> class.
        /// </summary>
        /// <param name="lowerlimit"></param>
        /// <param name="upperlimit"></param>
        /// <param name="rateA"></param>
        /// <param name="rateB"></param>
        public TaxRate(decimal lowerlimit, decimal upperlimit, decimal rateA, decimal rateB)
        {
            LowerLimit = lowerlimit;
            UpperLimit = upperlimit;
            RateA = rateA;
            RateB = rateB;
        }

    }
    /// <summary>
    /// Represents an employee's pay slip with tax rate and payment details.
    /// </summary>
    public class PaySlip
    {
        /// <summary>
        /// Gets or sets the employee's ID.
        /// </summary>
        public int EmployeeID { get; set; }
        /// <summary>
        /// Gets or sets the employee's first name.
        /// </summary>
        public string FirstName { get; set; }
        /// <summary>
        /// Gets or sets the employee's last name.
        /// </summary>
        public string LastName { get; set; }
        /// <summary>
        /// Gets or sets the employee's hourly pay rate.
        /// </summary>
        public decimal HourlyRate { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether the employee has a tax threshold.
        /// </summary>
        public bool TaxThreshold { get; set; }
        /// <summary>
        /// Gets or sets the list of applicable tax rates for the employee.
        /// </summary>
        public List<TaxRate> TaxRates { get; set; }

        /// <summary>
        /// Gets the full details of the employee, including ID and name.
        /// </summary>
        public string FullDetails => $"{FirstName} {LastName}";

        /// <summary>
        /// Initializes a new instance of the <see cref="PaySlip"/> class.
        /// </summary>
        /// <param name="employeeID"></param>
        /// <param name="firstName"></param>
        /// <param name="lastName"></param>
        /// <param name="hourlyRate"></param>
        /// <param name="taxThreshold"></param>
        public PaySlip(int employeeID, string firstName, string lastName, decimal hourlyRate, bool taxThreshold)
        {
            EmployeeID = employeeID;
            FirstName = firstName;
            LastName = lastName;
            HourlyRate = hourlyRate;
            TaxThreshold = taxThreshold;
            TaxRates = new List<TaxRate>();
        }

        /// <summary>
        /// Loads tax rates from a file based on whether the employee has a tax threshold.
        /// </summary>
        /// <param name="withThreshold"></param>True if using a tax threshold; otherwise false.
        public void LoadTaxRates(bool withThreshold)
        {
            TaxRates.Clear();
            string taxRateFile = withThreshold ? "taxrate-withthreshold.csv" : "taxrate-nothreshold.csv";

            using (var reader = new StreamReader(taxRateFile))

                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(',');

                    if (values.Length >= 4 &&
                    decimal.TryParse(values[0], out var lowerLimit) &&
                    decimal.TryParse(values[1], out var upperLimit) &&
                    decimal.TryParse(values[2], out var rateA) &&
                    decimal.TryParse(values[3], out var rateB))
                    {
                        var taxRate = new TaxRate(lowerLimit, upperLimit, rateA, rateB); ;
                        TaxRates.Add(taxRate);
                    }
                }

        }
    }

    /// <summary>
    /// Base class for calculating employee pay, including gross pay, net pay, tax, and super.
    /// </summary>
    public abstract class PayCalculator
    {
        /// <summary>
        /// Gets or sets the hourly rate for the employee.
        /// </summary>
        public decimal hourlyRate { get; set; }
        /// <summary>
        /// gets or sets the superannuation for the employee.
        /// </summary>
        public decimal super { get; set; }
        /// <summary>
        /// Gets or sets the list of applicable tax rates for each employee.
        /// Each tax rate is represented by an instance of <see cref="TaxRate"/>.
        /// </summary>
        public List<TaxRate> TaxRates { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PayCalculator"/> class.
        /// </summary>
        /// <param name="hourlyRate">The hourly pay rate for the employee.</param>
        /// <param name="superRate">The superannuation rate for the employee.</param>
        /// <param name="taxRates">The list of tax rates applicable to the employee.</param>
        public PayCalculator(decimal hourlyRate, decimal superRate, List<TaxRate> taxRates)
        {
            this.hourlyRate = hourlyRate;
            this.super = superRate;
            this.TaxRates = taxRates;

        }

        /// <summary>
        /// Calculates the gross pay based on the number of hours worked.
        /// </summary>
        /// <param name="hoursWorked">The total number of hours worked by the employee.</param>
        /// <returns>The gross pay as a decimal value.</returns>
        public decimal CalculateGrossPay(decimal hoursWorked)
        {
            return hourlyRate * hoursWorked;
        }

        /// <summary>
        /// Calculates the superannuation contribution based on the gross pay.
        /// </summary>
        /// <param name="grossPay">The gross pay of the employee.</param>
        /// <returns>The superannuation contribution as a decimal value.</returns>
        public decimal CalculateSuper(decimal grossPay)
        {
            var superRate = 0.115m;
            return grossPay * superRate;
        }

        /// <summary>
        /// Calculates the tax amount based on the gross pay.
        /// This method must be implemented in derived classes to handle specific tax rules.
        /// </summary>
        /// <param name="grossPay">The gross pay for the employee.</param>
        /// <returns>The calculated tax amount as a decimal value.</returns>
        public abstract decimal CalculateTax(decimal grossPay);

        /// <summary>
        /// Calculates the net pay after deducting tax and superannuation from the gross pay.
        /// </summary>
        /// <param name="hoursWorked">The total number of hours worked by the employee.</param>
        /// <returns>The net pay as a decimal value.</returns>
        public decimal CalculateNetPay(decimal hoursWorked)
        {
            decimal grossPay = CalculateGrossPay(hoursWorked);
            decimal tax = CalculateTax(grossPay);
            decimal super = CalculateSuper(grossPay);

            return (grossPay - tax) - super;
        }
    }

    /// <summary>
    /// Extends <see cref="PayCalculator"/> class for employees without a tax threshold.
    /// </summary>
    public class PayCalculatorNoThreshold : PayCalculator
    {
        /// <summary>
        /// Initalizes a new instance of the <see cref="PayCalculatorNoThreshold"/> class.
        /// </summary>
        /// <param name="hourlyRate">The hourly rate for the employee.</param>
        /// <param name="superRate">The superannuation rate.</param>
        /// <param name="taxRates">The list of tax rates applicable to the employee.</param>
        public PayCalculatorNoThreshold(decimal hourlyRate, decimal superRate, List<TaxRate> taxRates) : base(hourlyRate, superRate, taxRates) { }

        /// <summary>
        /// Calculates the tax for employees without a tax threshold.
        /// </summary>
        /// <param name="grossPay">The gross pay to calculate tax on.</param>
        /// <returns>The calculated tax amount.</returns>
        public override decimal CalculateTax(decimal grossPay)
        {
            decimal tax = 0;

            foreach (var taxRate in TaxRates)
            {
                if (grossPay <= taxRate.LowerLimit)
                {
                    continue;
                }

                decimal taxableAmount = Math.Min(grossPay, taxRate.UpperLimit) - taxRate.LowerLimit;

                decimal taxForBracket = taxableAmount * taxRate.RateA;
                tax += taxForBracket;
            }

            return tax;
        }

    }

    /// <summary>
    /// Extends <see cref="PayCalculator"/> class for employees with a tax threshold.
    /// </summary>
    public class PayCalculatorWithThreshold : PayCalculator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PayCalculatorWithThreshold"/> class.
        /// </summary>
        /// <param name="hourlyRate">The hourly rate for the employee.</param>
        /// <param name="superRate">The superannuation rate.</param>
        /// <param name="taxRates">The list of tax rates applicable to the employee.</param>
        public PayCalculatorWithThreshold(decimal hourlyRate, decimal superRate, List<TaxRate> taxRates) : base(hourlyRate, superRate, taxRates) { }

        /// <summary>
        /// Calculates the tax for employees with a tax threshold.
        /// </summary>
        /// <param name="grossPay">The gross pay to calculate tax on.</param>
        /// <returns>The calculated tax amount.</returns>
        public override decimal CalculateTax(decimal grossPay)
        {
            decimal tax = 0;

            foreach (var taxRate in TaxRates)
            {
                if (grossPay <= taxRate.LowerLimit)
                {
                    continue;
                }

                decimal taxableAmount = Math.Min(grossPay, taxRate.UpperLimit) - taxRate.LowerLimit;

                decimal taxForBracket = taxableAmount * taxRate.RateA;
                tax += taxForBracket;
            }

            return tax;
        }

    }

}

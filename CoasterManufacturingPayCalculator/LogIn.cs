using System;
using System.Text;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.IO;

namespace CoasterManufacturingPayCalculator
{
    /// <summary>
    /// Represents the login for the application.
    /// Handles user authentication and navigation to the main form.
    /// </summary>
    public partial class LogIn : Form
    {
        /// <summary>
        /// The file path to the user credentials file.
        /// </summary>
        private string filePath = "users.txt";

        /// <summary>
        /// Initialises a new instance of the <see cref="LogIn"/> class
        /// </summary>
        public LogIn()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Handles th elogin button click event.
        /// Authenticates the user and opens the main form if successful.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text;
            string password = txtPassword.Text;
            string hashedPassword = HashPassword(password);

            if (ValidateUser(username, hashedPassword))
            {
                WeeklyPayCalculator weeklyPayCalculator = new WeeklyPayCalculator();
                weeklyPayCalculator.Show();
                this.Hide();
            } 
            else
            {
                MessageBox.Show("Invalid username or password.");
            }
        }

        /// <summary>
        /// Hases a password using the SHA-256 algorithm.
        /// </summary>
        /// <param name="password">The plain-text password to hash</param>
        /// <returns>The hashed password as a hexadecimal string</returns>
        public string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));

                StringBuilder builder = new StringBuilder();
                foreach (var b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        /// <summary>
        /// Validate's the user's credentials by checking them against stored data.
        /// </summary>
        /// <param name="username">The username entered by the user</param>
        /// <param name="hashedPassword">The hashed password entered by the user.</param>
        /// <returns><c>true</c> if the credentials match; otherwise, <c>false</c>.</returns>
        private bool ValidateUser(string username, string hashedPassword)
        {
            if (!File.Exists(filePath))
            {
                MessageBox.Show("User file not found.");
                return false;
            }

            string[] lines = File.ReadAllLines(filePath);

            foreach (string line in lines)
            {
                string[] parts = line.Split(':');

                if (parts.Length == 2)
                {
                    string storedUsername = parts[0];
                    string storedHash = parts[1];


                    if (storedUsername == username && storedHash == hashedPassword)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Handles the form closing event.
        /// Ensures the application exits completely when the login form is closed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LogIn_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }

        /// <summary>
        /// Clears the username and password fields on the form.
        /// Useful for resetting the form after a failed login or logout.
        /// </summary>
        public void ResetFields()
        {
            txtUsername.Text = "";
            txtPassword.Text = "";
        }

    }
}

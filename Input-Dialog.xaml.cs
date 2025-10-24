using System.Windows;

namespace ContractMonthlyClaimSystem2
{
    public partial class InputDialog : Window
    {
        // Renamed to avoid ambiguity with any control names
        public string UserAnswer { get; private set; }

        public InputDialog(string title, string prompt)
        {
            InitializeComponent();
            this.Title = title;
            lblPrompt.Text = prompt; // Refers to the TextBlock in XAML
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            UserAnswer = txtInput.Text.Trim();
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}

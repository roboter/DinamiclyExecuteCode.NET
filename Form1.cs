using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace DynCode
{
    public partial class Form1 : Form
    {
        private const string CSharpDefaultCode =
@"List<string> result = new List<string>();

foreach (string country in countryList.Items)
{
    if (country.StartsWith(""C"", StringComparison.InvariantCultureIgnoreCase))
    {
        result.Add(country);
    }
}

return result;";

        private const string FSharpDefaultCode =
@"let result = new List<string>()

for item in countryList.Items do
    let country = string item
    if country.StartsWith(""C"", StringComparison.InvariantCultureIgnoreCase) then
        result.Add(country)

result";

        public Form1()
        {
            InitializeComponent();
            InitializeLanguageSelector();
        }

        private void InitializeLanguageSelector()
        {
            cmbLanguage.Items.Clear();
            cmbLanguage.Items.Add("C#");
            cmbLanguage.Items.Add("F#");
            cmbLanguage.SelectedIndex = 0;
        }

        private void cmbLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedLang = cmbLanguage.SelectedItem as string ?? "C#";

            if (selectedLang == "F#")
            {
                richTextBox1.Text = "type SelectCountries() =\n    member this.FilterCountries(countryList: ListBox) : List<string> =";
                richTextBox2.Text = "// returns List<string> (ResizeArray)";
                txtCodeToRun.Text = FSharpDefaultCode;
            }
            else
            {
                richTextBox1.Text = "public List<string> FilterCountries(ListBox countryList)\n{";
                richTextBox2.Text = "}";
                txtCodeToRun.Text = CSharpDefaultCode;
            }
        }

        private void btnRunCode_Click(object sender, EventArgs e)
        {
            Execute(txtCodeToRun.Text);
        }

        private string BuildCSharpCode(string userCode)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Windows.Forms;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine();
            sb.AppendLine("namespace CountryList");
            sb.AppendLine("{");
            sb.AppendLine("      public class SelectCountries");
            sb.AppendLine("      {");
            sb.AppendLine("            public List<string> FilterCountries(ListBox countryList)");
            sb.AppendLine("            {");
            sb.AppendLine(userCode);
            sb.AppendLine("            }");
            sb.AppendLine("      }");
            sb.AppendLine("}");
            return sb.ToString();
        }

        private string BuildFSharpCode(string userCode)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("namespace CountryList");
            sb.AppendLine();
            sb.AppendLine("open System");
            sb.AppendLine("open System.Windows.Forms");
            sb.AppendLine("open System.Collections.Generic");
            sb.AppendLine();
            sb.AppendLine("type SelectCountries() =");
            sb.AppendLine("    member this.FilterCountries(countryList: ListBox) : List<string> =");

            string[] lines = userCode.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            foreach (string line in lines)
            {
                sb.AppendLine("        " + line);
            }

            return sb.ToString();
        }

        private void Execute(string code)
        {
            CodeLanguage language = (cmbLanguage.SelectedItem != null && cmbLanguage.SelectedItem.ToString() == "F#")
                ? CodeLanguage.FSharp
                : CodeLanguage.CSharp;

            string classCode = (language == CodeLanguage.FSharp)
                ? BuildFSharpCode(code)
                : BuildCSharpCode(code);

            dynamic classRef;
            try
            {
                txtErrors.Clear();

                classRef = CodeHelper.HelperFunction(classCode, "CountryList.SelectCountries", new object[] { }, language);

                if (classRef is CompilerErrorCollection)
                {
                    StringBuilder sberror = new StringBuilder();
                    foreach (CompilerError error in (CompilerErrorCollection)classRef)
                    {
                        sberror.AppendLine(string.Format("{0}:{1} {2} {3}", error.Line, error.Column, error.ErrorNumber, error.ErrorText));
                    }
                    txtErrors.Text = sberror.ToString();
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Compilation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtErrors.Text = ex.Message;
                return;
            }

            try
            {
                List<string> targetValues = classRef.FilterCountries(lstSource);

                lstTarget.Items.Clear();
                if (targetValues != null)
                {
                    lstTarget.Items.AddRange(targetValues.ToArray());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Execution Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtErrors.Text = ex.Message;
            }
        }
    }
}
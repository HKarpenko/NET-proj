using System;
using System.Windows.Forms;
using System.IO;
using System.Globalization;
using System.Collections.Generic;

namespace ASCII_file_formatting
{
    public partial class ASCII_Formatting : Form
    {
        public static List<string> array_input_content = new List<string>();
        public static List<string> array_output_content = new List<string>();

        public static string input_file_name;

        public ASCII_Formatting()
        {
            InitializeComponent();
        }

        private void input_browse_but_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog OFD = new OpenFileDialog();
                if (OFD.ShowDialog() == DialogResult.OK)
                {
                    input_file_path_text.Clear();
                    input_file_content.Clear();
                    array_input_content.Clear();

                    string path = OFD.FileName;
                    string extension = Path.GetExtension(path);

                    if (extension != ".txt")
                        throw new FormatException();

                    input_file_path_text.Text = path;
                    input_file_name = OFD.SafeFileName;

                    var fileStream = OFD.OpenFile();
                    using (StreamReader reader = new StreamReader(fileStream))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            array_input_content.Add(line);
                            input_file_content.Text += line + "\r\n";
                        }
                    }
                    fileStream.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (array_input_content.Count == 0 || text_antenna_height.Text == string.Empty)
                    throw new Exception("At least one required field is empty");

                decimal antenna_height=0;

                NumberStyles style = NumberStyles.Number;
                CultureInfo culture = CultureInfo.InvariantCulture;
                if (!decimal.TryParse(text_antenna_height.Text, style, culture, out antenna_height))
                {
                    text_antenna_height.Clear();
                    throw new Exception("Incorrect antenna height input format");
                }

                array_output_content.Clear();
                text_output_file.Clear();

                //Data is ok, start processing

                for (int i = 0; i < array_input_content.Count - 1; i++)
                {
                    try
                    {
                        string[] separator = { "," };
                        string[] ZDA = array_input_content[i].Split(separator, StringSplitOptions.RemoveEmptyEntries);
                        string[] GGA = array_input_content[i + 1].Split(separator, StringSplitOptions.RemoveEmptyEntries);
                        if (ZDA[0] == "$GPZDA" && GGA[0] == "$GPGGA" && ZDA[1] == GGA[1])
                        {
                            string out_line = "";

                            out_line += ZDA[4] + "/" + ZDA[3] + "/" + ZDA[2]; //Date

                            out_line += "," + ZDA[1][0] + ZDA[1][1] + ":" + ZDA[1][2] + ZDA[1][3] + ":" + ZDA[1][4] + ZDA[1][5]; //Time

                            out_line += "," + convertToDecimalDegree(GGA[2], GGA[3], 2); //latitude

                            out_line += "," + convertToDecimalDegree(GGA[4], GGA[5], 3); //longitude

                            out_line += "," + ellipsoidalHeight(GGA[9], GGA[11], antenna_height); //Ellipsoidal height

                            array_output_content.Add(out_line);
                            text_output_file.Text += out_line + "\r\n";
                            i++;
                        }
                    }
                    catch (Exception ex) { }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public static string convertToDecimalDegree(string dmFormat, string сardinal, int degNum)
        {
            decimal degrees = decimal.Parse(dmFormat.Substring(0, degNum), CultureInfo.InvariantCulture);
            decimal minutes = decimal.Parse(dmFormat.Substring(degNum, dmFormat.Length - degNum), CultureInfo.InvariantCulture);
            if (сardinal == "S" || сardinal == "W")
            {
                degrees *= -1;
                minutes *= -1;
            }
            return (degrees + minutes / 60).ToString("0.00000000", System.Globalization.CultureInfo.InvariantCulture);
        }
        public static string ellipsoidalHeight(string SmeanAltitude, string SgeoHeight, decimal userAntennaHeight)
        {
            decimal meanAltitude = decimal.Parse(SmeanAltitude, CultureInfo.InvariantCulture);
            decimal geoHeight = decimal.Parse(SgeoHeight, CultureInfo.InvariantCulture);
            return (meanAltitude + geoHeight - userAntennaHeight).ToString("0.00000", System.Globalization.CultureInfo.InvariantCulture); ;
        }

        private void clear_but_Click(object sender, EventArgs e)
        {
            try
            {
                array_input_content.Clear();
                array_output_content.Clear();
                text_antenna_height.Clear();
                text_output_file.Clear();
                input_file_content.Clear();
                input_file_path_text.Clear();
            }
            catch (Exception ex) { }
        }

        private void saveAs_but_Click(object sender, EventArgs e)
        {
            if (array_output_content.Count == 0)
                return;
            SaveFileDialog SFD = new SaveFileDialog();
            SFD.InitialDirectory = @"C:\";
            SFD.RestoreDirectory = true;
            SFD.FileName = input_file_name.Substring(0, input_file_name.Length-4) + "_output.txt";
            SFD.DefaultExt = "txt";
            SFD.Filter = "txt files (*.txt)|*.txt";
            try
            {
                if (SFD.ShowDialog() == DialogResult.OK)
                {
                    var fileStream = SFD.OpenFile();
                    using (StreamWriter sw = new StreamWriter(fileStream))
                    {
                        foreach (string s in array_output_content)
                            sw.WriteLine(s);
                    }
                    MessageBox.Show("The file was saved successfully");
                    fileStream.Close();
                }
            }
            catch (Exception ex) { }

        }
    }
}

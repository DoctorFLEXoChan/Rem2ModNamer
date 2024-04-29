using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using IniParser;
using IniParser.Model;
using IniParser.Parser;
using System.IO;
using System.Text.RegularExpressions;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {

        public Form1()
        {
            InitializeComponent();
        }

        private void MoveFilesByChunkIDOrModName(string folderPath, string baseDestinationPath)
        {
            try
            {
                if (!Directory.Exists(folderPath))
                {
                    MessageBox.Show($"Source directory '{folderPath}' does not exist.");
                    return;
                }

                if (string.IsNullOrEmpty(baseDestinationPath) || !Directory.Exists(baseDestinationPath))
                {
                    MessageBox.Show($"Invalid or nonexistent base destination path: {baseDestinationPath}.");
                    return;
                }

                dataGridView1.EndEdit();
                HashSet<string> selectedChunkIDs = GetSelectedChunkIDs(); // Assuming this method exists to get selected chunk IDs
                bool foundMatchingFile = false;

                foreach (string filePath in Directory.GetFiles(folderPath))
                {
                    string fileName = Path.GetFileNameWithoutExtension(filePath);
                    string[] numbers = fileName.Where(char.IsDigit)
                                               .Select(char.GetNumericValue)
                                               .Select(Convert.ToInt32)
                                               .Select(n => n.ToString())
                                               .ToArray();

                    // Check if numbers form a string of only '1's and '0's
                    if (numbers.Length == 1 && (numbers[0] == "0" || numbers[0] == "1"))
                    {
                        continue; // Skip this file
                    }

                    if (numbers.Length > 0 && numbers.Length <= 3)
                    {
                        string chunkId = string.Join("", numbers);

                        if (selectedChunkIDs.Count == 0 || selectedChunkIDs.Contains(chunkId))
                        {
                            string modName = GetModNameForChunkID(chunkId);
                            string offset = GetOffsetForChunkID(chunkId); // Retrieve Offset for moving
                            string destinationPath = Path.Combine(baseDestinationPath, offset);

                            Directory.CreateDirectory(destinationPath); // Ensure the destination directory exists
                            MoveAndRenameFile(filePath, modName, destinationPath);
                            foundMatchingFile = true;
                        }
                    }
                }

                if (!foundMatchingFile)
                {
                    HashSet<string> modNames = GetModNamesFromColumn(); // Implement this method

                    bool anyCheckBoxChecked = false;

                    // Check if any checkbox is checked
                    foreach (DataGridViewRow row in dataGridView1.Rows)
                    {
                        if (row.Cells["Selection"].Value != null && (bool)row.Cells["Selection"].Value)
                        {
                            anyCheckBoxChecked = true;
                            break;
                        }
                    }

                    foreach (string modName in modNames)
                    {
                        // Reset foundMatchingFile flag for each modName
                        foundMatchingFile = false;

                        foreach (DataGridViewRow row in dataGridView1.Rows)
                        {
                            if (row.Cells["ModName"].Value?.ToString() == modName &&
                                (anyCheckBoxChecked && row.Cells["Selection"].Value != null && (bool)row.Cells["Selection"].Value ||
                                !anyCheckBoxChecked))
                            {
                                foreach (string filePath in Directory.GetFiles(folderPath))
                                {
                                    string fileName = Path.GetFileNameWithoutExtension(filePath);
                                    if (fileName.Contains(modName))
                                    {
                                        string offset = GetOffsetForModName(modName); // Implement GetOffsetForModName method
                                        string destinationPath = Path.Combine(baseDestinationPath, offset);

                                        Directory.CreateDirectory(destinationPath); // Ensure the destination directory exists
                                        MoveAndRenameFile(filePath, modName, destinationPath);
                                        foundMatchingFile = true;
                                    }
                                }
                            }
                        }
                    }
                }

                //MessageBox.Show("Files moved successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error occurred: " + ex.Message);
            }
        }



        private void RenameAndMoveFiles(string folderPath, string baseDestinationPath)
        {
            try
            {
                if (!Directory.Exists(folderPath))
                {
                    MessageBox.Show($"Source directory '{folderPath}' does not exist.");
                    return;
                }

                dataGridView1.EndEdit();

                if (string.IsNullOrEmpty(baseDestinationPath) || !Directory.Exists(baseDestinationPath))
                {
                    MessageBox.Show($"Invalid or nonexistent base destination path: {baseDestinationPath}.");
                    return;
                }

                dataGridView1.EndEdit();
                HashSet<string> selectedChunkIDs = GetSelectedChunkIDs();
                bool foundMatchingFile = false;

                foreach (string filePath in Directory.GetFiles(folderPath))
                {
                    string fileName = Path.GetFileNameWithoutExtension(filePath);
                    string[] numbers = fileName.Where(char.IsDigit)
                                               .Select(char.GetNumericValue)
                                               .Select(Convert.ToInt32)
                                               .Select(n => n.ToString())
                                               .ToArray();

                    // Check if numbers form a string of only '1's and '0's
                    if (numbers.Length == 1 && (numbers[0] == "0" || numbers[0] == "1"))
                    {
                        continue; // Skip this file
                    }

                    if (numbers.Length > 0 && numbers.Length <= 3)
                    {
                        string chunkId = string.Join("", numbers);

                        if (selectedChunkIDs.Count == 0 || selectedChunkIDs.Contains(chunkId))
                        {
                            string modName = GetModNameForChunkID(chunkId);
                            string offset = GetOffsetForChunkID(chunkId); // Retrieve Offset for moving
                            string destinationPath = Path.Combine(baseDestinationPath, offset);

                            Directory.CreateDirectory(destinationPath); // Ensure the destination directory exists
                            MoveAndRenameFile(filePath, modName, destinationPath);
                            foundMatchingFile = true;
                        }
                    }
                }

                if (!foundMatchingFile)
                {
                    HashSet<string> modNames = GetModNamesFromColumn(); // Implement this method

                    bool anyCheckBoxChecked = false;

                    // Check if any checkbox is checked
                    foreach (DataGridViewRow row in dataGridView1.Rows)
                    {
                        if (row.Cells["Selection"].Value != null && (bool)row.Cells["Selection"].Value)
                        {
                            anyCheckBoxChecked = true;
                            break;
                        }
                    }

                    foreach (string modName in modNames)
                    {
                        // Reset foundMatchingFile flag for each modName
                        foundMatchingFile = false;

                        foreach (DataGridViewRow row in dataGridView1.Rows)
                        {
                            if (row.Cells["ModName"].Value?.ToString() == modName &&
                                (anyCheckBoxChecked && row.Cells["Selection"].Value != null && (bool)row.Cells["Selection"].Value ||
                                !anyCheckBoxChecked))
                            {
                                foreach (string filePath in Directory.GetFiles(folderPath))
                                {
                                    string fileName = Path.GetFileNameWithoutExtension(filePath);
                                    if (fileName.Contains(modName))
                                    {
                                        string offset = GetOffsetForModName(modName); // Implement GetOffsetForModName method
                                        string destinationPath = Path.Combine(baseDestinationPath, offset);

                                        Directory.CreateDirectory(destinationPath); // Ensure the destination directory exists
                                        MoveAndRenameFile(filePath, modName, destinationPath);
                                        foundMatchingFile = true;
                                    }
                                }
                            }
                        }
                    }

                    if (!foundMatchingFile)
                    {
                        //MessageBox.Show("No suitable file found to rename or move.");
                    }
                    else
                    {
                        MessageBox.Show("Files renamed and moved successfully.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error occurred: " + ex.Message);
            }
        }

        private void MoveAndRenameFile(string originalFilePath, string newFileName, string destinationPath)
        {
            string newFilePath = Path.Combine(destinationPath, newFileName + Path.GetExtension(originalFilePath));
            if (File.Exists(newFilePath))
            {
                File.Delete(newFilePath); // Ensure this is desired behavior
            }
            File.Move(originalFilePath, newFilePath);
        }

        private string GetOffsetForChunkID(string chunkId)
        {
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.Cells["ChunkID"].Value?.ToString() == chunkId)
                {
                    return row.Cells["Offset"].Value?.ToString() ?? string.Empty;
                }
            }

            return string.Empty;
        }

        private string GetOffsetForModName(string modName)
        {
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.Cells["ModName"].Value?.ToString() == modName)
                {
                    return row.Cells["Offset"].Value?.ToString() ?? string.Empty;
                }
            }

            return string.Empty;
        }

        private HashSet<string> GetModNamesFromColumn()
        {
            HashSet<string> modNames = new HashSet<string>();

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                string modName = row.Cells["ModName"].Value?.ToString();
                if (!string.IsNullOrEmpty(modName))
                {
                    modNames.Add(modName);
                }
            }

            return modNames;
        }



        private void RenameFiles(string folderPath)
        {
            try
            {
                // Ensure that the source directory exists
                if (!Directory.Exists(folderPath))
                {
                    MessageBox.Show($"Source directory '{folderPath}' does not exist.");
                    return;
                }

                // Force DataGridView to end any editing mode to update its state
                dataGridView1.EndEdit();

                // Collect all selected ChunkIDs from the DataGridView
                HashSet<string> selectedChunkIDs = GetSelectedChunkIDs();

                bool foundMatchingFile = false; // Flag to track if a matching file was found

                // If no ChunkID is selected, rename files with three numbers in their names
                if (selectedChunkIDs.Count == 0)
                {
                    foreach (string filePath in Directory.GetFiles(folderPath))
                    {

                        // Extract the file name without extension
                        string fileName = Path.GetFileNameWithoutExtension(filePath);

                        // Extract numbers from the file name
                        string[] numbers = fileName.Where(char.IsDigit).Select(char.GetNumericValue).Select(Convert.ToInt32).Select(n => n.ToString()).ToArray();

                        if (numbers.Length == 1 && (numbers[0] == "0" || numbers[0] == "1"))
                        {
                            continue; // Skip this file
                        }

                        // Check if there are exactly three numbers in the file name
                        if (numbers.Length > 0 && numbers.Length <= 3)
                        {
                            // Get the ModName from the same row as ChunkID
                            string modName = GetModNameForChunkID(string.Join("", numbers));


                            // Rename the file with corresponding ModName
                            RenameFile(filePath, modName);
                            foundMatchingFile = true;
                        }
                    }
                }
                else
                {
                    // Iterate through each file in the specified folder path
                    foreach (string filePath in Directory.GetFiles(folderPath))
                    {
                        // Extract the file name without extension
                        string fileName = Path.GetFileNameWithoutExtension(filePath);

                        // Extract numbers from the file name
                        string[] numbers = fileName.Where(char.IsDigit).Select(char.GetNumericValue).Select(Convert.ToInt32).Select(n => n.ToString()).ToArray();

                        // Check if there are up to three numbers in the file name
                        if (numbers.Length > 0 && numbers.Length <= 3)
                        {
                            // Convert numbers to string
                            string chunkId = string.Join("", numbers);

                            // Check if the extracted ChunkID is in the selectedChunkIDs set
                            if (selectedChunkIDs.Contains(chunkId))
                            {
                                // Get the ModName for this ChunkID
                                string modName = GetModNameForChunkID(chunkId);

                                // Rename the file with corresponding ModName
                                RenameFile(filePath, modName);
                                foundMatchingFile = true;
                            }
                        }
                    }
                }

                if (!foundMatchingFile)
                {
                    throw new Exception("No file found with numbers in the name matching any selected ChunkID.");
                }

                MessageBox.Show("Files renamed successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error occurred: " + ex.Message);
            }
        }

        private HashSet<string> GetSelectedChunkIDs()
        {
            HashSet<string> selectedChunkIDs = new HashSet<string>();

            // Iterate through each row in the DataGridView
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                // Check if the "Selection" cell is checked and the ChunkID cell is not null
                if (row.Cells["Selection"].Value != null && (bool)row.Cells["Selection"].Value &&
                    row.Cells["ChunkID"].Value != null)
                {
                    selectedChunkIDs.Add(row.Cells["ChunkID"].Value.ToString());
                }
            }

            return selectedChunkIDs;
        }

        private string GetModNameForChunkID(string chunkId)
        {
            // Find and return the ModName for the given ChunkID from the DataGridView
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.Cells["ChunkID"].Value != null && row.Cells["ModName"].Value != null &&
                    row.Cells["ChunkID"].Value.ToString() == chunkId)
                {
                    return row.Cells["ModName"].Value.ToString();
                }
            }

            return null; // ModName not found
        }

        private void RenameFile(string filePath, string modName)
        {
            try
            {
                // Extract file extension
                string fileExtension = Path.GetExtension(filePath);

                // Create new file name with ModName and original file extension
                string newFileName = $"{modName}{fileExtension}";

                // Construct the new file path
                string newFilePath = Path.Combine(Path.GetDirectoryName(filePath), newFileName);

                // Rename the file
                File.Move(filePath, newFilePath);
            }
            catch (Exception ex)
            {
                throw new Exception("Error occurred while renaming file: " + ex.Message);
            }
        }

        private string GetFolderPathFromIni(string section)
        {
            try
            {
                string pathIniFilePath = @"C:\ProgramData\path.ini";

                var parser = new FileIniDataParser();
                IniData iniData = parser.ReadFile(pathIniFilePath);

                if (iniData.Sections.ContainsSection(section))
                {
                    KeyDataCollection pathSection = iniData[section];
                    if (pathSection.ContainsKey("FolderPath"))
                    {
                        return pathSection["FolderPath"];
                    }
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Error occurred while retrieving folder path: " + ex.Message);
            }

            return null; // Return null if path.ini or folder path is not found
        }

        private void SaveFolderPathToIni(string folderPath, string section)
        {
            try
            {
                string pathIniFilePath = @"C:\ProgramData\path.ini";

                var parser = new FileIniDataParser();
                IniData iniData;

                // If the path.ini file exists, read its content; otherwise, create a new IniData object
                if (File.Exists(pathIniFilePath))
                    iniData = parser.ReadFile(pathIniFilePath);
                else
                    iniData = new IniData();

                // Add or update the specified section with the selected folder path
                if (!iniData.Sections.ContainsSection(section))
                    iniData.Sections.AddSection(section);

                iniData[section]["FolderPath"] = folderPath;

                parser.WriteFile(pathIniFilePath, iniData);

                MessageBox.Show("Folder path saved successfully: " + folderPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error occurred while saving folder path: " + ex.Message);
            }
        }

        private void LoadDataFromIni()
        {
            string filePath = @"C:\ProgramData\data.ini";

            if (!File.Exists(filePath))
            {
                MessageBox.Show("File does not exist.");
                return;
            }

            try
            {
                var parser = new FileIniDataParser();
                IniData iniData = parser.ReadFile(filePath);

                foreach (var section in iniData.Sections)
                {
                    int rowIndex = dataGridView1.Rows.Add();
                    //dataGridView1.Rows[rowIndex].HeaderCell.Value = section.SectionName;
                    dataGridView1.Rows[rowIndex].Cells["ChunkID"].Value = section.Keys.ContainsKey("ChunkID") ? section.Keys["ChunkID"] : "";
                    dataGridView1.Rows[rowIndex].Cells["ModName"].Value = section.Keys.ContainsKey("ModName") ? section.Keys["ModName"] : "";
                    dataGridView1.Rows[rowIndex].Cells["Offset"].Value = section.Keys.ContainsKey("Offset") ? section.Keys["Offset"] : "";
                }

                //MessageBox.Show("Data loaded successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error occurred: " + ex.Message);
            }
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            LoadDataFromIni();
            label1.Text = GetFolderPathFromIni("Path");
            label2.Text = GetFolderPathFromIni("ProjectPakPath");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                using (TextWriter writer = new StreamWriter(@"C:\ProgramData\data.ini"))
                {
                    foreach (DataGridViewRow row in dataGridView1.Rows)
                    {
                        // Skip empty rows and rows with all empty cells
                        if (row.IsNewRow || row.Cells.Cast<DataGridViewCell>().All(cell => cell.Value == null || string.IsNullOrWhiteSpace(cell.Value.ToString())))
                            continue;

                        string sectionName = "Section" + (dataGridView1.Rows.IndexOf(row) + 1);

                        writer.WriteLine($"[{sectionName}]");

                        // Exclude the checkbox column and the "Selection" column
                        for (int i = 0; i < dataGridView1.Columns.Count; i++)
                        {
                            string columnName = dataGridView1.Columns[i].HeaderText;
                            if (columnName != "Selection")
                            {
                                DataGridViewCell cell = row.Cells[i];
                                // Check for null values or empty strings
                                string key = dataGridView1.Columns[cell.ColumnIndex].HeaderText;
                                string value = (cell.Value != null) ? cell.Value.ToString() : "";

                                if (!string.IsNullOrWhiteSpace(value)) // Only write non-empty values
                                    writer.WriteLine($"{key}={value}");
                            }
                        }

                        writer.WriteLine(""); // Empty line between sections
                    }
                }

                //MessageBox.Show("Data saved successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error occurred: " + ex.Message);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                using (TextWriter writer = new StreamWriter(@"C:\ProgramData\data.ini"))
                {
                    foreach (DataGridViewRow row in dataGridView1.Rows)
                    {
                        // Skip empty rows and rows with all empty cells
                        if (row.IsNewRow || row.Cells.Cast<DataGridViewCell>().All(cell => cell.Value == null || string.IsNullOrWhiteSpace(cell.Value.ToString())))
                            continue;

                        string sectionName = "Section" + (dataGridView1.Rows.IndexOf(row) + 1);

                        writer.WriteLine($"[{sectionName}]");

                        // Exclude the checkbox column and the "Selection" column
                        for (int i = 0; i < dataGridView1.Columns.Count; i++)
                        {
                            string columnName = dataGridView1.Columns[i].HeaderText;
                            if (columnName != "Selection")
                            {
                                DataGridViewCell cell = row.Cells[i];
                                // Check for null values or empty strings
                                string key = dataGridView1.Columns[cell.ColumnIndex].HeaderText;
                                string value = (cell.Value != null) ? cell.Value.ToString() : "";

                                if (!string.IsNullOrWhiteSpace(value)) // Only write non-empty values
                                    writer.WriteLine($"{key}={value}");
                            }
                        }

                        writer.WriteLine(""); // Empty line between sections
                    }
                }

                //MessageBox.Show("Data saved successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error occurred: " + ex.Message);
            }
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            using (var folderBrowserDialog = new FolderBrowserDialog())
            {
                DialogResult result = folderBrowserDialog.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(folderBrowserDialog.SelectedPath))
                {
                    string selectedPath = folderBrowserDialog.SelectedPath;
                    SaveFolderPathToIni(selectedPath, "Path"); // Save the selected folder path to path.ini
                    label1.Text = GetFolderPathFromIni("Path");
                }
            }
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            using (var folderBrowserDialog = new FolderBrowserDialog())
            {
                DialogResult result = folderBrowserDialog.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(folderBrowserDialog.SelectedPath))
                {
                    string selectedPath = folderBrowserDialog.SelectedPath;
                    SaveFolderPathToIni(selectedPath, "ProjectPakPath"); // Save the selected folder path to path.ini
                    label2.Text = GetFolderPathFromIni("ProjectPakPath");
                }
            }
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            RenameAndMoveFiles(GetFolderPathFromIni("ProjectPakPath"), GetFolderPathFromIni("Path"));
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            RenameFiles(GetFolderPathFromIni("ProjectPakPath"));
        }

        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            MoveFilesByChunkIDOrModName(GetFolderPathFromIni("ProjectPakPath"), GetFolderPathFromIni("ProjectPakPath"));
        }
    }
}

using OmnimonModuleEditor.Models;
using OmnimonModuleEditor.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;

/// <summary>
/// Dialog for editing evolution criteria.
/// </summary>
public class EvolutionCriteriaForm : Form
{
    private Evolution evolution;
    private List<string> items;

    // Input fields
    private NumericUpDown[] numConditionHearts = new NumericUpDown[2];
    private NumericUpDown[] numTraining = new NumericUpDown[2];
    private NumericUpDown[] numBattles = new NumericUpDown[2];
    private NumericUpDown[] numWinRatio = new NumericUpDown[2];
    private NumericUpDown[] numMistakes = new NumericUpDown[2];
    private NumericUpDown[] numLevel = new NumericUpDown[2];
    private NumericUpDown[] numOverfeed = new NumericUpDown[2];
    private NumericUpDown[] numSleepDisturbances = new NumericUpDown[2];
    private NumericUpDown[] numStage5 = new NumericUpDown[2];

    private NumericUpDown numArea;
    private NumericUpDown numStage;
    private NumericUpDown numVersion;
    private ComboBox cmbAttribute;
    private TextBox txtJogress;
    private CheckBox chkSpecialEncounter;
    private ComboBox cmbItem;

    private Button btnSave;
    private Button btnCancel;
    private Button btnCopy;
    private Button btnPaste;

    // Adicione o campo privado
    private CheckBox chkJogressPrefix;

    /// <summary>
    /// Initializes a new instance of the <see cref="EvolutionCriteriaForm"/> class.
    /// </summary>
    public EvolutionCriteriaForm(Evolution evo, List<Item> items)
    {
        this.evolution = evo;
        this.items = GetItemNames(items);

        InitializeComponent();
        LoadEvolution();
    }

    /// <summary>
    /// Gets the list of item names from the item list.
    /// </summary>
    private List<string> GetItemNames(List<Item> items)
    {
        if (items == null)
            return new List<string>();
        return items.Select(i => i.Name).ToList();
    }

    /// <summary>
    /// Initializes the layout and UI controls.
    /// </summary>
    private void InitializeComponent()
    {
        this.Text = Resources.EvolutionCriteriaForm_Title;
        this.Size = new Size(440, 620);
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.StartPosition = FormStartPosition.CenterParent;

        var scrollPanel = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            Padding = new Padding(8)
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 2,
            AutoSize = true
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        int row = 0;

        void AddLabel(string text)
        {
            var lbl = new Label
            {
                Text = text,
                Anchor = AnchorStyles.Left,
                AutoSize = true,
                Font = new Font("Segoe UI", 8),
                Margin = new Padding(0, 6, 0, 0)
            };
            layout.Controls.Add(lbl, 0, row);
        }

        void AddControl(Control control)
        {
            control.Margin = new Padding(0, 4, 0, 0);
            layout.Controls.Add(control, 1, row);
            row++;
        }

        void AddRange(string label, NumericUpDown[] controls, int min, int max)
        {
            AddLabel(label);
            var rangePanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                WrapContents = false
            };
            controls[0] = new NumericUpDown { Minimum = min, Maximum = max, Width = 60 };
            controls[1] = new NumericUpDown { Minimum = min, Maximum = max, Width = 60 };
            rangePanel.Controls.Add(controls[0]);
            rangePanel.Controls.Add(new Label { Text = Resources.EvolutionCriteriaForm_LabelTo, AutoSize = true, TextAlign = ContentAlignment.MiddleCenter, Padding = new Padding(4, 6, 4, 0) });
            rangePanel.Controls.Add(controls[1]);
            AddControl(rangePanel);
        }

        AddRange(Resources.EvolutionCriteriaForm_LabelConditionHearts, numConditionHearts, 0, 999999);
        AddRange(Resources.EvolutionCriteriaForm_LabelTraining, numTraining, 0, 999999);
        AddRange(Resources.EvolutionCriteriaForm_LabelBattles, numBattles, 0, 999999);
        AddRange(Resources.EvolutionCriteriaForm_LabelWinRatio, numWinRatio, 0, 100);
        AddRange(Resources.EvolutionCriteriaForm_LabelMistakes, numMistakes, 0, 999999);
        AddRange(Resources.EvolutionCriteriaForm_LabelLevel, numLevel, 0, 10);
        AddRange(Resources.EvolutionCriteriaForm_LabelOverfeed, numOverfeed, 0, 999999);
        AddRange(Resources.EvolutionCriteriaForm_LabelSleepDisturbances, numSleepDisturbances, 0, 999999);

        AddLabel(Resources.EvolutionCriteriaForm_LabelArea);
        numArea = new NumericUpDown { Minimum = 0, Maximum = 999999, Width = 60 };
        AddControl(numArea);

        AddLabel(Resources.EvolutionCriteriaForm_LabelJogressName);
        txtJogress = new TextBox { Width = 120 };
        txtJogress.TextChanged += (s, e) =>
        {
            bool enabled = !string.IsNullOrWhiteSpace(txtJogress.Text);
            numStage.Enabled = enabled;
            numVersion.Enabled = enabled;
            cmbAttribute.Enabled = enabled;
        };
        AddControl(txtJogress);

        AddLabel("Jogress Prefix");
        chkJogressPrefix = new CheckBox();
        AddControl(chkJogressPrefix);

        AddLabel(Resources.EvolutionCriteriaForm_LabelStageJogress);
        numStage = new NumericUpDown { Minimum = 0, Maximum = 999999, Width = 60 };
        AddControl(numStage);

        AddLabel(Resources.EvolutionCriteriaForm_LabelVersionJogress);
        numVersion = new NumericUpDown { Minimum = 0, Maximum = 999999, Width = 60 };
        AddControl(numVersion);

        AddLabel(Resources.EvolutionCriteriaForm_LabelAttributeJogress);
        cmbAttribute = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 80 };
        cmbAttribute.Items.AddRange(new object[] { "", "Da", "Vi", "Va" });
        AddControl(cmbAttribute);

        AddLabel(Resources.EvolutionCriteriaForm_LabelSpecialEncounter);
        chkSpecialEncounter = new CheckBox();
        AddControl(chkSpecialEncounter);

        AddRange(Resources.EvolutionCriteriaForm_LabelStage5, numStage5, 0, 999999);

        AddLabel(Resources.EvolutionCriteriaForm_LabelItem);
        cmbItem = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 150 };
        cmbItem.Items.Add(""); // none
        foreach (var item in items)
            cmbItem.Items.Add(item);
        AddControl(cmbItem);

        // Buttons
        btnSave = new Button { Text = Resources.Button_Save, Width = 90, DialogResult = DialogResult.OK };
        btnCancel = new Button { Text = Resources.Button_Cancel, Width = 90, DialogResult = DialogResult.Cancel };
        btnCopy = new Button { Text = Resources.Button_Copy, Width = 90 };
        btnPaste = new Button { Text = Resources.Button_Paste, Width = 90 };

        btnCopy.Click += (s, e) =>
        {
            try
            {
                var evoCopy = new Evolution();
                SaveToEvolution();
                CopyEvolutionFields(evolution, evoCopy);
                string json = JsonSerializer.Serialize(evoCopy, new JsonSerializerOptions { WriteIndented = false });
                Clipboard.SetText(json);
            }
            catch (Exception ex)
            {
                MessageBox.Show(Resources.EvolutionCriteriaForm_ErrorCopy + ex.Message, Resources.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        };

        btnPaste.Click += (s, e) =>
        {
            try
            {
                if (Clipboard.ContainsText())
                {
                    string json = Clipboard.GetText();
                    var evoPaste = JsonSerializer.Deserialize<Evolution>(json);
                    if (evoPaste != null)
                    {
                        string currentTo = evolution.To;
                        CopyEvolutionFields(evoPaste, evolution);
                        evolution.To = currentTo;
                        LoadEvolution();
                    }
                    else
                    {
                        MessageBox.Show(Resources.EvolutionCriteriaForm_ErrorInvalidClipboard, Resources.Error, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(Resources.EvolutionCriteriaForm_ErrorPaste + ex.Message, Resources.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        };

        var buttonPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            FlowDirection = FlowDirection.RightToLeft,
            Height = 50,
            Padding = new Padding(0, 10, 8, 8)
        };
        buttonPanel.Controls.Add(btnCancel);
        buttonPanel.Controls.Add(btnSave);
        buttonPanel.Controls.Add(btnPaste);
        buttonPanel.Controls.Add(btnCopy);

        btnSave.Click += (s, e) =>
        {
            if (SaveToEvolution())
                this.DialogResult = DialogResult.OK;
        };

        btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

        scrollPanel.Controls.Add(layout);
        this.Controls.Add(scrollPanel);
        this.Controls.Add(buttonPanel);
    }

    /// <summary>
    /// Copies all fields from one Evolution object to another (deep copy).
    /// </summary>
    private void CopyEvolutionFields(Evolution src, Evolution dest)
    {
        dest.To = src.To;
        dest.ConditionHearts = src.ConditionHearts != null ? (int[])src.ConditionHearts.Clone() : null;
        dest.Training = src.Training != null ? (int[])src.Training.Clone() : null;
        dest.Battles = src.Battles != null ? (int[])src.Battles.Clone() : null;
        dest.WinRatio = src.WinRatio != null ? (int[])src.WinRatio.Clone() : null;
        dest.Mistakes = src.Mistakes != null ? (int[])src.Mistakes.Clone() : null;
        dest.Level = src.Level != null ? (int[])src.Level.Clone() : null;
        dest.Overfeed = src.Overfeed != null ? (int[])src.Overfeed.Clone() : null;
        dest.SleepDisturbances = src.SleepDisturbances != null ? (int[])src.SleepDisturbances.Clone() : null;
        dest.Area = src.Area;
        dest.Stage = src.Stage;
        dest.Version = src.Version;
        dest.Attribute = src.Attribute;
        dest.Jogress = src.Jogress;
        dest.SpecialEncounter = src.SpecialEncounter;
        dest.Stage5 = src.Stage5 != null ? (int[])src.Stage5.Clone() : null;
        dest.Item = src.Item;
        dest.JogressPrefix = src.JogressPrefix;
    }

    /// <summary>
    /// Loads the evolution data into the form fields.
    /// </summary>
    private void LoadEvolution()
    {
        void SetRange(NumericUpDown[] ctrls, int[] val)
        {
            ctrls[0].Value = val != null && val.Length > 0 ? val[0] : 0;
            ctrls[1].Value = val != null && val.Length > 1 ? val[1] : 0;
        }
        SetRange(numConditionHearts, evolution.ConditionHearts);
        SetRange(numTraining, evolution.Training);
        SetRange(numBattles, evolution.Battles);
        SetRange(numWinRatio, evolution.WinRatio);
        SetRange(numMistakes, evolution.Mistakes);
        SetRange(numLevel, evolution.Level);
        SetRange(numOverfeed, evolution.Overfeed);
        SetRange(numSleepDisturbances, evolution.SleepDisturbances);
        SetRange(numStage5, evolution.Stage5);

        numArea.Value = evolution.Area ?? 0;
        numStage.Value = evolution.Stage ?? 0;
        numVersion.Value = evolution.Version ?? 0;
        cmbAttribute.SelectedItem = evolution.Attribute ?? "";
        txtJogress.Text = evolution.Jogress ?? "";
        chkSpecialEncounter.Checked = evolution.SpecialEncounter ?? false;
        cmbItem.SelectedItem = evolution.Item ?? "";

        // Carregue o valor:
        if (evolution.JogressPrefix == null) {
            evolution.JogressPrefix = false;
        }
        chkJogressPrefix.Checked = evolution.JogressPrefix ?? false;
    }

    /// <summary>
    /// Saves the form fields back to the evolution object.
    /// </summary>
    private bool SaveToEvolution()
    {
        int[] GetRange(NumericUpDown[] ctrls, int max)
        {
            int v0 = (int)ctrls[0].Value;
            int v1 = (int)ctrls[1].Value;
            if (v0 == 0 && v1 == 0) return null;
            if (v0 == 0 && v1 == max) return new int[] { 0, 999999 };
            return new int[] { v0, v1 };
        }

        evolution.ConditionHearts = GetRange(numConditionHearts, 999999);
        evolution.Training = GetRange(numTraining, 999999);
        evolution.Battles = GetRange(numBattles, 999999);
        evolution.WinRatio = GetRange(numWinRatio, 100);
        evolution.Mistakes = GetRange(numMistakes, 999999);
        evolution.Level = GetRange(numLevel, 10);
        evolution.Overfeed = GetRange(numOverfeed, 999999);
        evolution.SleepDisturbances = GetRange(numSleepDisturbances, 999999);
        evolution.Stage5 = GetRange(numStage5, 999999);

        evolution.Area = numArea.Value == 0 ? (int?)null : (int)numArea.Value;
        evolution.Stage = numStage.Value == 0 ? (int?)null : (int)numStage.Value;
        evolution.Version = numVersion.Value == 0 ? (int?)null : (int)numVersion.Value;
        evolution.Attribute = string.IsNullOrEmpty(cmbAttribute.Text) ? null : cmbAttribute.Text;
        evolution.Jogress = string.IsNullOrWhiteSpace(txtJogress.Text) ? null : txtJogress.Text;
        evolution.SpecialEncounter = chkSpecialEncounter.Checked ? true : (bool?)null;
        evolution.Item = string.IsNullOrEmpty(cmbItem.Text) ? null : cmbItem.Text;

        evolution.JogressPrefix = chkJogressPrefix.Checked ? true : (bool?)null;

        return true;
    }
}
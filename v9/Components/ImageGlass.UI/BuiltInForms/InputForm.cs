﻿/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2010 - 2022 DUONG DIEU PHAP
Project homepage: https://imageglass.org

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/
using ImageGlass.Base;
using System.Globalization;
using System.Text.RegularExpressions;
using Windows.Win32;
using Windows.Win32.Graphics.Dwm;
using Windows.Win32.UI.Controls;

namespace ImageGlass.UI.BuiltInForms;

public partial class InputForm : Form
{
    #region Borderless form

    private bool isAeroEnabled;
    private const int WM_NCPAINT = 0x0085;
    private const int WS_EX_NOACTIVATE = 0x8000000;
    private const int WS_EX_TOOLWINDOW = 0x00000080;


    protected override CreateParams CreateParams
    {
        get
        {
            if (DesignMode) return base.CreateParams;

            isAeroEnabled = CheckAeroEnabled();
            var cp = base.CreateParams;


            if (!isAeroEnabled)
            {
                cp.ExStyle |= WS_EX_NOACTIVATE | WS_EX_TOOLWINDOW;
            }

            return cp;
        }
    }


    protected override void WndProc(ref Message m)
    {
        switch (m.Msg)
        {
            case WM_NCPAINT:
                if (isAeroEnabled)
                {
                    unsafe
                    {
                        var value = 2;

                        PInvoke.DwmSetWindowAttribute(new(Handle),
                            DWMWINDOWATTRIBUTE.DWMWA_NCRENDERING_POLICY,
                            &value, sizeof(int));


                        var margins = new MARGINS()
                        {
                            cyTopHeight = 1,
                            cyBottomHeight = 1,
                            cxLeftWidth = 1,
                            cxRightWidth = 1,
                        };

                        PInvoke.DwmExtendFrameIntoClientArea(new(Handle), &margins);
                    }
                }
                break;

            default:
                break;
        }


        base.WndProc(ref m);

    }


    private static bool CheckAeroEnabled()
    {
        if (Environment.OSVersion.Version.Major >= 6)
        {
            PInvoke.DwmIsCompositionEnabled(out var enabled);

            return enabled ? true : false;
        }

        return false;
    }

    #endregion

    #region Borderless form moving

    private bool isMouseDown; // moving windows is taking place
    private Point lastLocation; // initial mouse position


    /// <summary>
    /// Initialize all event handlers required to manage borderless window movement.
    /// </summary>
    protected void RegisterFormEvents()
    {
        MouseDown += InputForm_MouseDown;
        MouseUp += InputForm_MouseUp;
        MouseMove += InputForm_MouseMove;

        foreach (Control control in Controls)
        {
            if (control is Label ||
                control is TableLayoutPanel ||
                control.HasChildren)
            {
                control.MouseDown += InputForm_MouseDown;
                control.MouseUp += InputForm_MouseUp;
                control.MouseMove += InputForm_MouseMove;
            }

            // child controls
            foreach (Control childControl in control.Controls)
            {
                if (childControl is Label || childControl is TableLayoutPanel)
                {
                    childControl.MouseDown += InputForm_MouseDown;
                    childControl.MouseUp += InputForm_MouseUp;
                    childControl.MouseMove += InputForm_MouseMove;
                }
            }
        }
    }

    private void InputForm_MouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Clicks == 1)
        {
            isMouseDown = true;
        }

        lastLocation = e.Location;
    }

    private void InputForm_MouseMove(object? sender, MouseEventArgs e)
    {
        if (!isMouseDown)
        {
            return; // not moving windows, ignore
        }

        Location = new Point(Location.X - lastLocation.X + e.X,
                Location.Y - lastLocation.Y + e.Y);

        Update();
    }

    private void InputForm_MouseUp(object? sender, MouseEventArgs e)
    {
        isMouseDown = false;
    }


    #endregion


    private bool _intValueOnly = false;
    private bool _unsignedIntValueOnly = false;
    private bool _floatValueOnly = false;
    private bool _unsignedFloatValueOnly = false;


    #region Public properties

    public IgTheme Theme { get; private set; }
    public IgLang Language { get; private set; }


    /// <summary>
    /// Form title
    /// </summary>
    public string Title
    {
        get => lblTitle.Text;
        set
        {
            lblTitle.Text = value;
        }
    }


    /// <summary>
    /// Description text
    /// </summary>
    public string Description
    {
        get => lblContent.Text;
        set => lblContent.Text = value;
    }

    /// <summary>
    /// Form value
    /// </summary>
    public string Value
    {
        get => txtValue.Text;
        set => txtValue.Text = value;
    }

    /// <summary>
    /// Hides or shows text input
    /// </summary>
    public bool ShowTextInput {
        get => txtValue.Visible;
        set => txtValue.Visible = value;
    }

    /// <summary>
    /// Hides or shows Shield icon for the CTA button
    /// </summary>
    public bool ShowCTAShieldIcon
    {
        get => btnOK.ShowShieldIcon;
        set => btnOK.ShowShieldIcon = value;
    }

    public string AcceptButtonText
    {
        get => btnOK.Text;
        set => btnOK.Text = value;
    }

    public string CancelButtonText
    {
        get => btnCancel.Text;
        set => btnCancel.Text = value;
    }

    /// <summary>
    /// The thumbnail of the form
    /// </summary>
    public Image? Thumbnail
    {
        get => picThumbnail.Image;
        set
        {
            picThumbnail.Image = value;
            picThumbnail.Visible = value != null;

            var columnIndex = tableMain.GetColumn(picThumbnail);

            if (value != null)
            {
                picThumbnail.Visible = true;
                picThumbnail.Width = picThumbnail.Height =
                    Math.Max(value.Width, value.Height);

                tableMain.ColumnStyles[columnIndex].SizeType = SizeType.AutoSize;
            }
            else
            {
                picThumbnail.Visible = false;
                tableMain.ColumnStyles[columnIndex].SizeType = SizeType.Absolute;
                tableMain.ColumnStyles[columnIndex].Width = 0;
            }
        }
    }

    /// <summary>
    /// Pattern for validation
    /// </summary>
    public string RegexPattern { get; set; } = "";

    /// <summary>
    /// Limit the number of characters the user can enter
    /// </summary>
    public int MaxLimit
    {
        set => txtValue.MaxLength = value;
    }

    /// <summary>
    /// Allows integer number value only
    /// </summary>
    public bool IntValueOnly
    {
        get => _intValueOnly;
        set
        {
            _intValueOnly = value;

            if (_intValueOnly)
            {
                var negativeSign = NumberFormatInfo.CurrentInfo.NegativeSign;
                var positiveSign = NumberFormatInfo.CurrentInfo.PositiveSign;

                RegexPattern = $"^[{positiveSign}{negativeSign}]?[0-9]+$";
            }
        }
    }

    /// <summary>
    /// Allows unsigned integer number only
    /// </summary>
    public bool UnsignedIntValueOnly
    {
        get => _unsignedIntValueOnly;
        set
        {
            _unsignedIntValueOnly = value;

            if (_unsignedIntValueOnly)
            {
                RegexPattern = $"^[0-9]+$";
            }
        }
    }

    /// <summary>
    /// Allows float number only
    /// </summary>
    public bool FloatValueOnly
    {
        get => _unsignedFloatValueOnly;
        set
        {
            _unsignedFloatValueOnly = value;

            if (_unsignedFloatValueOnly)
            {
                var decSeparator = NumberFormatInfo.CurrentInfo.NumberDecimalSeparator;

                RegexPattern = $"^([0-9]+([{decSeparator}][0-9]*)?|[{decSeparator}][0-9]+)$";
            }
        }
    }

    /// <summary>
    /// Allows unsigned float number only
    /// </summary>
    public bool UnsignedFloatValueOnly
    {
        get => _floatValueOnly;
        set
        {
            _floatValueOnly = value;

            if (_floatValueOnly)
            {
                var negativeSign = NumberFormatInfo.CurrentInfo.NegativeSign;
                var positiveSign = NumberFormatInfo.CurrentInfo.PositiveSign;
                var decSeparator = NumberFormatInfo.CurrentInfo.NumberDecimalSeparator;

                RegexPattern = $"^[{positiveSign}{negativeSign}]?([0-9]+([{decSeparator}][0-9]*)?|[{decSeparator}][0-9]+)$";
            }
        }
    }

    /// <summary>
    /// Allow valid filename only
    /// </summary>
    public bool FileNameValueOnly { get; set; } = false;

    #endregion


    public InputForm(IgTheme theme, IgLang lang)
    {
        InitializeComponent();
        RegisterFormEvents();

        ShowInTaskbar = false;
        lblTitle.Text = "";
        lblContent.Text = "";
        Thumbnail = null; // hide thumbnail by default

        Language = lang;
        ApplyLanguage();

        Theme = theme;
        ApplyTheme();
    }


    /// <summary>
    /// Apply language pack
    /// </summary>
    public void ApplyLanguage()
    {
        btnOK.Text = Language["_._OK"];
        btnCancel.Text = Language["_._Cancel"];
    }


    /// <summary>
    /// Apply theme to the form
    /// </summary>
    public void ApplyTheme()
    {
        lblTitle.ForeColor = 
            lblContent.ForeColor = Theme.Settings.TextColor;

        txtValue.BackColor = Theme.Settings.ToolbarBgColor;
        txtValue.ForeColor = Theme.Settings.TextColor;

        lblTitle.BackColor =
        panBottom.BackColor = Theme.Settings.ToolbarBgColor;

        BackColor = Theme.Settings.BgColor;

        if (Theme.Info.IsDark)
        {
            btnOK.DarkMode = btnCancel.DarkMode = true;
        }
    }


    /// <summary>
    /// Validate the input and show error
    /// </summary>
    /// <returns></returns>
    private bool ValidateInput()
    {
        var isValid = true;

        if (!string.IsNullOrEmpty(RegexPattern))
        {
            isValid = Regex.IsMatch(txtValue.Text, RegexPattern);
        }
        else if (FileNameValueOnly)
        {
            var badChars = Path.GetInvalidFileNameChars();

            foreach (var c in badChars)
            {
                if (txtValue.Text.Contains(c))
                {
                    isValid = false;
                    break;
                }
            }
        }

        // invalid char
        if (!isValid)
        {
            btnOK.Enabled = false;

            txtValue.BackColor = Theme.DangerColor;
        }
        else
        {
            btnOK.Enabled = true;
            txtValue.BackColor = Theme.Settings.ToolbarBgColor;
        }

        return isValid;
    }


    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        // disable parent form shotcuts
        return false;
    }


    private void InputForm_Load(object sender, EventArgs e)
    {
        txtValue.Focus();
        txtValue.SelectAll();
    }

    private void InputForm_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Escape && !e.Control && !e.Shift && !e.Alt)
        {
            btnCancel.PerformClick();
        }
    }

    private void TxtValue_TextChanged(object sender, EventArgs e)
    {
        _ = ValidateInput();
    }

    private void BtnOK_Click(object sender, EventArgs e)
    {
        if (ValidateInput())
        {
            DialogResult = DialogResult.OK;
        }
        else
        {
            txtValue.Focus();
        }
    }

    private void BtnCancel_Click(object sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
    }

}

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
using ImageGlass.Base.WinApi;
using ImageGlass.Settings;
using ImageGlass.UI;
using ImageGlass.Views;

namespace ImageGlass;

public partial class FrmCrop : ToolForm
{
    public FrmCrop(Form owner, IgTheme theme) : base(theme)
    {
        InitializeComponent();
        Owner = owner;


        ApplyTheme(Theme.Settings.IsDarkMode);
    }


    protected override void ApplyTheme(bool darkMode, BackdropStyle? style = null)
    {
        SuspendLayout();
        if (Theme != null)
        {
            EnableTransparent = darkMode = Theme.Settings.IsDarkMode;

            TableBottom.BackColor = BackColor.InvertBlackOrWhite(30);
            CmbAspectRatio.DarkMode =
                LblLocation.DarkMode =
                LblSize.DarkMode =
                LblAspectRatio.DarkMode =

                NumX.DarkMode =
                NumY.DarkMode =
                NumWidth.DarkMode =
                NumHeight.DarkMode =
                
                NumRatioFrom.DarkMode =
                NumRatioTo.DarkMode =

                BtnSettings.DarkMode =
                BtnQuickSelect.DarkMode =
                BtnReset.DarkMode =

                BtnSave.DarkMode =
                BtnSaveAs.DarkMode =
                BtnCopy.DarkMode =
                BtnCrop.DarkMode = darkMode;

            if (!darkMode)
            {
                BackColor = Color.White;
                TableBottom.BackColor = BackColor.InvertBlackOrWhite(10);
            }
        }

        ResumeLayout(false);
        base.ApplyTheme(darkMode, style);
    }


    protected override void OnRequestUpdatingColorMode(SystemColorModeChangedEventArgs e)
    {
        // update theme here
        ApplyTheme(e.IsDarkMode);
        Invalidate(true);

        base.OnRequestUpdatingColorMode(e);
    }


    protected override void OnLoad(EventArgs e)
    {
        UpdateHeight();
        LoadAspectRatioItems();

        // add control events
        Local.FrmMain.PicMain.OnSelectionChanged += PicMain_OnImageSelecting;
        Local.FrmMain.PicMain.OnImageChanged += PicMain_OnImageChanged;
        NumX.LostFocus += NumSelections_LostFocus;
        NumY.LostFocus += NumSelections_LostFocus;
        NumWidth.LostFocus += NumSelections_LostFocus;
        NumHeight.LostFocus += NumSelections_LostFocus;


        // set default location offset on the parent form
        var padding = DpiApi.Transform(10);
        var x = padding;
        var y = DpiApi.Transform(SystemInformation.CaptionHeight + Constants.TOOLBAR_ICON_HEIGHT * 2) + padding;
        InitLocation = new Point(x, y);

        base.OnLoad(e);

        ApplyLanguage();
    }


    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        base.OnFormClosing(e);

        Local.FrmMain.PicMain.OnSelectionChanged -= PicMain_OnImageSelecting;
        Local.FrmMain.PicMain.OnImageChanged -= PicMain_OnImageChanged;
        NumX.LostFocus -= NumSelections_LostFocus;
        NumY.LostFocus -= NumSelections_LostFocus;
        NumWidth.LostFocus -= NumSelections_LostFocus;
        NumHeight.LostFocus -= NumSelections_LostFocus;
    }


    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        // get hotkey list
        bool CheckHotkey(string action)
        {
            var menuHotkeyList = Config.GetHotkey(FrmMain.CurrentMenuHotkeys, action);
            var menuHotkey = menuHotkeyList.SingleOrDefault(k => k.KeyData == e.KeyData);

            return menuHotkey != null;
        }

        if (CheckHotkey(nameof(Local.FrmMain.MnuSave)))
        {
            BtnSave.PerformClick();
            return;
        }

        if (CheckHotkey(nameof(Local.FrmMain.MnuSaveAs)))
        {
            BtnSaveAs.PerformClick();
            return;
        }

        if (CheckHotkey(nameof(Local.FrmMain.MnuCopyImageData)))
        {
            BtnCopy.PerformClick();
            return;
        }
    }


    private void PicMain_OnImageSelecting(Views.SelectionEventArgs e)
    {
        NumX.Value = (decimal)e.SourceSelection.X;
        NumY.Value = (decimal)e.SourceSelection.Y;
        NumWidth.Value = (decimal)e.SourceSelection.Width;
        NumHeight.Value = (decimal)e.SourceSelection.Height;
    }


    private void PicMain_OnImageChanged(EventArgs e)
    {
        TableTop.Enabled =
            TableBottom.Enabled = Local.FrmMain.PicMain.Source != ImageSource.Null;

        UpdateAspectRatioValues();
    }


    private void CmbAspectRatio_SelectedIndexChanged(object sender, EventArgs e)
    {
        UpdateAspectRatioValues();
    }


    private void NumRatio_ValueChanged(object sender, EventArgs e)
    {
        Local.FrmMain.PicMain.SelectionAspectRatio = new SizeF((float)NumRatioFrom.Value, (float)NumRatioTo.Value);
    }


    private void NumSelections_LostFocus(object? sender, EventArgs e)
    {
        var newRect = new RectangleF(
                (float)NumX.Value,
                (float)NumY.Value,
                (float)NumWidth.Value,
                (float)NumHeight.Value);

        if (newRect != Local.FrmMain.PicMain.SourceSelection)
        {
            Local.FrmMain.PicMain.SourceSelection = newRect;
        }

        Local.FrmMain.PicMain.Invalidate();
    }


    /// <summary>
    /// Recalculate and update window height.
    /// </summary>
    private void UpdateHeight()
    {
        // calculate form height
        var contentHeight = TableTop.Height + TableTop.Padding.Vertical +
            TableBottom.Height + (TableBottom.Padding.Vertical * 2);

        Height = contentHeight;
    }


    private void ApplyLanguage()
    {
        // get hotkey string
        var saveHotkey = Config.GetHotkeyString(FrmMain.CurrentMenuHotkeys, nameof(Local.FrmMain.MnuSave));
        var saveAsHotkey = Config.GetHotkeyString(FrmMain.CurrentMenuHotkeys, nameof(Local.FrmMain.MnuSaveAs));
        var copyHotkey = Config.GetHotkeyString(FrmMain.CurrentMenuHotkeys, nameof(Local.FrmMain.MnuCopyImageData));

        // set hotkey
        TooltipMain.SetToolTip(BtnSave, saveHotkey);
        TooltipMain.SetToolTip(BtnSaveAs, saveAsHotkey);
        TooltipMain.SetToolTip(BtnCopy, copyHotkey);
    }


    private void LoadAspectRatioItems()
    {
        CmbAspectRatio.Items.Clear();

        foreach (SelectionAspectRatio arValue in Enum.GetValues(typeof(SelectionAspectRatio)))
        {
            var displayName = "";
            if (Constants.AspectRatioValue.TryGetValue(arValue, out var enumValue))
            {
                displayName = string.Join(":", enumValue);
            }
            else
            {
                var arName = Enum.GetName(typeof(SelectionAspectRatio), arValue);
                displayName = arName;
            }

            CmbAspectRatio.Items.Add(displayName);
        }

        // select item
        CmbAspectRatio.SelectedIndex = 0;
    }


    private void UpdateAspectRatioValues()
    {
        var ratio = (SelectionAspectRatio)CmbAspectRatio.SelectedIndex;
        var ratioFrom = 0;
        var ratioTo = 0;


        if (ratio == SelectionAspectRatio.Original)
        {
            var srcW = (int)Local.FrmMain.PicMain.SourceWidth;
            var srcH = (int)Local.FrmMain.PicMain.SourceHeight;

            if (srcW > 0 && srcH > 0)
            {
                var results = BHelper.SimplifyFractions(srcW, srcH);
                ratioFrom = results[0];
                ratioTo = results[1];
            }
        }

        // fill selected item to the text boxes
        else if (Constants.AspectRatioValue.TryGetValue((SelectionAspectRatio)CmbAspectRatio.SelectedIndex, out var value))
        {
            ratioFrom = value[0];
            ratioTo = value[1];
        }

        // load custom aspect ratio
        NumRatioFrom.Value = ratioFrom;
        NumRatioTo.Value = ratioTo;

        NumRatioFrom.Visible = NumRatioTo.Visible =
            ratio == SelectionAspectRatio.Original || ratio == SelectionAspectRatio.Custom;
        NumRatioFrom.Enabled = NumRatioTo.Enabled = ratio == SelectionAspectRatio.Custom;

        // adjust form size
        UpdateHeight();


        // load selection area data
        NumX.Value = (decimal)Local.FrmMain.PicMain.SourceSelection.X;
        NumY.Value = (decimal)Local.FrmMain.PicMain.SourceSelection.Y;
        NumWidth.Value = (decimal)Local.FrmMain.PicMain.SourceSelection.Width;
        NumHeight.Value = (decimal)Local.FrmMain.PicMain.SourceSelection.Height;

    }


    private void BtnSave_Click(object sender, EventArgs e)
    {
        Local.FrmMain.IG_Save();
    }


    private void BtnSaveAs_Click(object sender, EventArgs e)
    {
        Local.FrmMain.IG_SaveAs();
    }


    private void BtnCopy_Click(object sender, EventArgs e)
    {
        Local.FrmMain.IG_CopyImageData();
    }


    private void BtnCrop_Click(object sender, EventArgs e)
    {
        Local.FrmMain.IG_Crop();
    }

    private void BtnReset_Click(object sender, EventArgs e)
    {
        Local.FrmMain.PicMain.ClientSelection = new();
        Local.FrmMain.PicMain.Invalidate();
    }

    private void BtnQuickSelect_Click(object sender, EventArgs e)
    {

    }

    private void BtnSettings_Click(object sender, EventArgs e)
    {

    }
}

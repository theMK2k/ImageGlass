﻿/*
ImageGlass Project - Image viewer for Windows
Copyright (C) 2010 - 2023 DUONG DIEU PHAP
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
using ImageGlass.Settings;
using ImageGlass.UI;
using System.Dynamic;

namespace ImageGlass;


/// <summary>
/// Provides settings for Color Picker tool.
/// </summary>
public class ColorPickerConfig : IToolConfig
{
    public string ToolId { get; init; }


    /// <summary>
    /// Shows alpha value of RGB code.
    /// </summary>
    public bool ShowRgbWithAlpha { get; set; } = true;

    /// <summary>
    /// Shows alpha value of HEX code.
    /// </summary>
    public bool ShowHexWithAlpha { get; set; } = true;

    /// <summary>
    /// Shows alpha value of HSL code.
    /// </summary>
    public bool ShowHslWithAlpha { get; set; } = true;

    /// <summary>
    /// Shows alpha value of HSV code.
    /// </summary>
    public bool ShowHsvWithAlpha { get; set; } = true;



    /// <summary>
    /// Initializes new instance of <see cref="ColorPickerConfig"/>.
    /// </summary>
    public ColorPickerConfig(string toolId)
    {
        ToolId = toolId;
    }


    public void LoadFromAppConfig()
    {
        var toolConfig = Config.ToolSettings.GetValue(ToolId);
        if (toolConfig is not ExpandoObject config) return;

        // load configs
        ShowRgbWithAlpha = config.GetValue(nameof(ShowRgbWithAlpha), ShowRgbWithAlpha);
        ShowHexWithAlpha = config.GetValue(nameof(ShowHexWithAlpha), ShowHexWithAlpha);
        ShowHslWithAlpha = config.GetValue(nameof(ShowHslWithAlpha), ShowHslWithAlpha);
        ShowHsvWithAlpha = config.GetValue(nameof(ShowHsvWithAlpha), ShowHsvWithAlpha);
    }


    public void SaveToAppConfig()
    {
        var settings = new ExpandoObject();

        settings.TryAdd(nameof(ShowRgbWithAlpha), ShowRgbWithAlpha);
        settings.TryAdd(nameof(ShowHexWithAlpha), ShowHexWithAlpha);
        settings.TryAdd(nameof(ShowHslWithAlpha), ShowHslWithAlpha);
        settings.TryAdd(nameof(ShowHsvWithAlpha), ShowHsvWithAlpha);

        // save to app config
        Config.ToolSettings.Set(ToolId, settings);
    }
}
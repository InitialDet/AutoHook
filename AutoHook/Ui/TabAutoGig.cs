using AutoHook.Classes;
using AutoHook.Configurations;
using AutoHook.Utils;
using Dalamud.Interface;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoHook.Ui;
internal class TabAutoGig : TabBaseConfig
{
    public override string TabName => "AutoGig";
    public override bool Enabled => true;

    public override void DrawHeader()
    {
        ImGui.Spacing();
        ImGui.TextWrapped("This is an experimental feature and it might miss the fish. If you find it missing too much, try adjusting the SpearFishing window scale to something different");
        ImGui.Spacing();
    }

    public override void Draw()
    {
        if (DrawUtil.Checkbox("Enable AutoGig", ref Service.Configuration.AutoGigEnabled, "You can uncheck this to not use any actions below"))
        {
            if (Service.Configuration.AutoGigEnabled)
            {
                Service.Configuration.AutoGigHideOverlay = false;
                Service.Configuration.Save();

            }
        }

        if (!Service.Configuration.AutoGigEnabled)
        {
            ImGui.Indent();
            if (DrawUtil.Checkbox("Hide overlay during Spearfishing", ref Service.Configuration.AutoGigHideOverlay, "It'll only hide if the AutoGig option is disabled"))
            {
                Service.Configuration.Save();
            }

            ImGui.Unindent();

        }
    }

}

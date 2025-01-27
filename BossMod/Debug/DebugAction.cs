﻿using Dalamud.Game.Gui;
using ImGuiNET;
using System;

namespace BossMod
{
    public class DebugAction
    {
        private int _customAction = 0;

        public unsafe void DrawActionData()
        {
            ImGui.InputInt("Action to show details for", ref _customAction);
            if (_customAction != 0)
            {
                var data = Service.DataManager.GetExcelSheet<Lumina.Excel.GeneratedSheets.Action>()?.GetRow((uint)_customAction);
                if (data != null)
                {
                    ImGui.TextUnformatted($"Name: {data.Name}");
                    ImGui.TextUnformatted($"Cast time: {data.Cast100ms / 10.0:f1}");
                    ImGui.TextUnformatted($"Range: {data.Range}");
                    ImGui.TextUnformatted($"Effect range: {data.EffectRange}");
                }
            }

            var mgr = FFXIVClientStructs.FFXIV.Client.Game.ActionManager.Instance();
            var hover = Service.GameGui.HoveredAction;
            if (hover.ActionID != 0)
            {
                var mnemonic = Service.ClientState.LocalPlayer?.ClassJob.GameData?.Abbreviation.ToString();
                var rotationType = mnemonic != null ? Type.GetType($"BossMod.{mnemonic}Rotation")?.GetNestedType("AID") : null;
                ImGui.TextUnformatted($"Hover action: {hover.ActionKind} {hover.ActionID} (base={hover.BaseActionID}) ({mnemonic}: {rotationType?.GetEnumName(hover.ActionID)})");

                var (name, type) = hover.ActionKind switch
                {
                    HoverActionKind.Action => (Service.DataManager.GetExcelSheet<Lumina.Excel.GeneratedSheets.Action>()?.GetRow(hover.ActionID)?.Name, FFXIVClientStructs.FFXIV.Client.Game.ActionType.Spell),
                    HoverActionKind.GeneralAction => (Service.DataManager.GetExcelSheet<Lumina.Excel.GeneratedSheets.GeneralAction>()?.GetRow(hover.ActionID)?.Name, FFXIVClientStructs.FFXIV.Client.Game.ActionType.General),
                    _ => (null, FFXIVClientStructs.FFXIV.Client.Game.ActionType.None)
                };
                ImGui.TextUnformatted($"Name: {name}");

                if (hover.ActionKind == HoverActionKind.Action)
                {
                    ImGui.TextUnformatted($"Range: {FFXIVClientStructs.FFXIV.Client.Game.ActionManager.GetActionRange(hover.ActionID)}");
                    ImGui.TextUnformatted($"Stacks: {FFXIVClientStructs.FFXIV.Client.Game.ActionManager.GetMaxCharges(hover.ActionID, 0)}");
                    ImGui.TextUnformatted($"Adjusted ID: {mgr->GetAdjustedActionId(hover.ActionID)}");
                }

                if (type != FFXIVClientStructs.FFXIV.Client.Game.ActionType.None)
                {
                    //ImGui.TextUnformatted($"Cost: {FFXIVClientStructs.FFXIV.Client.Game.ActionManager.GetActionCost(type, hover.ActionID, 0, 0, 0, 0)}");
                    ImGui.TextUnformatted($"Status: {mgr->GetActionStatus(type, hover.ActionID)}");
                    ImGui.TextUnformatted($"Adjusted recast: {FFXIVClientStructs.FFXIV.Client.Game.ActionManager.GetAdjustedRecastTime(type, hover.ActionID):f2}");
                    ImGui.TextUnformatted($"Adjusted cast: {FFXIVClientStructs.FFXIV.Client.Game.ActionManager.GetAdjustedCastTime(type, hover.ActionID):f2}");
                    ImGui.TextUnformatted($"Recast: {mgr->GetRecastTime(type, hover.ActionID):f2}");
                    ImGui.TextUnformatted($"Recast elapsed: {mgr->GetRecastTimeElapsed(type, hover.ActionID):f2}");
                    ImGui.TextUnformatted($"Recast active: {mgr->IsRecastTimerActive(type, hover.ActionID)}");
                    var groupID = mgr->GetRecastGroup((int)type, hover.ActionID);
                    ImGui.TextUnformatted($"Recast group: {groupID}");
                    var group = mgr->GetRecastGroupDetail(groupID);
                    if (group != null)
                        ImGui.TextUnformatted($"Recast group details: active={group->IsActive}, action={group->ActionID}, elapsed={group->Elapsed}, total={group->Total}");
                }
            }
            else if (Service.GameGui.HoveredItem != 0)
            {
                uint itemID = (uint)Service.GameGui.HoveredItem % 1000000;
                bool isHQ = Service.GameGui.HoveredItem / 1000000 > 0;
                ImGui.TextUnformatted($"Hover item: {Service.GameGui.HoveredItem}");
                ImGui.TextUnformatted($"Name: {Service.DataManager.GetExcelSheet<Lumina.Excel.GeneratedSheets.Item>()?.GetRow(itemID)?.Name}{(isHQ ? " (HQ)" : "")}");
                ImGui.TextUnformatted($"Count: {FFXIVClientStructs.FFXIV.Client.Game.InventoryManager.Instance()->GetInventoryItemCount(itemID, isHQ, false, false)}");
                ImGui.TextUnformatted($"Status: {mgr->GetActionStatus(FFXIVClientStructs.FFXIV.Client.Game.ActionType.Item, itemID)}");
                ImGui.TextUnformatted($"Adjusted recast: {FFXIVClientStructs.FFXIV.Client.Game.ActionManager.GetAdjustedRecastTime(FFXIVClientStructs.FFXIV.Client.Game.ActionType.Item, itemID):f2}");
                ImGui.TextUnformatted($"Adjusted cast: {FFXIVClientStructs.FFXIV.Client.Game.ActionManager.GetAdjustedCastTime(FFXIVClientStructs.FFXIV.Client.Game.ActionType.Item, itemID):f2}");
                ImGui.TextUnformatted($"Recast: {mgr->GetRecastTime(FFXIVClientStructs.FFXIV.Client.Game.ActionType.Item, itemID):f2}");
                ImGui.TextUnformatted($"Recast elapsed: {mgr->GetRecastTimeElapsed(FFXIVClientStructs.FFXIV.Client.Game.ActionType.Item, itemID):f2}");
                ImGui.TextUnformatted($"Recast active: {mgr->IsRecastTimerActive(FFXIVClientStructs.FFXIV.Client.Game.ActionType.Item, itemID)}");
                var groupID = mgr->GetRecastGroup(2, itemID);
                ImGui.TextUnformatted($"Recast group: {groupID}");
                var group = mgr->GetRecastGroupDetail(groupID);
                if (group != null)
                    ImGui.TextUnformatted($"Recast group details: active={group->IsActive}, action={group->ActionID}, elapsed={group->Elapsed}, total={group->Total}");
            }
            else
            {
                ImGui.TextUnformatted("Hover: none");
            }
        }
    }
}

using Dalamud.Data;
using Dalamud.Interface;
using Dalamud.Logging;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using Lumina.Data.Files;
using Lumina.Excel.GeneratedSheets;
using Action = Lumina.Excel.GeneratedSheets.Action;
using Companion = Lumina.Excel.GeneratedSheets.Companion;
using DelvUI.Enums;
using ImGuiScene;

namespace DelvUI.Helpers
{
    public class LastUsedCast
    {
        private dynamic _lastUsedAction;
        private readonly BattleChara.CastInfo _castInfo;
        public readonly ActionType ActionType;
        private readonly DataManager _dataManager;
        private readonly UiBuilder _uiBuilder;
        public readonly uint CastId;
        private TexFile _icon;
        public string ActionText;
        public DamageType DamageType;
        public TextureWrap IconTexture;
        public bool HasIcon;
        public bool Interruptable;

        public LastUsedCast(uint castId, ActionType actionType, BattleChara.CastInfo castInfo, DataManager dataManager, UiBuilder uiBuilder)
        {
            CastId = castId;
            ActionType = actionType;
            _dataManager = dataManager;
            _uiBuilder = uiBuilder;
            _castInfo = castInfo;
            SetCastProperties();
            LoadAndCacheTexture();
            PluginLog.Log("Loaded new icon");
        }

        private void LoadAndCacheTexture()
        {
            HasIcon = false;
            if (_icon?.FilePath.Path == "ui/icon/000000/000000.tex" || _icon == null) return;
            IconTexture = _uiBuilder.LoadImageRaw(_icon.GetRgbaImageData(), _icon.Header.Width, _icon.Header.Height, 4);
            HasIcon = true;
        }

        private void SetCastProperties()
        {
            _lastUsedAction = null;
            Interruptable = _castInfo.Interruptible > 0;
            if (CastId == 1) {
                ActionText = "Interacting...";
                _icon = _dataManager.GetIcon(0);
                return;
            }
            ActionText = "Casting";
            _icon = _dataManager.GetIcon(0);
            
            switch (ActionType)
            {
                case ActionType.PetAction:
                case ActionType.Spell:
                case ActionType.SquadronAction:
                case ActionType.PvPAction:
                case ActionType.CraftAction:
                case ActionType.Ability:
                    _lastUsedAction = _dataManager.GetExcelSheet<Action>()?.GetRow(CastId);
                    ActionText = _lastUsedAction?.Name.ToString();
                    _icon = _dataManager.GetIcon(_lastUsedAction?.Icon ?? 0);
                    DamageType = GetDamageType(_lastUsedAction);
                    break;
                case ActionType.Mount:
                    _lastUsedAction = _dataManager.GetExcelSheet<Mount>()?.GetRow(CastId);
                    ActionText = _lastUsedAction?.Singular.ToString();
                    _icon = _dataManager.GetIcon(_lastUsedAction?.Icon ?? 0);
                    DamageType = DamageType.Unknown;
                    break;
                case ActionType.KeyItem:
                case ActionType.Item:
                    _lastUsedAction = _dataManager.GetExcelSheet<Item>()?.GetRow(CastId);
                    ActionText = ActionText != "" ? _lastUsedAction?.Name.ToString() : "Using item...";
                    _icon = _dataManager.GetIcon(_lastUsedAction?.Icon ?? 0);
                    DamageType = DamageType.Unknown;
                    break;
                case ActionType.Companion:
                    _lastUsedAction = _dataManager.GetExcelSheet<Companion>()?.GetRow(CastId);
                    ActionText = _lastUsedAction?.Singular.ToString();
                    _icon = _dataManager.GetIcon(_lastUsedAction?.Icon ?? 0);
                    DamageType = DamageType.Unknown;
                    break;
                case ActionType.None:
                case ActionType.General:
                case ActionType.Unk_7:
                case ActionType.Unk_8:
                case ActionType.MainCommand:
                case ActionType.Waymark:
                case ActionType.ChocoboRaceAbility:
                case ActionType.ChocoboRaceItem:
                case ActionType.Unk_12:
                case ActionType.Unk_18:
                case ActionType.Accessory:
                    _lastUsedAction = null;
                    ActionText = "Casting...";
                    _icon = _dataManager.GetIcon(_lastUsedAction?.Icon ?? 0);
                    DamageType = DamageType.Unknown;
                    break;
                default:
                    _lastUsedAction = null;
                    ActionText = "Casting...";
                    _icon = _dataManager.GetIcon(_lastUsedAction?.Icon ?? 0);
                    DamageType = DamageType.Unknown;
                    break;
            }
        }

        private static DamageType GetDamageType(Action action)
        {
            var damageType = (DamageType) action.AttackType.Row;
            if (damageType != DamageType.Magic && damageType != DamageType.Darkness && damageType != DamageType.Unknown)
                damageType = DamageType.Physical;
            return damageType;
        }
    }
}
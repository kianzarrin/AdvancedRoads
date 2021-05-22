namespace AdaptiveRoads.UI.RoadEditor {
    using ColossalFramework.UI;
    using KianCommons;
    using KianCommons.UI;
    using System;
    using UnityEngine;
    using AdaptiveRoads.Util;
    using System.Reflection;
    using System.Linq;
    using KianCommons.UI.Helpers;
    using AdaptiveRoads.Manager;
    using static KianCommons.ReflectionHelpers;
    using KianCommons.Plugins;

    internal struct FlagDataT {
        public delegate void SetHandlerD(IConvertible flag);
        public delegate IConvertible GetHandlerD();
        internal readonly Type EnumType;
        public readonly SetHandlerD SetValue;
        public readonly GetHandlerD GetValue;
        public readonly TypeCode UnderlyingType;
        public FlagDataT(SetHandlerD setValue, GetHandlerD getValue, Type enumType) {
            SetValue = setValue;
            GetValue = getValue;
            EnumType = enumType;
            UnderlyingType = getValue().GetTypeCode();
            Assertion.Assert(
                UnderlyingType == TypeCode.Int64 || UnderlyingType == TypeCode.Int32,
                $"bad enum type:{enumType}, Underlying Type:{UnderlyingType}");
            Assertion.Assert(enumType.IsEnum, "isEnum");
            Assertion.Equal(
                Type.GetTypeCode(Enum.GetUnderlyingType(enumType)),
                UnderlyingType,
                "underlaying types mismatch");
        }

        public long GetValueLong() {
            var flag = GetValue();
            switch (UnderlyingType) {
                case TypeCode.Int32: return (long)(uint)(int)flag;
                case TypeCode.Int64: return (long)flag;
                default: throw new Exception("unreachable code");
            }
        }

        public void SetValueLong(long flag) {
            switch (UnderlyingType) {
                case TypeCode.Int32:
                    SetValue((int)flag);
                    break;
                case TypeCode.Int64:
                    SetValue(flag);
                    break;
                default: throw new Exception("unreachable code");
            }
        }
    }

    public class BitMaskPanel : UIPanel, IDataUI {
        internal FlagDataT FlagData;

        public UILabel Label;
        public UICheckboxDropDown DropDown;

        public string Hint;
        public event REPropertySet.PropertyChangedHandler EventPropertyChanged;

        public override void OnDestroy() {
            ReflectionHelpers.SetAllDeclaredFieldsToNull(this);
            base.OnDestroy();
        }

        internal static BitMaskPanel Add(
            RoadEditorPanel roadEditorPanel,
            UIComponent container,
            string label,
            string hint,
            FlagDataT flagData) {
            try {
                Log.Debug($"BitMaskPanel.Add(container:{container}, label:{label}, enumType:{flagData.EnumType})");
                var subPanel = UIView.GetAView().AddUIComponent(typeof(BitMaskPanel)) as BitMaskPanel;
                subPanel.FlagData = flagData;
                subPanel.Initialize();
                subPanel.Label.text = label + ":";
                subPanel.Hint = hint;
                //if (dark)
                //    subPanel.opacity = 0.1f;
                //else
                //    subPanel.opacity = 0.3f;

                container.AttachUIComponent(subPanel.gameObject);
                roadEditorPanel.FitToContainer(subPanel);
                subPanel.EventPropertyChanged += roadEditorPanel.OnObjectModified;

                return subPanel;
            }catch(Exception ex) {
                Log.Exception(ex);
                return null;
            }
        }

        public override void Awake() {
            base.Awake();
            size = new Vector2(370, 54);
            atlas = TextureUtil.Ingame;
            //backgroundSprite = "GenericPanelWhite";
            //color = Color.white;

            Label = AddUIComponent<UILabel>();
            Label.relativePosition = new Vector2(0, 6);

            DropDown = AddUIComponent<UICheckboxDropDown>();
            EditorMultiSelectDropDown.Init(DropDown);
            DropDown.relativePosition = new Vector2(width - DropDown.width, 28);
            DropDown.eventAfterDropdownClose += DropdownClose;
        }

        private void Initialize() {
            //Disable();
            Populate(DropDown, FlagData.GetValueLong(), FlagData.EnumType);
            UpdateText();
            Enable();
        }

        public void Refresh() => Initialize();

        public static void Populate(UICheckboxDropDown dropdown, long flags, Type enumType) {
            var values = EnumBitMaskExtensions.GetPow2Values(enumType);
            foreach (IConvertible flag in values) {
                bool hasFlag = (flags & flag.ToInt64()) != 0;

                var itemInfo = enumType.GetEnumMemberInfo(flag);
                bool hide = itemInfo.HasAttribute<HideAttribute>();
                hide &= ModSettings.HideIrrelavant;
                hide &= !hasFlag;
                if (hide)
                    continue; // hide

                dropdown.AddItem(
                    item: Enum.GetName(enumType, flag),
                    isChecked: hasFlag,
                    userData: flag);
            }
        }

        public override void Start() {
            base.Start();
            UIButton button = DropDown.triggerButton as UIButton;
        }

        private void DropdownClose(UICheckboxDropDown checkboxdropdown) {
            SetValue(GetCheckedFlags());
            UpdateText();
            UIButton button = DropDown.triggerButton as UIButton;
        }

        // apply checked flags from UI to prefab
        protected void SetValue(long value) {
            if (FlagData.GetValueLong() != value) {
                FlagData.SetValueLong(value);
                
                EventPropertyChanged?.Invoke();
            }
        }

        // get checked flags in UI
        private long GetCheckedFlags() {
            long ret = 0;
            for (int i = 0; i < DropDown.items.Length; i++) {
                if (DropDown.GetChecked(i)) {
                    ret |= (DropDown.GetItemUserData(i) as IConvertible).ToInt64();
                }
            }
            return ret;
        }

        private string ToText(IConvertible value) =>
            Enum.Format(enumType: FlagData.EnumType, value: value, format: "G");

        private void UpdateText() {
            var flags = FlagData.GetValue();
            string text = ToText(flags);
            ApplyText(DropDown, text);
        }

        // private UIFontRenderer ObtainTextRenderer()
        static MethodInfo mObtainTextRenderer = GetMethod(typeof(UIButton), "ObtainTextRenderer");
        static UIFontRenderer ObtainTextRenderer(UIButton button) =>
            mObtainTextRenderer.Invoke(button, null) as UIFontRenderer;

        public static void ApplyText(UICheckboxDropDown dd, string text) {
            UIButton uibutton = (UIButton)dd.triggerButton;
            var padding = uibutton.textPadding;
            padding.left = 5;
            padding.right = 21;

            uibutton.text = text; // must set text to mearure text once and only once.

            using (UIFontRenderer uifontRenderer = ObtainTextRenderer(uibutton)) {
                float p2uRatio = uibutton.GetUIView().PixelsToUnits();
                var widths = uifontRenderer.GetCharacterWidths(text);
                float x = widths.Sum() / p2uRatio;
                //Log.Debug($"{uifontRenderer}.GetCharacterWidths(\"{text}\")->{widths.ToSTR()}");
                //if (x > uibutton.width - 42) 
                //    uibutton.textHorizontalAlignment = UIHorizontalAlignment.Left;
                //else
                //    uibutton.textHorizontalAlignment = UIHorizontalAlignment.Center;

                if (x > uibutton.width - uibutton.textPadding.horizontal) {
                    for (int n = 4; n < text.Length; ++n) {
                        float x2 = widths.Take(n).Sum() / p2uRatio + 15; // 15 = width of ...
                        if (x2 > uibutton.width - 21) {
                            text = text.Substring(0, n - 1) + "...";
                            break;
                        }
                    }

                }
            }
            uibutton.text = text;
        }


        [FPSBoosterSkipOptimizations]
        public override void Update() {
            isVisible = isVisible;
            base.Update();
            if (IsHovered())
                color = Color.blue;
            else
                color = Color.white;
        }

        public bool IsHovered() {
            if (containsMouse)
                return true;
            if (DropDown.GetHoverIndex() >= 0)
                return true;
            return false;
        }

        public string GetHint() {
            int i = DropDown.GetHoverIndex();
            if (i >= 0) {
                Enum flag = DropDown.GetItemUserData(i) as Enum;
                return flag.GetEnumMemberInfo().GetHints().JoinLines();
            } else if (DropDown.containsMouse || Label.containsMouse) {
                return Hint;
            } else {
                return null;
            }
        }
    }
}

using ColossalFramework.UI;
using KianCommons;
using KianCommons.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using AdaptiveRoads.Patches.RoadEditor;
using System.Reflection;
using KianCommons.UI.Helpers;
using AdaptiveRoads.Manager;

namespace AdaptiveRoads.UI.RoadEditor.MenuStyle {
    public class MenuTextField : UITextField {
        public override void Awake() {
            base.Awake();
            height = 35;
            textScale = 1.3f;
            padding = new RectOffset(10, 10, 5, 2);
            horizontalAlignment = UIHorizontalAlignment.Left;

            color = new Color32(58, 88, 104, 255);
            textColor = new Color32(174, 197, 211, 255);
            selectionBackgroundColor = new Color32(0, 171, 234, 255);

            atlas = TextureUtil.Ingame;
            selectionSprite = "EmptySprite";
            normalBgSprite = "TextFieldPanel";

            builtinKeyNavigation = true;
        }
        public override void Start() {
            base.Start();
            eventTooltipTextChanged += (_, __) => RefreshTooltip();
        }
    }
}


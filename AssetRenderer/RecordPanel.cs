using UnityEngine;
using UniverseLib.UI;
using UniverseLib.UI.Panels;

namespace AssetRenderer
{
    public class RecordPanel: PanelBase
    {
        public override string Name { get; } = "Tween Recorder";
        public override int MinWidth { get; } = 200;
        public override int MinHeight { get; } = 400;
        public override Vector2 DefaultAnchorMin { get; }
        public override Vector2 DefaultAnchorMax { get; }
        
        public RecordPanel(UIBase owner) : base(owner)
        {
        }

        protected override void ConstructPanelContent()
        {
        }
    }
}
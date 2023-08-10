using fluXis.Game.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace fluXis.Game.Screens.Edit.Tabs.Charting.Tools.Effects;

public abstract class EffectTool : ChartingTool
{
    public abstract string Letter { get; }

    public override Drawable CreateIcon() => new Container
    {
        Child = new FluXisSpriteText
        {
            Text = Letter,
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            FontSize = 32
        }
    };
}

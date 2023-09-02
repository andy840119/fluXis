using System;
using fluXis.Game.Audio;
using fluXis.Game.Graphics.Sprites;
using fluXis.Game.Graphics.UserInterface.Color;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osuTK;

namespace fluXis.Game.Screens.Menu.UI;

public partial class MenuPlayButton : CompositeDrawable
{
    public Action Action { get; set; }

    public string Description
    {
        set
        {
            text = value;
            if (spriteText != null)
                spriteText.Text = value;
        }
    }

    [Resolved]
    private UISamples samples { get; set; }

    private Container hover;
    private FluXisSpriteText spriteText;
    private string text;

    [BackgroundDependencyLoader]
    private void load()
    {
        Height = 60;
        CornerRadius = 10;
        Masking = true;

        InternalChildren = new Drawable[]
        {
            new Container // Background
            {
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Width = .5f,
                        Colour = FluXisColors.Background2
                    },
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Width = .7f,
                        Anchor = Anchor.BottomRight,
                        Origin = Anchor.BottomRight,
                        Shear = new Vector2(-.2f, 0),
                        CornerRadius = 10,
                        Masking = true,
                        Child = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = FluXisColors.Background2
                        }
                    }
                }
            },
            hover = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Alpha = 0,
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Width = .5f
                    },
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Width = .7f,
                        Anchor = Anchor.BottomRight,
                        Origin = Anchor.BottomRight,
                        Shear = new Vector2(-.2f, 0),
                        CornerRadius = 10,
                        Masking = true,
                        Child = new Box
                        {
                            RelativeSizeAxes = Axes.Y,
                            Width = 355,
                            Shear = new Vector2(.2f, 0),
                            Anchor = Anchor.BottomRight,
                            Origin = Anchor.BottomRight
                        }
                    }
                }
            },
            new SpriteIcon
            {
                Size = new Vector2(30),
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                Margin = new MarginPadding { Left = 20 },
                Shadow = true,
                Icon = FontAwesome.Solid.Play
            },
            new FluXisSpriteText
            {
                FontSize = 30,
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.BottomLeft,
                Margin = new MarginPadding { Left = 60 },
                Y = 8,
                Shadow = true,
                Text = "Play"
            },
            spriteText = new FluXisSpriteText
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.TopLeft,
                Margin = new MarginPadding { Left = 60 },
                Colour = FluXisColors.Text2,
                Shadow = true,
                Text = text
            }
        };
    }

    protected override bool OnHover(HoverEvent e)
    {
        hover.FadeTo(.2f, 50);
        samples.Hover();
        return true;
    }

    protected override void OnHoverLost(HoverLostEvent e)
    {
        hover.FadeOut(200);
    }

    protected override bool OnClick(ClickEvent e)
    {
        Action?.Invoke();
        samples.Click();
        return true;
    }
}

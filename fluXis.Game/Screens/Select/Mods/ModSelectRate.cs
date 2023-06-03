using System;
using System.Globalization;
using fluXis.Game.Audio;
using fluXis.Game.Graphics;
using fluXis.Game.Graphics.Slider;
using fluXis.Game.Mods;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;

namespace fluXis.Game.Screens.Select.Mods;

public partial class ModSelectRate : Container
{
    [Resolved]
    private AudioClock clock { get; set; }

    public ModSelector Selector { get; set; }

    private BindableFloat rateBindable;
    private RateMod mod;

    private SpriteText rateText;
    private SpriteText multiplierText;

    [BackgroundDependencyLoader]
    private void load()
    {
        rateBindable = new BindableFloat(1f)
        {
            MinValue = 0.5f,
            MaxValue = 3f,
            Precision = 0.05f
        };

        mod = new RateMod();

        RelativeSizeAxes = Axes.X;
        AutoSizeAxes = Axes.Y;
        CornerRadius = 10;
        Masking = true;
        Children = new Drawable[]
        {
            new Box
            {
                Colour = Colour4.FromHex("#ffdb69"),
                RelativeSizeAxes = Axes.Both
            },
            new Container
            {
                RelativeSizeAxes = Axes.X,
                Height = 30,
                Padding = new MarginPadding { Vertical = 2, Horizontal = 10 },
                Child = new SpriteText
                {
                    Text = "Rate",
                    Colour = FluXisColors.TextDark,
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Font = FluXisFont.Default(22),
                }
            },
            new Container
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Padding = new MarginPadding { Horizontal = 2, Bottom = 2, Top = 30 },
                Child = new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    CornerRadius = 8,
                    Masking = true,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            Colour = FluXisColors.Background2,
                            RelativeSizeAxes = Axes.Both
                        },
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Padding = new MarginPadding(5),
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    Colour = FluXisColors.Surface,
                                    RelativeSizeAxes = Axes.Both
                                },
                                new Container
                                {
                                    RelativeSizeAxes = Axes.X,
                                    Height = 30,
                                    Padding = new MarginPadding { Vertical = 2, Horizontal = 10 },
                                    Children = new Drawable[]
                                    {
                                        rateText = new SpriteText
                                        {
                                            Anchor = Anchor.CentreLeft,
                                            Origin = Anchor.CentreLeft,
                                            Font = FluXisFont.Default(22),
                                        },
                                        multiplierText = new SpriteText
                                        {
                                            Anchor = Anchor.CentreRight,
                                            Origin = Anchor.CentreRight,
                                            Font = FluXisFont.Default(22)
                                        }
                                    }
                                },
                                new FluXisSlider<float>
                                {
                                    Bindable = rateBindable,
                                    RelativeSizeAxes = Axes.X,
                                    Step = rateBindable.Precision,
                                    Margin = new MarginPadding { Top = 35 },
                                },
                                new Container
                                {
                                    RelativeSizeAxes = Axes.X,
                                    Height = 30,
                                    Padding = new MarginPadding { Horizontal = 20 },
                                    Margin = new MarginPadding { Top = 57 },
                                    Children = new SliderTickMark[]
                                    {
                                        new() { Value = 0.5f },
                                        new() { Value = 1 },
                                        new() { Value = 2 },
                                        new() { Value = 3 }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };
    }

    protected override void LoadComplete()
    {
        rateBindable.BindValueChanged(e =>
        {
            var rate = e.NewValue;

            if (rate == 1f)
                Selector.Deselect(mod);
            else if (!Selector.SelectedMods.Contains(mod))
                Selector.Select(mod);

            mod.Rate = rate;
            clock.RateTo(rate);
            Selector.UpdateTotalMultiplier();

            rateText.Text = $"{Math.Round(rate, 2).ToString(CultureInfo.InvariantCulture)}x";

            int multiplier = (int)Math.Round(mod.ScoreMultiplier * 100) - 100;
            multiplierText.Text = $"{(multiplier > 0 ? "+" : string.Empty)}{multiplier}%";
        }, true);
    }

    private partial class SliderTickMark : Container
    {
        public float Value { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            AutoSizeAxes = Axes.Both;
            RelativePositionAxes = Axes.Both;
            X = (Value - 0.5f) / 2.5f;
            Origin = Anchor.TopCentre;

            InternalChildren = new Drawable[]
            {
                new CircularContainer
                {
                    Size = new(3),
                    Masking = true,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Child = new Box { RelativeSizeAxes = Axes.Both }
                },
                new SpriteText
                {
                    Text = $"{Value.ToString(CultureInfo.InvariantCulture)}x",
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Font = FluXisFont.Default(16),
                    Margin = new MarginPadding { Top = 5 }
                }
            };
        }
    }
}
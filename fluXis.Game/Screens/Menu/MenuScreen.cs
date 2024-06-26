using System.Linq;
using fluXis.Game.Audio;
using fluXis.Game.Configuration;
using fluXis.Game.Graphics.Background;
using fluXis.Game.Graphics.Containers;
using fluXis.Game.Graphics.Sprites;
using fluXis.Game.Graphics.UserInterface.Panel;
using fluXis.Game.Graphics.UserInterface.Text;
using fluXis.Game.Localization;
using fluXis.Game.Map;
using fluXis.Game.Online.Fluxel;
using fluXis.Game.Overlay.Auth;
using fluXis.Game.Overlay.Settings;
using fluXis.Game.Overlay.Toolbar;
using fluXis.Game.Screens.Browse;
using fluXis.Game.Screens.Edit;
using fluXis.Game.Screens.Menu.UI;
using fluXis.Game.Screens.Menu.UI.NowPlaying;
using fluXis.Game.Screens.Menu.UI.Snow;
using fluXis.Game.Screens.Menu.UI.Updates;
using fluXis.Game.Screens.Menu.UI.Visualizer;
using fluXis.Game.Screens.Multiplayer;
using fluXis.Game.Screens.Ranking;
using fluXis.Game.Screens.Select;
using fluXis.Game.UI;
using fluXis.Game.Utils.Extensions;
using fluXis.Shared.Components.Users;
using osu.Framework.Allocation;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input.Events;
using osu.Framework.Platform;
using osu.Framework.Screens;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace fluXis.Game.Screens.Menu;

public partial class MenuScreen : FluXisScreen
{
    public override float Zoom => pressedStart ? 1f : 1.2f;
    public override bool ShowToolbar => pressedStart;
    public override bool AutoPlayNext => true;

    [Resolved]
    private MapStore maps { get; set; }

    [Resolved]
    private GlobalBackground backgrounds { get; set; }

    [Resolved]
    private SettingsMenu settings { get; set; }

    [Resolved]
    private LoginOverlay login { get; set; }

    [Resolved]
    private GlobalClock clock { get; set; }

    [Resolved]
    private FluxelClient fluxel { get; set; }

    [Resolved]
    private FluXisConfig config { get; set; }

    [Resolved]
    private Toolbar toolbar { get; set; }

    [Resolved]
    private PanelContainer panels { get; set; }

    private FluXisTextFlow splashText;
    private FluXisSpriteText pressAnyKeyText;

    private ParallaxContainer visualizerContainer;
    private MenuVisualizer visualizer;
    private ParallaxContainer snowContainer;

    private bool shouldSnow => Game.CurrentSeason == Season.Winter || forceSnow.Value;

    private Bindable<bool> forceSnow;

    private Container textContainer;
    private Container buttonContainer;
    private FillFlowContainer linkContainer;
    private MenuUpdates updates;

    private Sprite logoText;
    private CircularContainer animationCircle;

    private MenuPlayButton playButton;
    private int mapCount;

    private MenuButton multiButton;
    private MenuButton rankingButton;
    private MenuButton browseButton;

    private bool pressedStart;
    private double inactivityTime;
    private const double inactivity_timeout = 60 * 1000;

    [BackgroundDependencyLoader]
    private void load(GameHost host, ISampleStore samples, TextureStore textures)
    {
        forceSnow = config.GetBindable<bool>(FluXisSetting.ForceSnow);

        InternalChildren = new Drawable[]
        {
            visualizerContainer = new ParallaxContainer
            {
                Child = visualizer = new MenuVisualizer(),
                RelativeSizeAxes = Axes.Both,
                Strength = .1f,
                Alpha = shouldSnow ? 0 : 1
            },
            new Container
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Padding = new MarginPadding(40),
                Children = new Drawable[]
                {
                    textContainer = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Alpha = 0,
                        Children = new Drawable[]
                        {
                            new FluXisSpriteText
                            {
                                Text = "fluXis",
                                FontSize = 100,
                                Shadow = true,
                                ShadowOffset = new Vector2(0, 0.04f)
                            },
                            splashText = new FluXisTextFlow
                            {
                                FontSize = 32,
                                RelativeSizeAxes = Axes.X,
                                Margin = new MarginPadding { Top = 80 },
                                Shadow = true
                            }
                        }
                    },
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Children = new Drawable[]
                        {
                            animationCircle = new CircularContainer
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Masking = true,
                                BorderColour = Color4.White,
                                BorderThickness = 20,
                                Children = new Drawable[]
                                {
                                    new Box
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        AlwaysPresent = true,
                                        Alpha = 0
                                    }
                                }
                            },
                            logoText = new Sprite
                            {
                                Texture = textures.Get("Logos/logo-text-shadow"),
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre
                            }
                        }
                    },
                    pressAnyKeyText = new FluXisSpriteText
                    {
                        Text = LocalizationStrings.MainMenu.PressAnyKey,
                        FontSize = 32,
                        Shadow = true,
                        Anchor = Anchor.BottomCentre,
                        Origin = Anchor.BottomCentre
                    },
                    new MenuNowPlaying(),
                    buttonContainer = new Container
                    {
                        AutoSizeAxes = Axes.Both,
                        Anchor = Anchor.BottomLeft,
                        Origin = Anchor.BottomLeft,
                        Alpha = 0,
                        Shear = new Vector2(-.2f, 0),
                        Margin = new MarginPadding { Left = 40 },
                        X = -200,
                        Children = new Drawable[]
                        {
                            new MenuButtonBackground { Y = 30 },
                            new MenuButtonBackground { Y = 110 },
                            new MenuButtonBackground { Y = 190 },
                            playButton = new MenuPlayButton
                            {
                                Description = LocalizationStrings.MainMenu.PlayDescription(mapCount),
                                Action = continueToPlay,
                                Width = 700
                            },
                            new SmallMenuButton
                            {
                                Icon = FontAwesome6.Solid.Gear,
                                Action = settings.ToggleVisibility,
                                Width = 90,
                                Y = 80
                            },
                            multiButton = new MenuButton
                            {
                                Text = LocalizationStrings.MainMenu.MultiplayerText,
                                Description = LocalizationStrings.MainMenu.MultiplayerDescription,
                                Icon = FontAwesome6.Solid.Users,
                                Action = continueToMultiplayer,
                                Width = 290,
                                X = 110,
                                Y = 80
                            },
                            rankingButton = new MenuButton
                            {
                                Text = LocalizationStrings.MainMenu.RankingText,
                                Description = LocalizationStrings.MainMenu.RankingDescription,
                                Icon = FontAwesome6.Solid.Trophy,
                                Action = continueToRankings,
                                Width = 280,
                                X = 420,
                                Y = 80
                            },
                            new SmallMenuButton
                            {
                                Icon = FontAwesome6.Solid.XMark,
                                Action = Game.Exit,
                                Width = 90,
                                Y = 160
                            },
                            browseButton = new MenuButton
                            {
                                Text = LocalizationStrings.MainMenu.BrowseText,
                                Description = LocalizationStrings.MainMenu.BrowseDescription,
                                Icon = FontAwesome6.Solid.Download,
                                Width = 330,
                                X = 110,
                                Y = 160,
                                Action = continueToBrowse
                            },
                            new MenuButton
                            {
                                Text = LocalizationStrings.MainMenu.EditText,
                                Description = LocalizationStrings.MainMenu.EditDescription,
                                Icon = FontAwesome6.Solid.Pen,
                                Action = () => this.Push(new EditorLoader()),
                                Width = 240,
                                X = 460,
                                Y = 160
                            }
                        }
                    },
                    new MenuGamepadTooltips
                    {
                        ButtonContainer = buttonContainer
                    },
                    updates = new MenuUpdates { X = 200 },
                    linkContainer = new FillFlowContainer
                    {
                        AutoSizeAxes = Axes.Both,
                        Anchor = Anchor.BottomRight,
                        Origin = Anchor.BottomRight,
                        Direction = FillDirection.Horizontal,
                        Spacing = new Vector2(10),
                        Alpha = 0,
                        X = 200,
                        Children = new Drawable[]
                        {
                            new MenuIconButton
                            {
                                Icon = FontAwesome6.Brands.Discord,
                                Action = () => host.OpenUrlExternally("https://discord.gg/29hMftpNq9"),
                                Text = "Discord"
                            },
                            new MenuIconButton
                            {
                                Icon = FontAwesome6.Brands.GitHub,
                                Action = () => host.OpenUrlExternally("https://github.com/TeamFluXis/fluXis"),
                                Text = "GitHub"
                            },
                            new MenuIconButton
                            {
                                Icon = FontAwesome6.Solid.EarthAmericas,
                                Action = () => host.OpenUrlExternally(fluxel.Endpoint.WebsiteRootUrl),
                                Text = "Website"
                            }
                        }
                    }
                }
            },
            snowContainer = new ParallaxContainer
            {
                RelativeSizeAxes = Axes.Both,
                Child = new MenuSnow(),
                Strength = .05f,
                Alpha = shouldSnow ? 1 : 0
            }
        };

        mapCount = maps.MapSets.Count;
        maps.CollectionUpdated += () => Schedule(() => this.TransformTo(nameof(mapCount), maps.MapSets.Count, 500, Easing.OutQuint));
    }

    protected override void LoadComplete()
    {
        base.LoadComplete();

        forceSnow.BindValueChanged(_ =>
        {
            visualizerContainer.FadeTo(shouldSnow ? 0 : 1, 600, Easing.OutQuint);
            snowContainer.FadeTo(shouldSnow ? 1 : 0, 600, Easing.OutQuint);
        });

        fluxel.User.BindValueChanged(updateButtons, true);
    }

    private void updateButtons(ValueChangedEvent<APIUser> e)
    {
        Scheduler.ScheduleIfNeeded(() =>
        {
            var enabled = e.NewValue != null;
            multiButton.Enabled.Value = enabled;
            rankingButton.Enabled.Value = enabled;
            browseButton.Enabled.Value = enabled;
        });
    }

    private void continueToPlay() => this.Push(new SelectScreen());
    private void continueToMultiplayer() => this.Push(new MultiplayerScreen());
    private void continueToRankings() => this.Push(new Rankings());
    private void continueToBrowse() => this.Push(new MapBrowser());

    private bool canPlayAnimation()
    {
        if (pressedStart) return false;

        playStartAnimation();
        return true;
    }

    private void playStartAnimation()
    {
        pressedStart = true;
        inactivityTime = 0;
        UISamples?.Select();
        randomizeSplash();
        backgrounds.Zoom = 1f;

        logoText.ScaleTo(1.1f, 800, Easing.OutQuint).FadeOut(600);
        animationCircle.TransformTo(nameof(animationCircle.BorderThickness), 20f).ResizeTo(0)
                       .TransformTo(nameof(animationCircle.BorderThickness), 0f, 1200, Easing.OutQuint).ResizeTo(400, 1000, Easing.OutQuint);

        this.Delay(800).FadeIn().OnComplete(_ =>
        {
            toolbar.ShowToolbar.Value = true;
            showMenu(true);
            login.Show();
        });

        pressAnyKeyText.FadeOut(600).MoveToY(200, 800, Easing.InQuint);
    }

    private void revertStartAnimation()
    {
        toolbar.ShowToolbar.Value = false;
        backgrounds.Zoom = 1.2f;
        hideMenu();

        logoText.Delay(800).ScaleTo(.9f).ScaleTo(1f, 800, Easing.OutQuint).FadeIn(400);
        this.Delay(800).FadeIn().OnComplete(_ => pressedStart = false);

        pressAnyKeyText.Delay(800).MoveToY(0, 800, Easing.OutQuint);
        pressAnyKeyText.FadeInFromZero(1400).Then().FadeOut(1400).Loop();
    }

    protected override bool OnKeyDown(KeyDownEvent e)
    {
        if (e.Key == Key.Escape)
        {
            panels.Content ??= new ConfirmExitPanel();
            return true;
        }

        return canPlayAnimation();
    }

    protected override bool OnMouseDown(MouseDownEvent e) => canPlayAnimation();
    protected override bool OnTouchDown(TouchDownEvent e) => canPlayAnimation();
    protected override bool OnMidiDown(MidiDownEvent e) => canPlayAnimation();

    private void showMenu(bool longer = false)
    {
        // we dont need the delay
        var delay = longer ? 0 : ENTER_DELAY;

        using (BeginDelayedSequence(delay))
        {
            var moveDuration = longer ? 1000 : MOVE_DURATION;
            var fadeDuration = longer ? 800 : FADE_DURATION;

            textContainer.MoveToX(0, moveDuration, Easing.OutQuint).FadeIn(fadeDuration);
            buttonContainer.MoveToX(0, moveDuration, Easing.OutQuint).FadeIn(fadeDuration);
            // linkContainer.MoveToX(0, moveDuration, Easing.OutQuint).FadeIn(fadeDuration);

            updates.CanShow = true;
            updates.Show(moveDuration, fadeDuration);
        }
    }

    private void hideMenu()
    {
        textContainer.MoveToX(-100, MOVE_DURATION, Easing.OutQuint).FadeOut(FADE_DURATION);
        buttonContainer.MoveToX(-100, MOVE_DURATION, Easing.OutQuint).FadeOut(FADE_DURATION);
        // linkContainer.MoveToX(200, duration, Easing.OutQuint).FadeOut(duration / 2f);

        updates.CanShow = false;
        updates.Hide();
    }

    private void randomizeSplash() => splashText.Text = MenuSplashes.RandomSplash;

    public void PreEnter()
    {
        if (config.Get<bool>(FluXisSetting.IntroTheme))
        {
            maps.CurrentMap = maps.CreateBuiltinMap(MapStore.BuiltinMap.Roundhouse).LowestDifficulty;

            // this doesn't loop perfectly and I hate it and can't do anything about it
            clock.RestartPoint = maps.CurrentMap?.Metadata.PreviewTime ?? 0;
            clock.AllowLimitedLoop = false;
            clock.Seek(0);
        }
        else // if disabled, load a random map
            maps.CurrentMap = maps.GetRandom()?.Maps.FirstOrDefault() ?? MapStore.CreateDummyMap();

        clock.Stop();
        clock.Volume = 0;

        backgrounds.AddBackgroundFromMap(maps.CurrentMapSet?.Maps.First());
    }

    public override void OnEntering(ScreenTransitionEvent e)
    {
        clock.FadeIn(500);

        clock.Start();

        if (config.Get<bool>(FluXisSetting.IntroTheme))
            clock.Seek(0);
        else
            clock.Seek(maps.CurrentMapSet?.Metadata?.PreviewTime ?? 0);

        pressAnyKeyText.FadeInFromZero(1400).Then().FadeOut(1400).Loop();
        inactivityTime = 0;

        if (!shouldSnow)
            visualizer.FadeInFromZero(2000);
    }

    public override void OnSuspending(ScreenTransitionEvent e)
    {
        this.FadeOut(300);
        hideMenu();
    }

    public override void OnResuming(ScreenTransitionEvent e)
    {
        using (BeginDelayedSequence(ENTER_DELAY))
            showMenu();

        randomizeSplash();
        this.FadeIn(300);
        inactivityTime = 0;
    }

    protected override void Update()
    {
        playButton.Description = LocalizationStrings.MainMenu.PlayDescription(mapCount);

        inactivityTime += Time.Elapsed;

        if (inactivityTime > inactivity_timeout && pressedStart)
        {
            inactivityTime = 0;
            revertStartAnimation();
        }
    }
}

using fluXis.Game.Graphics.Drawables;
using fluXis.Game.Graphics.Sprites;
using fluXis.Game.Graphics.UserInterface;
using fluXis.Game.Graphics.UserInterface.Buttons;
using fluXis.Game.Graphics.UserInterface.Buttons.Presets;
using fluXis.Game.Graphics.UserInterface.Color;
using fluXis.Game.Graphics.UserInterface.Files;
using fluXis.Game.Graphics.UserInterface.Panel;
using fluXis.Game.Online;
using fluXis.Game.Online.API.Requests.Account;
using fluXis.Game.Online.API.Requests.Users;
using fluXis.Game.Online.Fluxel;
using fluXis.Game.Overlay.Network.Tabs.Account;
using fluXis.Game.Overlay.Notifications;
using fluXis.Game.Overlay.Notifications.Tasks;
using fluXis.Game.Utils;
using fluXis.Shared.API.Parameters.Users;
using fluXis.Shared.Components.Users;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Logging;
using osuTK;

namespace fluXis.Game.Overlay.Network.Tabs;

#nullable enable

public partial class DashboardAccountTab : DashboardTab
{
    public override string Title => "Account";
    public override IconUsage Icon => FontAwesome6.Solid.Gear;
    public override DashboardTabType Type => DashboardTabType.Account;

    [Resolved]
    private FluxelClient fluxel { get; set; } = null!;

    [Resolved]
    private NotificationManager notifications { get; set; } = null!;

    [Resolved]
    private PanelContainer panels { get; set; } = null!;

    private APIEditingUser user = null!;
    private Container editContent = null!;
    private Container unsavedContent = null!;
    private LoadingIcon loadingIcon = null!;

    private DashboardAccountCategory socialsCategory = null!;
    private DashboardAccountEntry twitterEntry = null!;
    private DashboardAccountEntry youtubeEntry = null!;
    private DashboardAccountEntry twitchEntry = null!;
    private DashboardAccountEntry discordEntry = null!;

    private DashboardAccountEntry displayNameEntry = null!;
    private DashboardAccountEntry aboutmeEntry = null!;

    private bool hasUnsavedChanges;
    private bool saving;

    [BackgroundDependencyLoader]
    private void load()
    {
        Content.Children = new Drawable[]
        {
            editContent = new Container
            {
                RelativeSizeAxes = Axes.Both
            },
            unsavedContent = new CircularContainer
            {
                Width = 644,
                AutoSizeAxes = Axes.Y,
                Anchor = Anchor.BottomCentre,
                Origin = Anchor.BottomCentre,
                Margin = new MarginPadding(12),
                Masking = true,
                Alpha = 0,
                Y = 40,
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = FluXisColors.Background3
                    },
                    new FluXisSpriteText
                    {
                        Text = "There are unsaved changes.",
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        WebFontSize = 14,
                        X = 16
                    },
                    new FillFlowContainer
                    {
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Horizontal,
                        Anchor = Anchor.CentreRight,
                        Origin = Anchor.CentreRight,
                        Spacing = new Vector2(8),
                        Padding = new MarginPadding(8),
                        Children = new Drawable[]
                        {
                            new FluXisButton
                            {
                                Size = new Vector2(128, 32),
                                FontSize = FluXisSpriteText.GetWebFontSize(14),
                                Data = new CancelButtonData("Reset", reset)
                            },
                            new FluXisButton
                            {
                                Size = new Vector2(128, 32),
                                FontSize = FluXisSpriteText.GetWebFontSize(14),
                                Data = new ButtonData()
                                {
                                    Text = "Save",
                                    Action = save,
                                    Color = FluXisColors.Highlight,
                                    TextColor = FluXisColors.Background2
                                }
                            }
                        }
                    }
                }
            },
            loadingIcon = new LoadingIcon
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Alpha = 0
            }
        };
    }

    private FillFlowContainer createContent()
    {
        return new FillFlowContainer
        {
            Width = 1200,
            AutoSizeAxes = Axes.Y,
            Direction = FillDirection.Full,
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre,
            Padding = new MarginPadding { Horizontal = 50, Vertical = 20 },
            Spacing = new Vector2(50),
            Children = new Drawable[]
            {
                new Container
                {
                    Width = 1100,
                    Height = 250,
                    Children = new Drawable[]
                    {
                        new ClickableContainer
                        {
                            Size = new Vector2(250),
                            CornerRadius = 30,
                            Masking = true,
                            Child = new DrawableAvatar(user)
                            {
                                RelativeSizeAxes = Axes.Both,
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                FillMode = FillMode.Fill
                            },
                            Action = () =>
                            {
                                panels.Content = new FileSelect
                                {
                                    OnFileSelected = file =>
                                    {
                                        var notification = new TaskNotificationData
                                        {
                                            Text = "Avatar Update",
                                            TextWorking = "Uploading..."
                                        };

                                        notifications.AddTask(notification);

                                        var req = new AvatarUploadRequest(file);
                                        req.Progress += (cur, max) => notification.Progress = cur / (float)max;
                                        req.Success += res =>
                                        {
                                            notification.State = res.Status == 200 ? LoadingState.Complete : LoadingState.Failed;
                                            UserCache.TriggerAvatarUpdate(user.ID);
                                        };
                                        req.Failure += ex =>
                                        {
                                            notification.State = LoadingState.Failed;
                                            Logger.Error(ex, "Failed to upload avatar!");
                                        };
                                        fluxel.PerformRequestAsync(req);
                                    },
                                    AllowedExtensions = FluXisGame.IMAGE_EXTENSIONS
                                };
                            }
                        },
                        new ClickableContainer
                        {
                            Size = new Vector2(750, 250),
                            CornerRadius = 30,
                            Masking = true,
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopRight,
                            Child = new DrawableBanner(user)
                            {
                                RelativeSizeAxes = Axes.Both,
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                FillMode = FillMode.Fill
                            },
                            Action = () =>
                            {
                                panels.Content = new FileSelect
                                {
                                    OnFileSelected = file =>
                                    {
                                        var notif = new TaskNotificationData
                                        {
                                            Text = "Banner Update",
                                            TextWorking = "Uploading..."
                                        };

                                        notifications.AddTask(notif);

                                        var req = new BannerUploadRequest(file);
                                        req.Progress += (cur, max) => notif.Progress = cur / (float)max;
                                        req.Success += res =>
                                        {
                                            notif.State = res.Status == 200 ? LoadingState.Complete : LoadingState.Failed;
                                            UserCache.TriggerBannerUpdate(user.ID);
                                        };
                                        req.Failure += ex =>
                                        {
                                            notif.State = LoadingState.Failed;
                                            Logger.Error(ex, "Failed to upload banner!");
                                        };
                                        fluxel.PerformRequestAsync(req);
                                    },
                                    AllowedExtensions = FluXisGame.IMAGE_EXTENSIONS
                                };
                            }
                        }
                    }
                },
                new DashboardAccountCategory("Account")
                {
                    Children = new Drawable[]
                    {
                        new DashboardAccountEntry
                        {
                            Title = "Username",
                            Default = user.Username,
                            ReadOnly = true
                        },
                        new DashboardAccountEntry
                        {
                            Title = "E-Mail",
                            Default = StringUtils.CensorEmail(user.Email),
                            ReadOnly = true
                        }
                    }
                },
                new DashboardAccountCategory("Password")
                {
                    Children = new Drawable[]
                    {
                        new DashboardAccountEntry
                        {
                            Title = "New Password",
                            Default = "",
                            Placeholder = "...",
                            ReadOnly = true
                        },
                        new DashboardAccountEntry
                        {
                            Title = "Confirm Password",
                            Default = "",
                            Placeholder = "...",
                            ReadOnly = true
                        }
                    }
                },
                socialsCategory = new DashboardAccountCategory("Socials")
                {
                    Children = new Drawable[]
                    {
                        twitterEntry = new DashboardAccountEntry
                        {
                            Title = "Twitter",
                            Default = user.Socials.Twitter,
                            OnChange = updateUnsavedStatus
                        },
                        youtubeEntry = new DashboardAccountEntry
                        {
                            Title = "YouTube",
                            Default = user.Socials.YouTube,
                            OnChange = updateUnsavedStatus
                        },
                        twitchEntry = new DashboardAccountEntry
                        {
                            Title = "Twitch",
                            Default = user.Socials.Twitch,
                            OnChange = updateUnsavedStatus
                        },
                        discordEntry = new DashboardAccountEntry
                        {
                            Title = "Discord",
                            Default = user.Socials.Discord,
                            OnChange = updateUnsavedStatus
                        }
                    }
                },
                new DashboardAccountCategory("Vanity")
                {
                    Children = new Drawable[]
                    {
                        displayNameEntry = new DashboardAccountEntry
                        {
                            Title = "Display Name",
                            Placeholder = "...",
                            Default = user.DisplayName,
                            OnChange = updateUnsavedStatus
                        },
                        aboutmeEntry = new DashboardAccountEntry
                        {
                            Title = "About Me",
                            Placeholder = "...",
                            Default = user.AboutMe,
                            OnChange = updateUnsavedStatus
                        }
                    }
                }
            }
        };
    }

    private void updateUnsavedStatus()
    {
        hasUnsavedChanges = false;

        hasUnsavedChanges |= twitterEntry.Value != user.Socials.Twitter;
        hasUnsavedChanges |= youtubeEntry.Value != user.Socials.YouTube;
        hasUnsavedChanges |= twitchEntry.Value != user.Socials.Twitch;
        hasUnsavedChanges |= discordEntry.Value != user.Socials.Discord;

        hasUnsavedChanges |= displayNameEntry.Value != user.DisplayName;
        hasUnsavedChanges |= aboutmeEntry.Value != user.AboutMe;

        if (hasUnsavedChanges)
            unsavedContent.FadeIn(400).MoveToY(0, 600, Easing.OutQuint);
        else
            unsavedContent.FadeOut(400).MoveToY(40, 600, Easing.OutQuint);
    }

    private void save()
    {
        if (saving || !hasUnsavedChanges)
            return;

        saving = true;

        var req = new UserProfileUpdateRequest(user.ID, new UserProfileUpdateParameters
        {
            Twitter = getValue(user.Socials.Twitter, twitterEntry.Value),
            YouTube = getValue(user.Socials.YouTube, youtubeEntry.Value),
            Twitch = getValue(user.Socials.Twitch, twitchEntry.Value),
            Discord = getValue(user.Socials.Discord, discordEntry.Value),
            DisplayName = getValue(user.DisplayName, displayNameEntry.Value),
            AboutMe = getValue(user.AboutMe, aboutmeEntry.Value)
        });

        req.Success += res =>
        {
            user = res.Data;
            saving = false;
            updateUnsavedStatus();
        };

        req.Failure += ex =>
        {
            updateUnsavedStatus();
            saving = false;
            notifications.SendError("Failed to save changes!", ex.Message);
        };

        fluxel.PerformRequestAsync(req);

        string? getValue(string? original, string? val) => val == original ? null : val;
    }

    private void reset()
    {
        twitterEntry.Value = user.Socials.Twitter;
        youtubeEntry.Value = user.Socials.YouTube;
        twitchEntry.Value = user.Socials.Twitch;
        discordEntry.Value = user.Socials.Discord;

        displayNameEntry.Value = user.DisplayName;
        aboutmeEntry.Value = user.AboutMe;

        updateUnsavedStatus();
    }

    public override void Enter()
    {
        base.Enter();

        editContent.Clear();
        loadingIcon.FadeIn(400);

        var req = new AccountSelfRequest();
        req.Success += res =>
        {
            user = res.Data;
            editContent.Child = createContent();
            loadingIcon.FadeOut(400);
        };
        req.Failure += ex =>
        {
            loadingIcon.FadeOut(400);
            Logger.Error(ex, "Failed to get user!");
            notifications.SendError("Failed to get self!", ex.Message);
        };

        fluxel.PerformRequestAsync(req);
    }
}

using System;
using System.Linq;
using fluXis.Game.Graphics.Containers;
using fluXis.Game.Graphics.UserInterface.Color;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK;

namespace fluXis.Game.Overlay.Network.Sidebar;

public partial class DashboardSidebar : ExpandingContainer
{
    private const int padding = 5;
    private const int size_closed = 64 + padding * 2;
    private const int size_open = 200 + padding * 2;

    protected override double HoverDelay => 500;

    public Action<DashboardTab> SelectAction { get; set; }

    private FillFlowContainer<DashboardSidebarButton> content;

    [BackgroundDependencyLoader]
    private void load()
    {
        Width = 64;
        RelativeSizeAxes = Axes.Y;

        InternalChildren = new Drawable[]
        {
            new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = FluXisColors.Background3
            },
            new Container
            {
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding(padding),
                Child = content = new FillFlowContainer<DashboardSidebarButton>
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(0, padding)
                }
            }
        };
    }

    public void AddTab(DashboardTab tab)
    {
        var button = new DashboardSidebarButton
        {
            Tab = tab,
            SelectAction = SelectAction
        };

        content.Add(button);
    }

    public DashboardSidebarButton GetButton(DashboardTab tab) => content.FirstOrDefault(b => b.Tab == tab);

    protected override void LoadComplete()
    {
        base.LoadComplete();

        Expanded.BindValueChanged(expanded =>
        {
            this.ResizeWidthTo(expanded.NewValue ? size_open : size_closed, 600, Easing.OutQuint);
        }, true);
    }
}

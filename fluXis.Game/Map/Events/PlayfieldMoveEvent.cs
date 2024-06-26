using fluXis.Game.Map.Structures;
using Newtonsoft.Json;
using osu.Framework.Graphics;

namespace fluXis.Game.Map.Events;

public class PlayfieldMoveEvent : ITimedObject, IHasDuration
{
    [JsonProperty("time")]
    public double Time { get; set; }

    [JsonProperty("x")]
    public float OffsetX { get; set; }

    [JsonProperty("y")]
    public float OffsetY { get; set; }

    [JsonProperty("duration")]
    public double Duration { get; set; }

    [JsonProperty("ease")]
    public Easing Easing { get; set; } = Easing.OutQuint;
}

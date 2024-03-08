﻿using fluXis.Shared.Components.Chat;
using Newtonsoft.Json;

namespace fluXis.Shared.API.Packets.Chat;

public class ChatMessagePacket : IPacket
{
    public string ID => PacketIDs.CHAT_MESSAGE;

    #region Client2Server

    [JsonProperty("content")]
    public string? Content { get; set; }

    [JsonProperty("channel")]
    public string? Channel { get; set; }

    #endregion

    #region Server2Client

    [JsonProperty("message")]
    public IChatMessage ChatMessage { get; set; } = null!;

    #endregion

    public static ChatMessagePacket CreateC2S(string content, string channel) => new() { Content = content, Channel = channel };
    public static ChatMessagePacket CreateS2C(IChatMessage message) => new() { ChatMessage = message };
}

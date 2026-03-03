using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using MessageTypeEnum = TicketPlatFormServer.Enum.MessageType;

namespace TicketPlatFormServer.DBModel;

public partial class ChatMessage
{
    public virtual User Sender { get; set; } = null!;

    [NotMapped]
    public MessageTypeEnum Type
    {
        get => System.Enum.TryParse<MessageTypeEnum>(MessageType, true, out var result) ? result : MessageTypeEnum.TEXT;
        set => MessageType = value.ToString();
    }

    [NotMapped]
    public virtual ICollection<ChatMessageImage> Images
    {
        get => ChatMessageImages;
        set => ChatMessageImages = value;
    }
}

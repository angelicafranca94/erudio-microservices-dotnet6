using GeekShopping.MessageBus;

namespace GeekShopping.PaymentAPI.Message;

public class UpdatePaymentResultMessage : BaseMessage
{

    public long OrderId { get; set; }
    public bool Status { get; set; }
    public string Email { get; set; }
}

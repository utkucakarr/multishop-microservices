namespace MultiShop.Payment.Settings
{
    public class GarantiPosSettings
    {
        public string ApiUrl { get; set; }

        public string Mode { get; set; }

        public string TerminalId { get; set; }

        public string MerchantId { get; set; }

        public string ProvUserId { get; set; }

        public string ProvisionPassword { get; set; }

        public int CurrencyCode { get; set; }

        public string CustomerIpAddress { get; set; }

        public string CustomerEmailAddress { get; set; }
    }
}

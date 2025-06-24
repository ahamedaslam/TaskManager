namespace TaskManager.Models.Response
{
    public class Response
    {
        public Guid UniqueId { get; set; } = Guid.NewGuid();

        public int ResponseCode { get; set; } //0,1001,1002,1003,1004,1005,1006
        public string ResponseDescription { get; set; }

        public object? ResponseDatas { get; set; }
    }
}

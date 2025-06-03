namespace TaskManager.Models
{
    public class Response
    {
        public Guid UniqueId { get; set; } = Guid.NewGuid();

        public int ResponseCode { get; set; }
        public string ResponseDescription { get; set; }

        public object ResponseDatas { get; set; }
    }
}

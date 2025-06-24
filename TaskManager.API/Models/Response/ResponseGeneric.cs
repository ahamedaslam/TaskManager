namespace TaskManager.Models.Responses
{
    public class Response<T>
    {
        public Guid UniqueId { get; set; } = Guid.NewGuid();
        public int ResponseCode { get; set; }
        public string ResponseDescription { get; set; }
        public T? ResponseDatas { get; set; }
    }
}

namespace Minor.WSA.Infrastructure
{
    public class TechnicalError
    {
        public int Code { get; }
        public string Message { get; }

        public TechnicalError(int code, string message)
        {
            Code = code;
            Message = message;
        }
    }
}
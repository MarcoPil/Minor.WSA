namespace Minor.WSA.Infrastructure
{
    public interface ICommandHandler
    {
        CommandResultMessage DispatchCommand(CommandReceivedMessage commandReceivedMessage);
    }
}
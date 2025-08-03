public interface ICommand
{
    bool Execute();
    void Undo();
    int ActionPointCost { get; }
    Character Character { get; }
    string GetCommandName();
}
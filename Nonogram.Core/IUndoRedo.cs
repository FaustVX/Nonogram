namespace Nonogram
{
    public interface IUndoRedo
    {
        bool IsCorrect { get; }
        void Undo();
        void Redo();
        void Restart();
    }
}
using System.Collections.Generic;

namespace Nonogram
{
    public partial class Game<T> where T : notnull
    {
        private abstract class HistoryFrame
        {
            public class SingleCell : HistoryFrame
            {
                public int X { get; }
                public int Y { get; }
                public ICell Cell { get; }

                public SingleCell(int x, int y, Game<T> game)
                    => (X, Y, Cell) = (x, y, game[x, y]);

                public override Game<T>.HistoryFrame.SingleCell Rebuild(Game<T> game)
                    => new(X, Y, game);

                public override void Apply(Game<T> game, LinkedList<HistoryFrame> first, LinkedList<HistoryFrame> last)
                {
                    base.Apply(game, first, last);
                    game.CalculateColoredCells(X, Y, Cell);
                    game.OnCollectionChanged(X, Y, Cell);
                    var autoSeal = game.AutoSeal;
                    game._autoSeal = false;
                    game.ValidateHints(X, Y);
                    game._autoSeal = autoSeal;
                }
            }

            public abstract HistoryFrame Rebuild(Game<T> game);
            public virtual void Apply(Game<T> game, LinkedList<HistoryFrame> first, LinkedList<HistoryFrame> last)
            {
                first.RemoveLast();
                last.AddLast(Rebuild(game));
            }
        }
    }
}

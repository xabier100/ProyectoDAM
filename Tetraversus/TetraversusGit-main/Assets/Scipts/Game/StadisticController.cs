using System.Collections.Generic;

namespace Scipts.Game
{
    using System.Collections.Generic;

    namespace Scipts.Game
    {
        public sealed class StadisticController
        {
            private static readonly StadisticController _instance = new StadisticController();
            public static StadisticController Instance => _instance;

            private readonly Dictionary<Tetromino, int> _statistics = new Dictionary<Tetromino, int>();

            private StadisticController()
            {
                foreach (Tetromino t in System.Enum.GetValues(typeof(Tetromino)))
                    _statistics[t] = 0;
            }

            public void Add(Tetromino tetromino)
            {
                if (_statistics.TryGetValue(tetromino, out int current))
                    _statistics[tetromino] = current + 1;
                else
                    _statistics[tetromino] = 1;
            }

            public int Get(Tetromino tetromino)
                => _statistics.TryGetValue(tetromino, out int value) ? value : 0;
            

            public void Reset() => ResetToZero();
            
            public void ResetToZero()
            {
                foreach (Tetromino t in System.Enum.GetValues(typeof(Tetromino)))
                    _statistics[t] = 0;
            }

            public Dictionary<Tetromino, int> GetStatistics()
            {
                // Return a copy to prevent external modification
                return new Dictionary<Tetromino, int>(_statistics);
            }
        }
    }
}
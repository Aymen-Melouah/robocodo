using System;

namespace Game.Model
{
    [Serializable]
    public struct Position
    {
        #region Variables
            public int x;
            public int y;
        #endregion

        #region Methods
            public Position (int x, int y)
            {
                this.x = x;
                this.y = y;
            }

            public static Position operator + (Position a, Position b) 
                => new Position(a.x + b.x, a.y + b.y);

            public static Position operator - (Position a, Position b) 
                => new Position(a.x - b.x, a.y - b.y);

            public static bool operator == (Position a, Position b)
                => a.x == b.x && a.y == b.y;

            public static bool operator != (Position a, Position b)
                => a.x != b.x || a.y != b.y;

            public override bool Equals(object obj)
            {
                if (obj is Position other)
                {
                    return this == other;
                }
                return false;
            }

            public override int GetHashCode()
            {
                unchecked 
                {
                    int hash = 17;
                    hash = hash * 23 + x.GetHashCode();
                    hash = hash * 23 + y.GetHashCode();
                    return hash;
                }
            }
        #endregion
    }
}

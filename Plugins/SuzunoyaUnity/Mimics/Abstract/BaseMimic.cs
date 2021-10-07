using System;
using Suzunoya.Entities;

namespace SuzunoyaUnity.Mimics {
public abstract class BaseMimic : Tokenized {
    public virtual Type[] CoreTypes => new Type[0];

    public abstract void _Initialize(IEntity ent);
}
}
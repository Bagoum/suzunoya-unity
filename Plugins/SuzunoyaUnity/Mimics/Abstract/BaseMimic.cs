using System;
using Suzunoya.Entities;

namespace SuzunoyaUnity.Mimics {
public abstract class BaseMimic : Tokenized {
    public virtual Type[] CoreTypes => Type.EmptyTypes;

    public abstract void _Initialize(IEntity ent);
}
}
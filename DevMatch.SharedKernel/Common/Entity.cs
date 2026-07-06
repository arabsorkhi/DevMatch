using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevMatch.SharedKernel.Common
{
    public interface IEntity
    {
    }
    public abstract class Entity<TKey> :IEntity
        where TKey : notnull
    {
        public TKey Id { get; protected set; } = default!;

        public override bool Equals(object? obj)
        {
            if (obj is not Entity<TKey> other)
                return false;

            return EqualityComparer<TKey>.Default.Equals(Id, other.Id);
        }

        public override int GetHashCode()
        {
            return Id!.GetHashCode();
        }

        public static bool operator ==(
            Entity<TKey>? left,
            Entity<TKey>? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(
            Entity<TKey>? left,
            Entity<TKey>? right)
        {
            return !(left == right);
        }
    }
}

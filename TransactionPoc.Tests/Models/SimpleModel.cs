using System;
using System.Linq;

using LinqToDB.Mapping;

using LiteDB;

namespace TransactionPoc.Tests.Models
{
    [Table(Name = "Simple")]
    public class SimpleModel : IEquatable<SimpleModel>
    {

        [Column(Name = "Id", IsIdentity = true, IsPrimaryKey = true), BsonId]
        public int Id { get; set; }

        [Column(Name = "Name"), NotNull, BsonField("name")]
        public string Name { get; set; }

        [Column(Name = "Number"), NotNull, BsonField("number")]
        public int Number { get; set; }

        #region Equality

        public bool Equals(SimpleModel other)
        {
            if (ReferenceEquals(null, other)) { return false; }
            if (ReferenceEquals(this, other)) { return true; }
            return Id == other.Id && string.Equals(Name, other.Name) && Number == other.Number;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) { return false; }
            if (ReferenceEquals(this, obj)) { return true; }
            if (obj.GetType() != this.GetType()) { return false; }
            return Equals((SimpleModel) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Id;
                hashCode = (hashCode * 397) ^ (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Number;
                return hashCode;
            }
        }

        public static bool operator ==(SimpleModel left, SimpleModel right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(SimpleModel left, SimpleModel right)
        {
            return !Equals(left, right);
        }

        #endregion
    }
}

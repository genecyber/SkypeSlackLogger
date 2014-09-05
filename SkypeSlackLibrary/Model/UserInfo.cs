namespace SkypeSlackLibrary.Model
{
    public struct UserInfo
    {
        public string Name { get; set; }
        public Sex Sex { get; set; }

        public static bool operator ==(UserInfo i1, UserInfo i2)
        {
            return (i1.Name.Equals(i2.Name) && i1.Sex.Equals(i2.Sex));
        }

        public static bool operator !=(UserInfo i1, UserInfo i2)
        {
            return (!i1.Sex.Equals(i2.Sex) || !i1.Name.Equals(i2.Name));
        }

        public bool Equals(UserInfo other)
        {
            return Equals(other.Name, Name) && Equals(other.Sex, Sex);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj.GetType() == typeof(UserInfo) && Equals((UserInfo)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Name != null ? Name.GetHashCode() : 0) * 397) ^ Sex.GetHashCode();
            }
        }
    }
}
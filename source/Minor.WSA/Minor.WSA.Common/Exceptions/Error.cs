using System;

namespace Minor.WSA.Common
{
    public class Error : IEquatable<Error>
    {
        public string Code { get; }
        public string Message { get; }

        public Error(string code, string message)
        {
            Code = code;
            Message = message;
        }

        #region IEquatable pattern
        private static bool AreEqual(Error e1, Error e2)
        {
            return e1.Code == e2.Code &&
                   e1.Message == e2.Message;
        }
        public static bool operator==(Error left, Error right)
        {
            return AreEqual(left, right);
        }
        public static bool operator!=(Error left, Error right)
        {
            return !AreEqual(left, right);
        }
        public bool Equals(Error other)
        {
            return AreEqual(this, other);
        }
        public override bool Equals(object obj)
        {
            return obj is Error && AreEqual(this, obj as Error);
        }
        public override int GetHashCode()
        {
            return Code.GetHashCode() ^ Message.GetHashCode();
        }
        #endregion IEquatable pattern

        public override string ToString()
        {
            return $"Error(code:\"{Code}\", message:\"{Message}\")";
        }
    }
}
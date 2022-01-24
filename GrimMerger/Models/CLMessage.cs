using GrimMerger.Enums;

namespace GrimMerger.Models
{
    internal sealed class CLMessage
    {
        internal CLMessageType Type { get; }
        internal string Value { get; }
        internal string[] Arguments { get; }

        #region Constructors

        private CLMessage(string value)
        {
            Value = value;
            Type = CLMessageType.VisualToView;
        }

        private CLMessage(string value, CLMessageType type, params string[] args)
        {
            Value = value;
            Type = type;
            Arguments = args;
        }

        #endregion

        internal static CLMessage Build(string value)
        {
            return new CLMessage(value);
        }

        internal static CLMessage Build(CLMessageType type, params string[] args)
        {
            return new CLMessage(string.Empty, type, args);
        }

    }
}

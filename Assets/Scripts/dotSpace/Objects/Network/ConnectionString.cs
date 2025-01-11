using dotSpace.Enumerations;
using System;
using System.Text.RegularExpressions;

namespace dotSpace.Objects.Network
{
    /// <summary>
    /// This entity maps a valid connection string to a property based representation.
    /// </summary>
    public class ConnectionString
    {
        /////////////////////////////////////////////////////////////////////////////////////////////
        #region // Constructors

        /// <summary>
        /// Initializes a new instances of the ConnectionString class.
        /// </summary>
        public ConnectionString(string uri)
        {
            string pattern = @"^(?:(?<protocol>[a-zA-Z]+)://)?(?<host>[^:/?]+)(?::(?<port>\d+))?(?:/(?<target>[a-zA-Z0-9-]*))?(?:\?(?<mode>[A-Z]+))?$";
            Match match = Regex.Match(uri, pattern);
            if (!match.Success)
            {
                throw new ArgumentException("invalid connectio string: " + uri);
            }
            Protocol = match.Groups["protocol"].Success ? (Protocol)Enum.Parse(typeof(Protocol), match.Groups["protocol"].Value.ToUpper()) : Protocol.TCP;
            Host = match.Groups["host"].Value;
            Port = match.Groups["port"].Success ? int.Parse(match.Groups["port"].Value) : 31415;
            Target = match.Groups["target"].Success ? match.Groups["target"].Value : string.Empty;
            Mode = match.Groups["mode"].Success ? (ConnectionMode)Enum.Parse(typeof(ConnectionMode), match.Groups["mode"].Value) : ConnectionMode.KEEP;
        }

        #endregion

        /////////////////////////////////////////////////////////////////////////////////////////////
        #region // Public Properties

        /// <summary>
        /// Gets the specified protocol. This property is optional. If no value was defined, it defaults to TCP.
        /// </summary>
        public Protocol Protocol { get; private set; }

        /// <summary>
        /// Gets the specified host. 
        /// </summary>
        public string Host { get; private set; }

        /// <summary>
        /// Gets the specified port. This property is optional. If no value was defined, it defaults to 31415.
        /// </summary>
        public int Port { get; private set; }

        /// <summary>
        /// Gets the specified target space. This property is optional depending on usage.
        /// </summary>
        public string Target { get; private set; }

        /// <summary>
        /// Gets the specified connection scheme. This property is optional. If no value was defined, it defaults to KEEP.
        /// </summary>
        public ConnectionMode Mode { get; private set; }

        #endregion

        /////////////////////////////////////////////////////////////////////////////////////////////
        #region // Public Methods

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        /// <summary>
        /// Returns true if the values representing the connection string are equal.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj is ConnectionString other)
            {
                return this.Protocol == other.Protocol && this.Host.Equals(other.Host)
                       && this.Port == other.Port && this.Target.Equals(other.Target) && this.Mode == other.Mode;
            }
            return false;
        }
        
        #endregion
    }
}

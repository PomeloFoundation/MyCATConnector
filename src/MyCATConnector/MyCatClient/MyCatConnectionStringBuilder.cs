using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;

namespace MyCat.Data.MyCatClient
{
	public sealed class MyCatConnectionStringBuilder : DbConnectionStringBuilder
	{
		public MyCatConnectionStringBuilder()
		{
		}

		public MyCatConnectionStringBuilder(string connectionString)
		{
			ConnectionString = connectionString;
		}

		// Base Options
		public string Server
		{
			get { return MyCatConnectionStringOption.Server.GetValue(this); }
			set { MyCatConnectionStringOption.Server.SetValue(this, value); }
		}

		public uint Port
		{
			get { return MyCatConnectionStringOption.Port.GetValue(this); }
			set { MyCatConnectionStringOption.Port.SetValue(this, value); }
		}

		public string UserID
		{
			get { return MyCatConnectionStringOption.UserID.GetValue(this); }
			set { MyCatConnectionStringOption.UserID.SetValue(this, value); }
		}

		public string Password
		{
			get { return MyCatConnectionStringOption.Password.GetValue(this); }
			set { MyCatConnectionStringOption.Password.SetValue(this, value); }
		}

		public string Database
		{
			get { return MyCatConnectionStringOption.Database.GetValue(this); }
			set { MyCatConnectionStringOption.Database.SetValue(this, value); }
		}

		// SSL/TLS Options
		public MyCatSslMode SslMode
		{
			get { return MyCatConnectionStringOption.SslMode.GetValue(this); }
			set { MyCatConnectionStringOption.SslMode.SetValue(this, value); }
		}

		public string CertificateFile
		{
			get { return MyCatConnectionStringOption.CertificateFile.GetValue(this); }
			set { MyCatConnectionStringOption.CertificateFile.SetValue(this, value); }
		}

		public string CertificatePassword
		{
			get { return MyCatConnectionStringOption.CertificatePassword.GetValue(this); }
			set { MyCatConnectionStringOption.CertificatePassword.SetValue(this, value); }
		}

		// Connection Pooling Options
		public bool Pooling
		{
			get { return MyCatConnectionStringOption.Pooling.GetValue(this); }
			set { MyCatConnectionStringOption.Pooling.SetValue(this, value); }
		}

		public bool ConnectionReset
		{
			get { return MyCatConnectionStringOption.ConnectionReset.GetValue(this); }
			set { MyCatConnectionStringOption.ConnectionReset.SetValue(this, value); }
		}

		public uint MinimumPoolSize
		{
			get { return MyCatConnectionStringOption.MinimumPoolSize.GetValue(this); }
			set { MyCatConnectionStringOption.MinimumPoolSize.SetValue(this, value); }
		}

		public uint MaximumPoolSize
		{
			get { return MyCatConnectionStringOption.MaximumPoolSize.GetValue(this); }
			set { MyCatConnectionStringOption.MaximumPoolSize.SetValue(this, value); }
		}

		// Other Options
		public bool AllowUserVariables
		{
			get { return MyCatConnectionStringOption.AllowUserVariables.GetValue(this); }
			set { MyCatConnectionStringOption.AllowUserVariables.SetValue(this, value); }
		}

		public string CharacterSet
		{
			get { return MyCatConnectionStringOption.CharacterSet.GetValue(this); }
			set { MyCatConnectionStringOption.CharacterSet.SetValue(this, value); }
		}

		public uint ConnectionTimeout
		{
			get { return MyCatConnectionStringOption.ConnectionTimeout.GetValue(this); }
			set { MyCatConnectionStringOption.ConnectionTimeout.SetValue(this, value); }
		}

		public bool ConvertZeroDateTime
		{
			get { return MyCatConnectionStringOption.ConvertZeroDateTime.GetValue(this); }
			set { MyCatConnectionStringOption.ConvertZeroDateTime.SetValue(this, value); }
		}

		public bool ForceSynchronous
		{
			get { return MyCatConnectionStringOption.ForceSynchronous.GetValue(this); }
			set { MyCatConnectionStringOption.ForceSynchronous.SetValue(this, value); }
		}

		public uint Keepalive
		{
			get { return MyCatConnectionStringOption.Keepalive.GetValue(this); }
			set { MyCatConnectionStringOption.Keepalive.SetValue(this, value); }
		}

		public bool OldGuids
		{
			get { return MyCatConnectionStringOption.OldGuids.GetValue(this); }
			set { MyCatConnectionStringOption.OldGuids.SetValue(this, value); }
		}

		public bool PersistSecurityInfo
		{
			get { return MyCatConnectionStringOption.PersistSecurityInfo.GetValue(this); }
			set { MyCatConnectionStringOption.PersistSecurityInfo.SetValue(this, value); }
		}

		public bool TreatTinyAsBoolean
		{
			get { return MyCatConnectionStringOption.TreatTinyAsBoolean.GetValue(this); }
			set { MyCatConnectionStringOption.TreatTinyAsBoolean.SetValue(this, value); }
		}

		public bool UseAffectedRows
		{
			get { return MyCatConnectionStringOption.UseAffectedRows.GetValue(this); }
			set { MyCatConnectionStringOption.UseAffectedRows.SetValue(this, value); }
		}

		public bool UseCompression
		{
			get { return MyCatConnectionStringOption.UseCompression.GetValue(this); }
			set { MyCatConnectionStringOption.UseCompression.SetValue(this, value); }
		}

		// Other Methods
		public override bool ContainsKey(string key)
		{
			var option = MyCatConnectionStringOption.TryGetOptionForKey(key);
			return option != null && base.ContainsKey(option.Key);
		}

		public override bool Remove(string key)
		{
			var option = MyCatConnectionStringOption.TryGetOptionForKey(key);
			return option != null && base.Remove(option.Key);
		}

		public override object this[string key]
		{
			get { return MyCatConnectionStringOption.GetOptionForKey(key).GetObject(this); }
			set { base[MyCatConnectionStringOption.GetOptionForKey(key).Key] = Convert.ToString(value, CultureInfo.InvariantCulture); }
		}

		internal string GetConnectionString(bool includePassword)
		{
			var connectionString = ConnectionString;
			if (includePassword)
				return connectionString;

			if (m_cachedConnectionString != connectionString)
			{
				var csb = new MyCatConnectionStringBuilder(connectionString);
				foreach (string key in Keys)
					foreach (var passwordKey in MyCatConnectionStringOption.Password.Keys)
						if (string.Equals(key, passwordKey, StringComparison.OrdinalIgnoreCase))
							csb.Remove(key);
				m_cachedConnectionStringWithoutPassword = csb.ConnectionString;
				m_cachedConnectionString = connectionString;
			}

			return m_cachedConnectionStringWithoutPassword;
		}

		string m_cachedConnectionString;
		string m_cachedConnectionStringWithoutPassword;
	}

	internal abstract class MyCatConnectionStringOption
	{
		// Base Options
		public static readonly MyCatConnectionStringOption<string> Server;
		public static readonly MyCatConnectionStringOption<uint> Port;
		public static readonly MyCatConnectionStringOption<string> UserID;
		public static readonly MyCatConnectionStringOption<string> Password;
		public static readonly MyCatConnectionStringOption<string> Database;

		// SSL/TLS Options
		public static readonly MyCatConnectionStringOption<MyCatSslMode> SslMode;
		public static readonly MyCatConnectionStringOption<string> CertificateFile;
		public static readonly MyCatConnectionStringOption<string> CertificatePassword;

		// Connection Pooling Options
		public static readonly MyCatConnectionStringOption<bool> Pooling;
		public static readonly MyCatConnectionStringOption<bool> ConnectionReset;
		public static readonly MyCatConnectionStringOption<uint> MinimumPoolSize;
		public static readonly MyCatConnectionStringOption<uint> MaximumPoolSize;

		// Other Options
		public static readonly MyCatConnectionStringOption<bool> AllowUserVariables;
		public static readonly MyCatConnectionStringOption<string> CharacterSet;
		public static readonly MyCatConnectionStringOption<uint> ConnectionTimeout;
		public static readonly MyCatConnectionStringOption<bool> ConvertZeroDateTime;
		public static readonly MyCatConnectionStringOption<bool> ForceSynchronous;
		public static readonly MyCatConnectionStringOption<uint> Keepalive;
		public static readonly MyCatConnectionStringOption<bool> OldGuids;
		public static readonly MyCatConnectionStringOption<bool> PersistSecurityInfo;
		public static readonly MyCatConnectionStringOption<bool> TreatTinyAsBoolean;
		public static readonly MyCatConnectionStringOption<bool> UseAffectedRows;
		public static readonly MyCatConnectionStringOption<bool> UseCompression;

		public static MyCatConnectionStringOption TryGetOptionForKey(string key)
		{
			MyCatConnectionStringOption option;
			return s_options.TryGetValue(key, out option) ? option : null;
		}

		public static MyCatConnectionStringOption GetOptionForKey(string key)
		{
			var option = TryGetOptionForKey(key);
			if (option == null)
				throw new InvalidOperationException("Option '{0}' not supported.".FormatInvariant(key));
			return option;
		}

		public string Key => m_keys[0];
		public IReadOnlyList<string> Keys => m_keys;

		public abstract object GetObject(MyCatConnectionStringBuilder builder);

		protected MyCatConnectionStringOption(IReadOnlyList<string> keys)
		{
			m_keys = keys;
		}

		private static void AddOption(MyCatConnectionStringOption option)
		{
			foreach (string key in option.m_keys)
				s_options.Add(key, option);
		}

		static MyCatConnectionStringOption()
		{
			s_options = new Dictionary<string, MyCatConnectionStringOption>(StringComparer.OrdinalIgnoreCase);

			// Base Options
			AddOption(Server = new MyCatConnectionStringOption<string>(
				keys: new[] { "Server", "Host", "Data Source", "DataSource", "Address", "Addr", "Network Address" },
				defaultValue: ""));

			AddOption(Port = new MyCatConnectionStringOption<uint>(
				keys: new[] { "Port" },
				defaultValue: 3306u));

			AddOption(UserID = new MyCatConnectionStringOption<string>(
				keys: new[] { "User Id", "UserID", "Username", "Uid", "User name", "User" },
				defaultValue: ""));

			AddOption(Password = new MyCatConnectionStringOption<string>(
				keys: new[] { "Password", "pwd" },
				defaultValue: ""));

			AddOption(Database = new MyCatConnectionStringOption<string>(
				keys: new[] { "Database", "Initial Catalog" },
				defaultValue: ""));

			// SSL/TLS Options
			AddOption(SslMode = new MyCatConnectionStringOption<MyCatSslMode>(
				keys: new[] { "SSL Mode", "SslMode" },
				defaultValue: MyCatSslMode.None));

			AddOption(CertificateFile = new MyCatConnectionStringOption<string>(
				keys: new[] { "CertificateFile", "Certificate File" },
				defaultValue: null));

			AddOption(CertificatePassword = new MyCatConnectionStringOption<string>(
				keys: new[] { "CertificatePassword", "Certificate Password" },
				defaultValue: null));

			// Connection Pooling Options
			AddOption(Pooling = new MyCatConnectionStringOption<bool>(
				keys: new[] { "Pooling" },
				defaultValue: true));

			AddOption(ConnectionReset = new MyCatConnectionStringOption<bool>(
				keys: new[] { "Connection Reset", "ConnectionReset" },
				defaultValue: true));

			AddOption(MinimumPoolSize = new MyCatConnectionStringOption<uint>(
				keys: new[] { "Minimum Pool Size", "Min Pool Size", "MinimumPoolSize", "minpoolsize" },
				defaultValue: 0));

			AddOption(MaximumPoolSize = new MyCatConnectionStringOption<uint>(
				keys: new[] { "Maximum Pool Size", "Max Pool Size", "MaximumPoolSize", "maxpoolsize" },
				defaultValue: 100));

			// Other Options
			AddOption(AllowUserVariables = new MyCatConnectionStringOption<bool>(
				keys: new[] { "AllowUserVariables", "Allow User Variables" },
				defaultValue: true));

			AddOption(CharacterSet = new MyCatConnectionStringOption<string>(
				keys: new[] { "CharSet", "Character Set", "CharacterSet" },
				defaultValue: ""));

			AddOption(ConnectionTimeout = new MyCatConnectionStringOption<uint>(
				keys: new[] { "Connection Timeout", "ConnectionTimeout", "Connect Timeout" },
				defaultValue: 15u));

			AddOption(ConvertZeroDateTime = new MyCatConnectionStringOption<bool>(
				keys: new[] { "Convert Zero Datetime", "ConvertZeroDateTime" },
				defaultValue: false));

			AddOption(ForceSynchronous = new MyCatConnectionStringOption<bool>(
				keys: new[] { "ForceSynchronous" },
				defaultValue: false));

			AddOption(Keepalive = new MyCatConnectionStringOption<uint>(
				keys: new[] { "Keep Alive", "Keepalive" },
				defaultValue: 0u));

			AddOption(OldGuids = new MyCatConnectionStringOption<bool>(
				keys: new[] { "Old Guids", "OldGuids" },
				defaultValue: false));

			AddOption(PersistSecurityInfo = new MyCatConnectionStringOption<bool>(
				keys: new[] { "Persist Security Info", "PersistSecurityInfo" },
				defaultValue: false));

			AddOption(TreatTinyAsBoolean = new MyCatConnectionStringOption<bool>(
				keys: new[] { "Treat Tiny As Boolean", "TreatTinyAsBoolean" },
				defaultValue: true));

			AddOption(UseAffectedRows = new MyCatConnectionStringOption<bool>(
				keys: new[] { "Use Affected Rows", "UseAffectedRows" },
				defaultValue: false));

			AddOption(UseCompression = new MyCatConnectionStringOption<bool>(
				keys: new[] { "Compress", "Use Compression", "UseCompression" },
				defaultValue: false));
		}

		static readonly Dictionary<string, MyCatConnectionStringOption> s_options;

		readonly IReadOnlyList<string> m_keys;
	}

	internal sealed class MyCatConnectionStringOption<T> : MyCatConnectionStringOption
	{
		public MyCatConnectionStringOption(IReadOnlyList<string> keys, T defaultValue, Func<T, T> coerce = null)
			: base(keys)
		{
			m_defaultValue = defaultValue;
			m_coerce = coerce;
		}

		public T GetValue(MyCatConnectionStringBuilder builder)
		{
			object objectValue;
			return builder.TryGetValue(Key, out objectValue) ? ChangeType(objectValue) : m_defaultValue;
		}

		public void SetValue(MyCatConnectionStringBuilder builder, T value)
		{
			builder[Key] = m_coerce != null ? m_coerce(value) : value;
		}

		public override object GetObject(MyCatConnectionStringBuilder builder)
		{
			return GetValue(builder);
		}

		private static T ChangeType(object objectValue)
		{
			if (typeof(T) == typeof(bool) && objectValue is string)
			{
				if (string.Equals((string) objectValue, "yes", StringComparison.OrdinalIgnoreCase))
					return (T) (object) true;
				if (string.Equals((string) objectValue, "no", StringComparison.OrdinalIgnoreCase))
					return (T) (object) false;
			}

			if (typeof(T) == typeof(MyCatSslMode) && objectValue is string)
			{
				foreach (var val in Enum.GetValues(typeof(T)))
				{
					if (string.Equals((string) objectValue, val.ToString(), StringComparison.OrdinalIgnoreCase))
						return (T) val;
				}
				throw new InvalidOperationException("Value '{0}' not supported for option '{1}'.".FormatInvariant(objectValue, typeof(T).Name));
			}

			return (T) Convert.ChangeType(objectValue, typeof(T), CultureInfo.InvariantCulture);
		}

		readonly T m_defaultValue;
		readonly Func<T, T> m_coerce;
	}
}

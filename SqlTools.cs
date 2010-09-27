using System;
using System.Data;
using System.Data.Common;

namespace SharpNick
{
	/// <summary>
	/// A group of convenience methods when accessing databases.
	/// </summary>
	public static class SqlTools
	{
		/// <summary>
		/// Gets the string value at the specified index of a data reader. If DBNull, return an empty string.
		/// </summary>
		/// <param name="reader"></param>
		/// <param name="index"></param>
		/// <returns></returns>
		public static string GetString(DbDataReader reader, int index)
		{
			if (reader.IsDBNull(index)) return string.Empty;
			return reader.GetString(index);
		}
		/// <summary>
		/// Gets the Decimal value for the scalar value returned by the DbCommand instance.
		/// </summary>
		/// <param name="cmd"></param>
		/// <returns></returns>
		public static string GetString(DbCommand cmd)
		{
			var result = cmd.ExecuteScalar();
			if (result is DBNull || result == null) return string.Empty;
			return Convert.ToString(result);
		}
		/// <summary>
		/// Gets the Int32 value at the specified index of a data reader. If DBNull, returns 0.
		/// </summary>
		/// <param name="reader"></param>
		/// <param name="index"></param>
		/// <returns></returns>
		public static int GetInt32(DbDataReader reader, int index)
		{
			if (reader.IsDBNull(index)) return default(int);
			return reader.GetInt32(index);
		}
		/// <summary>
		/// Gets the Int32 value for the scalar value returned by the DbCommand instance.
		/// </summary>
		/// <param name="cmd"></param>
		/// <returns></returns>
		public static int GetInt32(DbCommand cmd)
		{
			var result = cmd.ExecuteScalar();
			if (result is DBNull || result == null) return 0;
			return Convert.ToInt32(result);
		}
		/// <summary>
		/// Gets the date/time value at the specified index of a data reader. If DBNull, returns DateTime.MinValue.
		/// </summary>
		/// <param name="reader"></param>
		/// <param name="index"></param>
		/// <returns></returns>
		public static DateTime GetDateTime(DbDataReader reader, int index)
		{
			if (reader.IsDBNull(index)) return default(DateTime);
			return reader.GetDateTime(index);
		}
		/// <summary>
		/// Gets the Decimal value for the scalar value returned by the DbCommand instance.
		/// </summary>
		/// <param name="cmd"></param>
		/// <returns></returns>
		public static DateTime GetDateTime(DbCommand cmd)
		{
			var result = cmd.ExecuteScalar();
			if (result is DBNull || result == null) return DateTime.MinValue;
			return Convert.ToDateTime(result);
		}
		/// <summary>
		/// Gets the boolean value at the specified index of a data reader. If DBNull, returns false.
		/// </summary>
		/// <param name="reader"></param>
		/// <param name="index"></param>
		/// <returns></returns>
		public static bool GetBoolean(DbDataReader reader, int index)
		{
			if (reader.IsDBNull(index)) return false;
			return reader.GetBoolean(index);
		}
		/// <summary>
		/// Gets the Decimal value for the scalar value returned by the DbCommand instance.
		/// </summary>
		/// <param name="cmd"></param>
		/// <returns></returns>
		public static bool GetBoolean(DbCommand cmd)
		{
			var result = cmd.ExecuteScalar();
			if (result is DBNull || result == null) return false;
			return Convert.ToBoolean(result);
		}
		/// <summary>
		/// Gets the Decimal value at the specified index of a data reader. If DBNull, returns 0.
		/// </summary>
		/// <param name="reader"></param>
		/// <param name="index"></param>
		/// <returns></returns>
		public static decimal GetDecimal(DbDataReader reader, int index)
		{
			if (reader.IsDBNull(index)) return default(decimal);
			return reader.GetDecimal(index);
		}
		/// <summary>
		/// Gets the Decimal value for the scalar value returned by the DbCommand instance.
		/// </summary>
		/// <param name="cmd"></param>
		/// <returns></returns>
		public static decimal GetDecimal(DbCommand cmd)
		{
			var result = cmd.ExecuteScalar();
			if (result is DBNull || result == null) return 0;
			return Convert.ToDecimal(result);
		}
		/// <summary>
		/// Executes a short query, not expecting any results from the query.
		/// </summary>
		/// <param name="connection"></param>
		/// <param name="command"></param>
		public static void ExecuteNonQuery(DbConnection connection, string command)
		{
			var cmd = connection.CreateCommand();
			cmd.CommandText = command;
			cmd.ExecuteNonQuery();
		}

		private static DbParameter CreateParameter(DbCommand cmd, string name, object value, DbType dbType)
		{
			var result = cmd.CreateParameter();
			result.ParameterName = name;
			result.Value = value;
			result.DbType = dbType;

			return result;
		}

		public static DbParameter CreateParameter(this DbCommand cmd, string name, DateTime value)
		{
			return CreateParameter(cmd, name, value, DbType.DateTime);
		}

		public static DbParameter CreateParameter(this DbCommand cmd, string name, string value)
		{
			return CreateParameter(cmd, name, value, DbType.String);
		}
		public static DbParameter CreateParameter(this DbCommand cmd, string name, object value)
		{
			var stringValue = value == null ? null : value.ToString();
			return CreateParameter(cmd, name, value, DbType.String);
		}

		public static DbParameter CreateParameter(this DbCommand cmd, string name, int value)
		{
			return CreateParameter(cmd, name, value, DbType.Int32);
		}

		public static DbParameter CreateParameter(this DbCommand cmd, string name, bool value)
		{
			return CreateParameter(cmd, name, value, DbType.Boolean);
		}

		public static DbParameter CreateParameter(this DbCommand cmd, string name, DbType dbType)
		{
			return CreateParameter(cmd, name, null, dbType);
		}
	}
}

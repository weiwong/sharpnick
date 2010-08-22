namespace SharpNick.SslRedirect
{
	/// <summary>
	/// List of security levels to serve the request in.
	/// </summary>
	public enum SecurityType
	{
		/// <summary>
		/// Ensure that the request is served under SSL.
		/// </summary>
		Secure,
		/// <summary>
		/// Ensure that the request is served under non-SSL.
		/// </summary>
		Unsecure,
		/// <summary>
		/// Served the request under whatever the request is under.
		/// </summary>
		Ignore
	}
	/// <summary>
	/// List of modes to run the SSL module under.
	/// </summary>
	public enum SslRedirectMode
	{
		/// <summary>
		/// Let the SSL module perform interception on all calls.
		/// </summary>
		On,
		/// <summary>
		/// Disable SSL module.
		/// </summary>
		Off,
		/// <summary>
		/// Intercept requests only if they are not made from the local machine.
		/// </summary>
		RemoteOnly
	}
}

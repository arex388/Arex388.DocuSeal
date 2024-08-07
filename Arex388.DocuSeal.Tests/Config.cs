using Microsoft.Extensions.Configuration;

namespace Arex388.DocuSeal.Tests;

internal sealed class Config {
	private static readonly IConfigurationRoot _configuration = new ConfigurationManager().AddUserSecrets<Config>().Build();

	public static string AuthorizationToken1 = _configuration["authorizationToken1"]!;
	public static string AuthorizationToken2 = _configuration["authorizationToken2"]!;
	public static string Email1 = _configuration["email1"]!;
	public static string Email2 = _configuration["email2"]!;
}
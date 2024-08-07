using Microsoft.Extensions.Configuration;

namespace Arex388.DocuSeal.Benchmarks;

internal sealed class Config {
	private static readonly IConfigurationRoot _configuration = new ConfigurationManager().AddUserSecrets<Config>().Build();

	public static string AuthorizationToken => _configuration["authorizationToken"]!;
}
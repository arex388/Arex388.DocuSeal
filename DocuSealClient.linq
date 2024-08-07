<Query Kind="Program">
  <Reference Relative="Arex388.DocuSeal\bin\Debug\netstandard2.0\Arex388.DocuSeal.dll">E:\Software Development\Arex388.DocuSeal\Arex388.DocuSeal\bin\Debug\netstandard2.0\Arex388.DocuSeal.dll</Reference>
  <NuGetReference>Microsoft.Extensions.Http</NuGetReference>
  <Namespace>Arex388.DocuSeal</Namespace>
  <Namespace>Microsoft.Extensions.DependencyInjection</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>Arex388.DocuSeal.Models</Namespace>
</Query>

private static readonly DocuSealClientOptions _options = new DocuSealClientOptions {
	AuthorizationToken = Util.GetPassword("???")
};

async Task Main() {
	//var docuSeal = GetClientMultiple();
	//var docuSeal = GetClientSingle();
}

public IDocuSealClient GetClientMultiple() {
	var services = new ServiceCollection().AddDocuSealClient().BuildServiceProvider();
	var docuSealFactory = services.GetRequiredService<IDocuSealClientFactory>();

	return docuSealFactory.CreateClient(_options);
}

public IDocuSealClient GetClientSingle() {
	var services = new ServiceCollection().AddDocuSealClient(_options).BuildServiceProvider();

	return services.GetRequiredService<IDocuSealClient>();
}

//	============================================================================
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//
//	EoF
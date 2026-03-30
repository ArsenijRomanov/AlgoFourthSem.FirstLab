using Analysis;

var configPath = Path.Combine(AppContext.BaseDirectory, "Configs", "main_config.json");
var suiteConfig = SuiteLoader.LoadFromFile(configPath);

var projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
var outputDirectory = Path.Combine(projectRoot, "results", suiteConfig.SuiteId, timestamp);

ExperimentRunnerFacade.RunToDirectory(suiteConfig, outputDirectory);

Console.WriteLine($"Готово. Результаты сохранены в: {outputDirectory}");

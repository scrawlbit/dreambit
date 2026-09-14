// Vários testes exercitam estado estático global do motor (Screen, AudioMixer, Input,
// SaveGame, DataCatalog, EngineLog, Localizer). O xUnit paraleliza coleções por padrão;
// desligamos a paralelização para rodar sequencial (como fazia o MSTest) e evitar corridas.
[assembly: Xunit.CollectionBehavior(DisableTestParallelization = true)]

#nullable enable
using Content.IntegrationTests.Fixtures;
using Content.Shared._Fish.Silicons.Borgs.Components;
using Robust.Shared.GameObjects;

namespace Content.IntegrationTests.Tests._Fish.Silicons.Borgs;

/// <summary>
/// Проверяет, что маркер <see cref="BorgNoFallComponent"/>, добавленный на живого борга
/// при runtime-апгрейде до Mk2, доходит до клиента.
/// Апгрейд (<c>BorgSwitchableTypeSystem.Upgrade</c>) добавляет компоненты через
/// <see cref="IEntityManager.AddComponent(EntityUid, string, bool)"/> на уже существующей
/// сущности, а не приходит из прототипа при спавне (MapInit). Без <c>[NetworkedComponent]</c>
/// клиент не получил бы компонент и обрабатывал бы <c>DownAttemptEvent</c> иначе, чем сервер.
/// </summary>
[TestFixture]
[TestOf(typeof(BorgNoFallComponent))]
public sealed class BorgNoFallComponentReplicationTest : GameTest
{
    [Test]
    public async Task RuntimeAddedComponentReplicatesToClient()
    {
        var pair = Pair;
        var server = pair.Server;
        var client = pair.Client;

        var testMap = await pair.CreateTestMap();

        // Спавним «живого борга» и даём сущности дойти до клиента без компонента.
        var serverEnt = await SpawnAtPosition(null, testMap.GridCoords);

        await pair.RunTicksSync(10);

        var netEnt = SEntMan.GetNetEntity(serverEnt);

        await client.WaitPost(() =>
        {
            var clientEnt = CEntMan.GetEntity(netEnt);
            Assert.That(CEntMan.HasComponent<BorgNoFallComponent>(clientEnt), Is.False,
                "До апгрейда у клиента не должно быть BorgNoFallComponent");
        });

        // Аналог ApplyMk2Components: компонент добавляется на уже существующую сущность,
        // а не приходит из прототипа при спавне (MapInit).
        await server.WaitPost(() => SEntMan.AddComponent<BorgNoFallComponent>(serverEnt));

        await pair.RunTicksSync(10);

        await client.WaitPost(() =>
        {
            var clientEnt = CEntMan.GetEntity(netEnt);
            Assert.That(CEntMan.HasComponent<BorgNoFallComponent>(clientEnt), Is.True,
                "BorgNoFallComponent должен реплицироваться клиенту как NetworkedComponent");
        });
    }
}

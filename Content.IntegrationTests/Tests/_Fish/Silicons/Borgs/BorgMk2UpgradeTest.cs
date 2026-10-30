#nullable enable
using Content.IntegrationTests.Fixtures;
using Content.Server.Silicons.Borgs;
using Content.Shared._Fish.Silicons.Borgs.Components;
using Content.Shared.Inventory;
using Content.Shared.Silicons.Borgs.Components;
using Robust.Shared.Containers;
using Robust.Shared.GameObjects;

namespace Content.IntegrationTests.Tests._Fish.Silicons.Borgs;

/// <summary>
/// Проверяет транзакционность апгрейда Security-борга: при сбое надевания Mk2-брони
/// борг остаётся на исходном типе и со старой бронёй, а предмет-апгрейд не расходуется.
/// </summary>
[TestFixture]
[TestOf(typeof(BorgSwitchableTypeSystem))]
public sealed class BorgMk2UpgradeTest : GameTest
{
    [Test]
    public async Task SuccessfulUpgradeAppliesMk2TypeAndArmor()
    {
        await Server.WaitAssertion(() =>
        {
            var borg = SSpawn("BorgChassisSecurity");
            var module = SSpawn("FishBorgUpgradeModule");
            var switchable = SComp<BorgSwitchableTypeComponent>(borg);
            var inventory = SComp<InventoryComponent>(borg);
            var inventorySystem = Server.System<InventorySystem>();
            var upgradeSystem = Server.System<BorgSwitchableTypeSystem>();

            var result = upgradeSystem.TryFinishUpgrade(
                new Entity<BorgUpgradeModuleComponent>(module, SComp<BorgUpgradeModuleComponent>(module)),
                borg,
                borg);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.True, "Апгрейд должен завершиться успешно");
                Assert.That(switchable.SelectedBorgType?.Id, Is.EqualTo("FishSecurityMk2"));
                Assert.That(SEntMan.IsQueuedForDeletion(module), Is.True,
                    "Успешный апгрейд должен израсходовать предмет-модуль");
                Assert.That(inventorySystem.TryGetSlotEntity(borg, "armor", out var armor, inventory), Is.True);
                Assert.That(SEntMan.GetComponent<MetaDataComponent>(armor!.Value).EntityPrototype?.ID,
                    Is.EqualTo("BorgArmorSiliconMk2"));
            });
        });
    }

    [Test]
    public async Task FailedArmorEquipRestoresOldArmorAndKeepsType()
    {
        await Server.WaitAssertion(() =>
        {
            var borg = SSpawn("BorgChassisSecurity");
            var module = SSpawn("FishBorgUpgradeModule");
            var switchable = SComp<BorgSwitchableTypeComponent>(borg);
            var inventory = SComp<InventoryComponent>(borg);
            var inventorySystem = Server.System<InventorySystem>();
            var upgradeSystem = Server.System<BorgSwitchableTypeSystem>();

            Assert.That(inventorySystem.TryGetSlotEntity(borg, "armor", out var oldArmor, inventory), Is.True,
                "Security-борг должен начинать со старой бронёй");

            SEntMan.AddComponent<BlockMk2ArmorInsertComponent>(borg);

            var result = upgradeSystem.TryFinishUpgrade(
                new Entity<BorgUpgradeModuleComponent>(module, SComp<BorgUpgradeModuleComponent>(module)),
                borg,
                borg);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.False, "Апгрейд должен отмениться при сбое надевания брони");
                Assert.That(switchable.SelectedBorgType?.Id, Is.EqualTo("security"));
                Assert.That(SEntMan.IsQueuedForDeletion(module), Is.False,
                    "Неудачный апгрейд не должен расходовать предмет-модуль");
                Assert.That(inventorySystem.TryGetSlotEntity(borg, "armor", out var armor, inventory), Is.True);
                Assert.That(armor, Is.EqualTo(oldArmor), "Старая броня должна быть возвращена в слот");
            });
        });
    }
}

[RegisterComponent]
public sealed partial class BlockMk2ArmorInsertComponent : Component;

/// <summary>
/// Тестовый блокиратор: запрещает вставку Mk2-брони в слот, чтобы проверить откат.
/// </summary>
public sealed partial class BlockMk2ArmorInsertSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BlockMk2ArmorInsertComponent, ContainerIsInsertingAttemptEvent>(OnInserting);
    }

    private void OnInserting(Entity<BlockMk2ArmorInsertComponent> ent, ref ContainerIsInsertingAttemptEvent args)
    {
        if (MetaData(args.EntityUid).EntityPrototype?.ID == "BorgArmorSiliconMk2")
            args.Cancel();
    }
}

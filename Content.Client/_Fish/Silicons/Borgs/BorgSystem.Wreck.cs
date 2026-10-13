using Content.Shared.Mobs;
using Content.Shared.Silicons.Borgs;
using Content.Shared.Silicons.Borgs.Components;
using Robust.Client.GameObjects;
using Robust.Shared.Prototypes;

#pragma warning disable IDE0130 // Необходимо поскольку этот partial класс расширяет ванильный тип и должен сохранять исходный namespace несмотря на расположение файла в _Fish
namespace Content.Client.Silicons.Borgs;

/// <summary>
/// Fish: переключение Body-слоя на wreck-состояние борга Mk2 в критическом/мёртвом
/// состоянии и обратно при возврате в Alive. Вызывается из ванильного
/// OnBorgAppearanceChanged одной строкой (дублирующая подписка запрещена EventBus).
/// </summary>
public sealed partial class BorgSystem
{
    [Dependency] private IPrototypeManager _wreckPrototypes = default!;

    /// <summary>
    /// Переключает Body-слой на wreck-состояние борга Mk2 в критическом/мёртвом
    /// состоянии и обратно при возврате в Alive. Wreck-спрайт есть только у типов,
    /// задавших <see cref="BorgTypePrototype.SpriteWreckState"/>.
    /// Также гасит слой фонаря (LightStatus) в крите: сервер уже вызывает
    /// HandheldLight.TurnOff через BorgNoFallSystem, но визуал обновляется с задержкой —
    /// прячем сразу на клиенте. При возврате в Alive слой восстановит ToggleableVisuals
    /// при включении фонаря.
    /// </summary>
    private void UpdateBorgWreckAppearance(Entity<BorgChassisComponent> chassis, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;

        if (!TryComp<BorgSwitchableTypeComponent>(chassis, out var switchable))
            return;

        if (switchable.SelectedBorgType is not { } borgType)
            return;

        if (!_wreckPrototypes.TryIndex(borgType, out var prototype) || prototype.SpriteWreckState == null)
            return;

        if (!_appearance.TryGetData<MobState>(chassis.Owner, MobStateVisuals.State, out var state, args.Component))
            return;

        // При возврате в Alive восстанавливаем исходный Body-состояние типа;
        // в критическом/мёртвом переключаемся на wreck-кадр.
        var bodyState = state == MobState.Alive
            ? prototype.SpriteBodyState
            : prototype.SpriteWreckState;

        _sprite.LayerSetRsiState((chassis.Owner, args.Sprite), BorgVisualLayers.Body, bodyState!);

        if (state != MobState.Alive)
            _sprite.LayerSetVisible((chassis.Owner, args.Sprite), BorgVisualLayers.LightStatus, false);
    }
}

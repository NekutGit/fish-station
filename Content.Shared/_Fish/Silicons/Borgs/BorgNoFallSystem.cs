using Content.Shared._Fish.Silicons.Borgs.Components;
using Content.Shared.Light;
using Content.Shared.Light.Components;
using Content.Shared.Mobs;
using Content.Shared.Standing;

namespace Content.Shared._Fish.Silicons.Borgs;

/// <summary>
/// Fish: предотвращает падение борга Mk2 в критическом/мёртвом состоянии
/// и гасит его фонарь при входе в крит.
/// Ванильные борги (без <see cref="BorgNoFallComponent"/>) не затрагиваются.
/// </summary>
public sealed partial class BorgNoFallSystem : EntitySystem
{
    [Dependency] private SharedHandheldLightSystem _handheldLight = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BorgNoFallComponent, DownAttemptEvent>(OnDownAttempt);
        SubscribeLocalEvent<BorgNoFallComponent, MobStateChangedEvent>(OnMobStateChanged);
    }

    private void OnDownAttempt(EntityUid uid, BorgNoFallComponent component, DownAttemptEvent args)
    {
        // Mk2 — робот, он не опрокидывается на пол при крите.
        args.Cancel();
    }

    private void OnMobStateChanged(Entity<BorgNoFallComponent> uid, ref MobStateChangedEvent args)
    {
        if (args.NewMobState == MobState.Alive)
            return;

        // Гасим фонарь: в критическом состоянии он не горит и не накладывает
        // текстуру на wreck-спрайт. После реанимации игрок включит сам.
        if (TryComp<HandheldLightComponent>(uid, out var light))
            _handheldLight.TurnOff((uid.Owner, light), makeNoise: false);
    }
}

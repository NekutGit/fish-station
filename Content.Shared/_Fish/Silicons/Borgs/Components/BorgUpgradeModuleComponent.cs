#pragma warning disable IDE0130 // Необходимо поскольку этот partial класс расширяет ванильный тип и должен сохранять исходный namespace несмотря на расположение файла в _Fish
namespace Content.Shared.Silicons.Borgs.Components;

/// <summary>
/// Предмет-апгрейд, превращающий обычный cyborg в его Mk2-вариант.
/// Механика реализована в <c>Content.Server.Silicons.Borgs.BorgSwitchableTypeSystem.Upgrade</c>.
/// </summary>
[RegisterComponent]
public sealed partial class BorgUpgradeModuleComponent : Component
{
    /// <summary>
    /// Длительность непрерывного апгрейда.
    /// </summary>
    [DataField]
    public TimeSpan Delay = TimeSpan.FromSeconds(15);
}

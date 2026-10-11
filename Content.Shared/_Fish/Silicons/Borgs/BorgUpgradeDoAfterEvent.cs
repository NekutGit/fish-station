using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

#pragma warning disable IDE0130 // Необходимо поскольку этот partial класс расширяет ванильный тип и должен сохранять исходный namespace несмотря на расположение файла в _Fish
namespace Content.Shared.Silicons.Borgs;

/// <summary>
/// Завершение апгрейда борга до Mk2 предметом BorgUpgradeModule.
/// </summary>
[Serializable, NetSerializable]
public sealed partial class BorgUpgradeDoAfterEvent : SimpleDoAfterEvent
{
}

namespace Content.Shared._Fish.Silicons.Borgs.Components;

/// <summary>
/// Fish: маркер — борг не падает при критическом/мёртвом состоянии
/// (<see cref="Content.Shared.Standing.DownAttemptEvent"/> отменяется)
/// и гасит фонарь при входе в крит.
/// Вешается только на Mk2-шасси; ванильные борги не затрагиваются.
/// </summary>
[RegisterComponent]
public sealed partial class BorgNoFallComponent : Component;

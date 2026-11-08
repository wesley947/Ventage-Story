using System;
using System.Collections.Generic;

namespace lingamsdigestivesim.Foods;

/// <summary>
/// Secondary asset (config/foodcodebindings.json): one food profile may have many
/// verified vanilla collectible codes. This is a lookup table, not an eating hook.
/// </summary>
public sealed class FoodCodeBindingCatalog
{
    public int SchemaVersion { get; set; }
    public string AuditVersion { get; set; } = "";
    public string Note { get; set; } = "";
    public List<FoodCodeBinding> Bindings { get; set; } = new();
}

public sealed class FoodCodeBinding
{
    public string NativeCode { get; set; } = "";
    public string ProfileId { get; set; } = "";
}

using JetBrains.Annotations;
using Newtonsoft.Json;

namespace CrowdControlMod.CrowdControlService;

public readonly struct CrowdControlRequest
{
    #region Static Methods

    [PublicAPI] [Pure]
    public static CrowdControlRequest FromJson(string data)
    {
        return JsonConvert.DeserializeObject<CrowdControlRequest>(data);
    }

    #endregion

    #region Fields

    [JsonProperty("id")]
    public readonly int Id;

    [JsonProperty("code")]
    [NotNull]
    public readonly string Code;

    [JsonProperty("viewer")]
    [NotNull]
    public readonly string Viewer;

    [JsonProperty("type")]
    public readonly int Type;

    #endregion

    #region Constructors

    [JsonConstructor]
    public CrowdControlRequest(int id, [NotNull] string code, [NotNull] string viewer, int type)
    {
        Id = id;
        Code = code;
        Viewer = viewer;
        Type = type;
    }

    #endregion
}
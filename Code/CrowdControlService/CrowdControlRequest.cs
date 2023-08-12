using System.Diagnostics.Contracts;
using Newtonsoft.Json;

namespace CrowdControlMod.CrowdControlService;

public readonly struct CrowdControlRequest
{
    #region Static Methods

    [Pure]
    public static CrowdControlRequest FromJson(string data)
    {
        return JsonConvert.DeserializeObject<CrowdControlRequest>(data);
    }

    [Pure]
    public static string ToJson(in CrowdControlRequest request)
    {
        return JsonConvert.SerializeObject(request);
    }

    #endregion

    #region Fields

    [JsonProperty("id")]
    public readonly int Id;

    [JsonProperty("code")]
    public readonly string Code;

    [JsonProperty("viewer")]
    public readonly string Viewer;

    [JsonProperty("type")]
    public readonly int Type;

    [JsonProperty("duration")]
    public readonly int Duration;

    #endregion

    #region Constructors

    [JsonConstructor]
    public CrowdControlRequest(int id, string code, string viewer, int type, int duration)
    {
        Id = id;
        Code = code;
        Viewer = viewer;
        Type = type;
        Duration = duration;
    }

    #endregion
}
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace CrowdControlMod.CrowdControlService;

public readonly struct CrowdControlResponse
{
    #region Static Methods

    [PublicAPI] [Pure] [NotNull]
    public static string ToJson(in CrowdControlResponse response)
    {
        return JsonConvert.SerializeObject(response);
    }

    #endregion

    #region Fields

    [JsonProperty("id")]
    public readonly int Id;

    [JsonProperty("status")]
    public readonly int Status;

    [JsonProperty("message")]
    [NotNull]
    public readonly string Message;

    #endregion

    #region Constructors

    [JsonConstructor]
    public CrowdControlResponse(int id, int status, [NotNull] string message)
    {
        Id = id;
        Status = status;
        Message = message;
    }

    #endregion
}
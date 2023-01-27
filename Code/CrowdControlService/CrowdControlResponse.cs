using System.Diagnostics.Contracts;
using Newtonsoft.Json;

namespace CrowdControlMod.CrowdControlService;

public readonly struct CrowdControlResponse
{
    #region Static Methods

    [Pure]
    public static CrowdControlResponse FromJson(string data)
    {
        return JsonConvert.DeserializeObject<CrowdControlResponse>(data);
    }
    
    [Pure]
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
    public readonly string Message;

    #endregion

    #region Constructors

    [JsonConstructor]
    public CrowdControlResponse(int id, int status, string message)
    {
        Id = id;
        Status = status;
        Message = message;
    }

    #endregion
}
namespace Arcraven.WebRtc;
public record SessionDescription(string Type, string Sdp); // "offer"|"answer"
public record IceCandidate(string Candidate, string SdpMid, int SdpMLineIndex);

public interface ISignalingClient : IAsyncDisposable
{
    Task ConnectAsync(CancellationToken ct);
    Task SendOfferAsync(SessionDescription offer, CancellationToken ct);
    Task SendAnswerAsync(SessionDescription answer, CancellationToken ct);
    Task SendIceAsync(IceCandidate candidate, CancellationToken ct);

    event Action<SessionDescription>? OnOffer;
    event Action<SessionDescription>? OnAnswer;
    event Action<IceCandidate>? OnIce;
}
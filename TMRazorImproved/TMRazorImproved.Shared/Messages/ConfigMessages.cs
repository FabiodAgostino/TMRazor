namespace TMRazorImproved.Shared.Messages
{
    /// <summary>
    /// Messaggio inviato quando una proprietà del profilo utente cambia.
    /// </summary>
    public record ConfigChangedMessage(string PropertyName);
}

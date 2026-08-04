namespace DevMatch.Application.Abstraction.Authentication;
 
    /// <summary>
    /// Creates and validates a short-lived signed OAuth state value to prevent CSRF.
    /// </summary>
    public interface IOAuthStateService
    {
        string CreateState();
        bool IsValid(string state);
    }
 

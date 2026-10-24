namespace KLENZ.Services
{
    public interface ICompanyContext
    {
        int? CurrentCompanyId { get; }
        string? CurrentCompanyName { get; }
        string? CurrentCompanyShortName { get; }
        bool HasSelectedCompany { get; }
        void SetCompany(int id, string fullName, string shortName);
        void ClearCompany();
    }
}

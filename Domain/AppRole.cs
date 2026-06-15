using Base.Domain;

namespace Domain;

public class AppRole : BaseIdentityRole
{
    public Guid ClientId { get; set; }
    public Client? Client { get; set; }
}
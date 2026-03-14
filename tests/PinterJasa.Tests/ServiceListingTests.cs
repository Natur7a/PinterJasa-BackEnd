using PinterJasa.API.DTOs.Services;
using PinterJasa.API.Services;

namespace PinterJasa.Tests;

public class ServiceListingTests
{
    [Fact]
    public async Task CreateService_ValidRequest_Succeeds()
    {
        var db = TestDbFactory.Create("create_service_valid");
        var svc = new ServiceService(db);

        var request = new CreateServiceRequest
        {
            CategoryId = TestDbFactory.CategoryId,
            Title = "New Jasa",
            Description = "Detail",
            Price = 50_000m,
            PriceUnit = "per_job"
        };

        var result = await svc.CreateAsync(TestDbFactory.ProviderUserId, request);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("New Jasa", result.Title);
        Assert.Equal("active", result.Status);
    }

    [Fact]
    public async Task UpdateService_Owner_Succeeds()
    {
        var db = TestDbFactory.Create("update_service_owner");
        var svc = new ServiceService(db);

        var result = await svc.UpdateAsync(TestDbFactory.ServiceId, TestDbFactory.ProviderUserId,
            new UpdateServiceRequest { Title = "Updated Title", Status = "inactive" });

        Assert.Equal("Updated Title", result.Title);
        Assert.Equal("inactive", result.Status);
    }

    [Fact]
    public async Task UpdateService_NonOwner_Throws()
    {
        var db = TestDbFactory.Create("update_service_non_owner");
        var svc = new ServiceService(db);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            svc.UpdateAsync(TestDbFactory.ServiceId, TestDbFactory.OtherProviderUserId,
                new UpdateServiceRequest { Title = "Hack" }));
    }

    [Fact]
    public async Task UpdateService_InvalidStatus_Throws()
    {
        var db = TestDbFactory.Create("update_service_bad_status");
        var svc = new ServiceService(db);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            svc.UpdateAsync(TestDbFactory.ServiceId, TestDbFactory.ProviderUserId,
                new UpdateServiceRequest { Status = "banned" }));
    }

    [Fact]
    public async Task GetActiveServices_ReturnsOnlyActiveListings()
    {
        var db = TestDbFactory.Create("get_active_services");
        var svc = new ServiceService(db);

        // Deactivate the existing service
        var service = db.Services.Find(TestDbFactory.ServiceId)!;
        service.Status = "inactive";
        db.SaveChanges();

        var results = await svc.GetActiveServicesAsync(null);
        Assert.Empty(results);
    }
}

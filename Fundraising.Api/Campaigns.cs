
using Fundraising.Data;
using Microsoft.EntityFrameworkCore;

static class CampaignApi
{
    public static IEndpointRouteBuilder MapCampaignApi(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/campaigns");
        group.MapPost("/campaigns", AddCampaign);
        group.MapGet("/campaigns", GetCampaigns);
        group.MapPut("/campaigns", UpdateCampaign);
        group.MapDelete("/campaigns/{id}", DeleteCampaign);

        return endpoints;
    }

    private static async Task<IResult> AddCampaign(AddCampaignDTO newCampaign, ApplicationDbContext dbContext)
    {
        var campaign = new Campaign
        {
            Name = newCampaign.Name
        };

        if (await dbContext.Campaigns.AnyAsync(c => c.Name == campaign.Name))
        {
            return Results.BadRequest("A campaign with that name already exists");
        }
        await dbContext.Campaigns.AddAsync(campaign);

        await dbContext.SaveChangesAsync();

        return Results.Created($"/campaigns/{campaign.Id}", new { campaign.Id, campaign.Name });
    }

    private static async Task<IResult> GetCampaigns(ApplicationDbContext dbContext)
    {
        var campaigns = await dbContext.Campaigns
            .Select(c => new { c.Id, c.Name })
            .ToListAsync();

        return Results.Ok(campaigns);
    }

    private static async Task<IResult> UpdateCampaign(UpdateCampaignDTO updatedCampaign, ApplicationDbContext dbContext)
    {
        var campaign = await dbContext.Campaigns.FindAsync(updatedCampaign.Id);

        if (campaign is null)
        {
            return Results.NotFound();
        }

        campaign.Name = updatedCampaign.Name;

        await dbContext.SaveChangesAsync();

        return Results.Ok();
    }

    private static async Task<IResult> DeleteCampaign(int id, ApplicationDbContext dbContext)
    {
        var campaign = await dbContext.Campaigns.FindAsync(id);

        if (campaign is null)
        {
            return Results.NotFound();
        }

        if(await dbContext.Visits.AnyAsync(v => v.CampaignId == id))
        {
            return Results.BadRequest("Cannot delete a campaign that has visits");
        }

        dbContext.Campaigns.Remove(campaign);

        await dbContext.SaveChangesAsync();

        return Results.Ok();
    }
}

record AddCampaignDTO(string Name);
record UpdateCampaignDTO(int Id, string Name);
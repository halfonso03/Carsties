using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuctionsController(AuctionDbContext context, IMapper mapper, IPublishEndpoint publishEndpoint) : ControllerBase
    {
        [HttpGet()]
        public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions(string date)
        {
            var query = context.Auctions.OrderBy(x => x.Item.Make).AsQueryable();

            if (!string.IsNullOrEmpty(date))
            {
                query = query.Where(x =>
                    x.UpdatedAt.CompareTo(DateTime.Parse(date).ToUniversalTime()) > 0);
            }

            return await query.ProjectTo<AuctionDto>(mapper.ConfigurationProvider).ToListAsync();

        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid id)
        {
            var auction = await context.Auctions
             .Include(x => x.Item)
             .Where(x => x.Id == id)
             .FirstOrDefaultAsync();

            if (auction is null) return NotFound();

            return mapper.Map<AuctionDto>(auction);
        }

        [HttpPost]
        public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto auctionDto)
        {
            var auction = mapper.Map<Auction>(auctionDto);

            auction.Seller = "test";

            context.Auctions.Add(auction);

            var newAuction = mapper.Map<AuctionDto>(auction);

            await publishEndpoint.Publish(mapper.Map<AuctionCreated>(newAuction));


            bool result = await context.SaveChangesAsync() > 0;


            if (!result) return BadRequest("Could not create auction");

            return CreatedAtAction(nameof(GetAuctionById), new { auction.Id }, newAuction);
        }



        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAuction(Guid id, UpdateAuctionDto auctionDto)
        {
            Auction auction = await context.Auctions.Include(x => x.Item)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (auction == null) return NotFound();


            auction.Item.Make = auctionDto.Make ?? auction.Item.Make;
            auction.Item.Model = auctionDto.Model ?? auction.Item.Model;
            auction.Item.Color = auctionDto.Make ?? auction.Item.Color;
            auction.Item.Mileage = auctionDto.Mileage ?? auction.Item.Mileage;
            auction.Item.Year = auctionDto.Year ?? auction.Item.Year;


            await publishEndpoint.Publish(mapper.Map<AuctionUpdated>(auction));

            bool result = await context.SaveChangesAsync() > 0;


            if (!result) return BadRequest("Could not update auction");

            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAuction(Guid id)
        {
            Auction auction = await context.Auctions
               .FirstOrDefaultAsync(x => x.Id == id);

            if (auction == null) return NotFound();

            context.Auctions.Remove(auction);

            AuctionDeleted auctionDeleted = mapper.Map<AuctionDeleted>(auction);


            Console.WriteLine("--------------------");
            Console.WriteLine(auctionDeleted.Id);

            await publishEndpoint.Publish<AuctionDeleted>(new { Id = auction.Id.ToString() });


            bool result = await context.SaveChangesAsync() > 0;

            if (!result) return BadRequest("Could not delete auction");

            return Ok();
        }

    }
}

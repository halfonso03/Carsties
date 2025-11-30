using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuctionsController(IAuctionRepository repo, IMapper mapper, IPublishEndpoint publishEndpoint) : ControllerBase
    {
        [HttpGet()]
        public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions(string date)
        {
            return await repo.GetAuctionsAsync(date);

        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid id)
        {
            var auction = await repo.GetAuctionByIdAsync(id);

            if (auction is null) return NotFound();

            return mapper.Map<AuctionDto>(auction);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto auctionDto)
        {
            var auction = mapper.Map<Auction>(auctionDto);

            auction.Seller = User.Identity.Name;

            repo.AddAuction(auction);

            var newAuction = mapper.Map<AuctionDto>(auction);

            await publishEndpoint.Publish(mapper.Map<AuctionCreated>(newAuction));

            bool result = await repo.SaveChangesAsync();

            if (!result) return BadRequest("Could not create auction");

            return CreatedAtAction(nameof(GetAuctionById), new { auction.Id }, newAuction);
        }


        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAuction(Guid id, UpdateAuctionDto auctionDto)
        {
            var auction = await repo.GetAunctionEntityById(id);

            if (auction == null) return NotFound();

            if (auction.Seller != User.Identity.Name) return Forbid();

            auction.Item.Make = auctionDto.Make ?? auction.Item.Make;
            auction.Item.Model = auctionDto.Model ?? auction.Item.Model;
            auction.Item.Color = auctionDto.Make ?? auction.Item.Color;
            auction.Item.Mileage = auctionDto.Mileage ?? auction.Item.Mileage;
            auction.Item.Year = auctionDto.Year ?? auction.Item.Year;


            await publishEndpoint.Publish(mapper.Map<AuctionUpdated>(auction));

            bool result = await repo.SaveChangesAsync();


            if (!result) return BadRequest("Could not update auction");

            return Ok();
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult> DeleteAuction(Guid id)
        {
            var auction = await repo.GetAunctionEntityById(id);

            if (auction == null) return NotFound();

            if (auction.Seller != User.Identity.Name) return Forbid();


            repo.RemoveAuction(auction);

            
            AuctionDeleted auctionDeleted = mapper.Map<AuctionDeleted>(auction);


            Console.WriteLine("--------------------");
            Console.WriteLine(auctionDeleted.Id);

            await publishEndpoint.Publish<AuctionDeleted>(new { Id = auction.Id.ToString() });


            bool result = await repo.SaveChangesAsync();


            if (!result) return BadRequest("Could not delete auction");

            return Ok();
        }

    }
}

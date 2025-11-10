using System;
using AuctionService.Data;
using AuctionService.Entities;
using Contracts;
using MassTransit;

namespace AuctionService.Consumers;

public class AuctionFinishedConsumer(AuctionDbContext context) : IConsumer<AuctionFinished>
{
    public async Task Consume(ConsumeContext<AuctionFinished> consumeContext)
    {
        Console.WriteLine("--> Consuming auction finished");

        var auction = await context.Auctions.FindAsync(Guid.Parse(consumeContext.Message.AuctionId))
            ?? throw new MessageException(typeof(AuctionFinished), "Cannot retrieve this auction");

        if (consumeContext.Message.ItemSold)
        {
            auction.Winner = consumeContext.Message.Winner;
            auction.SoldAmount = consumeContext.Message.Amount;
        }

        auction.Status = auction.SoldAmount > auction.ReservePrice
            ? Status.Finished : Status.ReserveNotMet;

        await context.SaveChangesAsync();
    }
}

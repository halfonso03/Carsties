using System;
using AutoMapper;
using Contracts;
using MassTransit;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Consumers;

public class AuctionUpdatedConsumer(IMapper mapper) : IConsumer<AuctionUpdated>
{
    public async Task Consume(ConsumeContext<AuctionUpdated> context)
    {
        Console.WriteLine("------------------------------------");
        Console.WriteLine("--> consuming auction update " + context.Message.Id);

        Console.WriteLine("------------------------------------");

        var item = mapper.Map<Item>(context.Message);

        Console.WriteLine(context.Message.Model);

        var result = await DB.Update<Item>()
                  .Match(b => b.ID == item.ID)
                  .ModifyOnly(x => new
                  {
                      x.Color,
                      x.Make,
                      x.Model,
                      x.Year,
                      x.Mileage
                  }, item)
                  .ExecuteAsync();

        if (!result.IsAcknowledged) throw new MessageException(typeof(AuctionUpdated), "Problem updating mongodb");

        Console.WriteLine(item.Model);
    }
}

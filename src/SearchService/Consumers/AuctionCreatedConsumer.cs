using System;
using AutoMapper;
using Contracts;
using MassTransit;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Consumers;

public class AuctionCreatedConsumer(IMapper mapper) : IConsumer<AuctionCreated>
{
    public async Task Consume(ConsumeContext<AuctionCreated> context)
    {
        var item = mapper.Map<Item>(context.Message);

        if (item.Model == "foo") throw new ArgumentException("Can't sell that car");

        await item.SaveAsync();
    }
}

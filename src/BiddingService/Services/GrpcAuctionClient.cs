using System;
using AuctionService;
using BiddingService.Models;
using Grpc.Net.Client;

namespace BiddingService.Services;

public class GrpcAuctionClient(ILogger<GrpcAuctionClient> logger, IConfiguration config)
{
    public Auction GetAuction(string id)
    {
        logger.LogInformation("Calling grpc service");

        var channel = GrpcChannel.ForAddress(config["GrpcAuction"]);
        var client = new GrpcAuction.GrpcAuctionClient(channel);
        var request = new GetAuctionRequest
        {
            Id = id
        };

        try
        {
            var reply = client.GetAuction(request);

            var auction = new Auction
            {

                ID = reply.Auction.Id,
                AuctionEnd = DateTime.Parse(reply.Auction.AuctionEnd),
                ReservePrice = reply.Auction.ReservePrice,
                Seller = reply.Auction.Seller

            };

            return auction;
        }
        catch (System.Exception ex)
        {
            logger.LogError(ex, "Could not call grpc server");
            return null;
        }
    }
}

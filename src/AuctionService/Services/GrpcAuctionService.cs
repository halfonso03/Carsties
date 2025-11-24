using System;
using AuctionService.Data;
using Grpc.Core;
using Microsoft.CodeAnalysis;

namespace AuctionService.Services;

public class GrpcAuctionService(AuctionDbContext dbContext) : GrpcAuction.GrpcAuctionBase
{
    public override async Task<GrpcAuctionResponse> GetAuction(GetAuctionRequest request, ServerCallContext context)
    {
        Console.WriteLine("===> Recived the grpc request for auction");

        var auction = await dbContext.Auctions.FindAsync(Guid.Parse(request.Id))
                        ?? throw new RpcException(new Status(StatusCode.NotFound, "Not Found"));

        if (auction == null) Console.WriteLine("===> Auction not found");
        if (auction != null) Console.WriteLine("===> Auction found");

        var response = new GrpcAuctionResponse
        {
            Auction = new GrpcAuctionModel
            {
                AuctionEnd = auction.AuctionEnd.ToString(),
                Id = auction.Id.ToString(),
                ReservePrice = auction.ReservePrice,
                Seller = auction.Seller
            }
        };

        return response;
    }
}

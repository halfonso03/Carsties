using AuctionService.Entities;
using Microsoft.AspNetCore.Mvc;

namespace AuctionService.UnitTests;

public class AuctionEntityTests
{
    /// <summary>
    /// MethodName_Scenario_ExpectedResult
    /// </summary>

    [Fact]
    public void HasReservePrice_ReservePriceGtZero_True()
    {
        // arrange

        var auction = new Auction
        {
            Id = Guid.NewGuid(),
            ReservePrice = 10
        };

        //act

        var result = auction.HasReservcePrice();


        //assert

        Assert.True(result);
    }

    
    [Fact]
    public void HasReservePrice_ReservePriceGtZero_False()
    {
        // arrange

        var auction = new Auction
        {
            Id = Guid.NewGuid(),
            ReservePrice = 0
        };

        //act

        var result = auction.HasReservcePrice();


        //assert

        Assert.False(result);
    }
}

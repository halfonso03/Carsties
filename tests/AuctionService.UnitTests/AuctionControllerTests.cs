using System;
using System.Net.Http.Headers;
using AuctionService.Controllers;
using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.RequestHelpers;
using AutoFixture;
using AutoMapper;
using Castle.Core.Logging;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using AuctionService.Entities;
using AuctionService.UnitTests.Utils;
using Microsoft.AspNetCore.Http;

namespace AuctionService.UnitTests;

public class AuctionControllerTests
{

    // MethodName_Scenario_ExpectedResult

    private readonly Mock<IAuctionRepository> _auctionRepo;
    private readonly Mock<IPublishEndpoint> _publishEndpoint;
    private readonly Fixture _fixture;
    private readonly AuctionsController _auctionsController;
    private readonly IMapper _mapper;

    public AuctionControllerTests()
    {
        _fixture = new Fixture();
        _auctionRepo = new Mock<IAuctionRepository>();
        _publishEndpoint = new Mock<IPublishEndpoint>();


       var mockMapper = new MapperConfiguration(mc =>
        {
            mc.AddMaps(typeof(MappingProfiles).Assembly);
        }).CreateMapper().ConfigurationProvider;

        _mapper = new Mapper(mockMapper);

        _auctionsController = new AuctionsController(_auctionRepo.Object, _mapper, _publishEndpoint.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = Helpers.GetClaimsPrincipal() }
            }
        };
    }

    [Fact]
    public async Task GetAuctions_WithNoParams_Returns10Auctions()
    {
        // Given
        var auctions = _fixture.CreateMany<AuctionDto>(10).ToList();
        _auctionRepo.Setup(x => x.GetAuctionsAsync(null)).ReturnsAsync(auctions);


        // When
        var result = await _auctionsController.GetAllAuctions(null);

        // Then
        Assert.Equal(10, result.Value.Count);
        Assert.IsType<ActionResult<List<AuctionDto>>>(result);

    }

    [Fact]
    public async Task GetAuctionbyId_WithValidGuid_ReturnsAuction()
    {
        // Given
        var auction = _fixture.Create<AuctionDto>();
        _auctionRepo.Setup(x => x.GetAuctionByIdAsync(It.IsAny<Guid>())).ReturnsAsync(auction);


        // When
        var result = await _auctionsController.GetAuctionById(auction.Id);

        // Then
        Assert.Equal(auction.Make, result.Value.Make);
        Assert.IsType<ActionResult<AuctionDto>>(result);

    }

    [Fact]
    public async Task GetAuctionbyId_WithInvalidGuid_ReturnsAuction()
    {
        // Given
        _auctionRepo.Setup(x => x.GetAuctionByIdAsync(It.IsAny<Guid>())).ReturnsAsync(value: null);


        // When
        ActionResult<AuctionDto> result = await _auctionsController.GetAuctionById(Guid.NewGuid());

        // Then
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task CreateAuction_WithValidCreateAuctionDto_ReturnsCreatedAtActionResult()
    {
        // Given
        var auction = _fixture.Create<CreateAuctionDto>();
        _auctionRepo.Setup(repo => repo.AddAuction(It.IsAny<Auction>()));
        _auctionRepo.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(true);


        // When
        var result = await _auctionsController.CreateAuction(auction);
        var createdResult = result.Result as CreatedAtActionResult;

        // Then
        Assert.NotNull(result);
        Assert.Equal("GetAuctionById", createdResult.ActionName);
        Assert.IsType<AuctionDto>(createdResult.Value);
    }

    [Fact]
    public async Task CreateAuction_WithFalseResult_ReturnsBadRequestResult()
    {
        // Given
        var auction = _fixture.Create<CreateAuctionDto>();
        _auctionRepo.Setup(repo => repo.AddAuction(It.IsAny<Auction>()));
        _auctionRepo.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(false);


        // When
        var result = await _auctionsController.CreateAuction(auction);
        var badRequestResult = result.Result as BadRequestObjectResult;

        // Then
        Assert.IsType<BadRequestObjectResult>(badRequestResult);
    }

    [Fact]
    public async Task UpdateAuction_WithInvalidGuid_ReturnsNotFoundResult()
    {
        // Given
        var auction = _fixture.Create<UpdateAuctionDto>();
        _auctionRepo.Setup(repo => repo.GetAunctionEntityById(It.IsAny<Guid>())).ReturnsAsync(value: null);

        // When
        var result = await _auctionsController.UpdateAuction(Guid.NewGuid(), auction);
        var notFoundResult = result as NotFoundResult;

        // Then
        Assert.IsType<NotFoundResult>(notFoundResult);
    }


    [Fact]
    public async Task UpdateAuction_WithSellerNotMatchingLoggedInUser_ReturnsForbidResult()
    {
        // Given
        var auction = _fixture.Build<Auction>()
            .With(x => x.Item, null as Item)
            .Create();
        auction.Seller = "invalid-seller";

        var auctionToUpdate = _fixture.Create<UpdateAuctionDto>();

        _auctionRepo.Setup(repo => repo.GetAunctionEntityById(It.IsAny<Guid>())).ReturnsAsync(auction);

        // When
        var result = await _auctionsController.UpdateAuction(Guid.NewGuid(), auctionToUpdate);
        var forbidResult = result as ForbidResult;

        // Then
        Assert.IsType<ForbidResult>(forbidResult);
    }


    [Fact]
    public async Task UpdateAuction_WithFalseResult_ReturnsBadRequestResult()
    {
        // Given
        var auction = _fixture.Build<Auction>()
            .With(x => x.Item, new Item())
            .Create();
        auction.Seller = Helpers.GetClaimsPrincipal().Identity.Name;
        var auctionToUpdate = _fixture.Create<UpdateAuctionDto>();

        _auctionRepo.Setup(repo => repo.GetAunctionEntityById(It.IsAny<Guid>())).ReturnsAsync(auction);
        _auctionRepo.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(false);

        // When
        var result = await _auctionsController.UpdateAuction(Guid.NewGuid(), auctionToUpdate);
        var badRequestResult = result as BadRequestObjectResult;

        // Then
        Assert.IsType<BadRequestObjectResult>(badRequestResult);
    }

    [Fact]
    public async Task UpdateAuction_WithTrueResult_ReturnsOkResult()
    {
        // Given
        var auction = _fixture.Build<Auction>()
            .With(x => x.Item, new Item())
            .Create();
        auction.Seller = Helpers.GetClaimsPrincipal().Identity.Name;
        var auctionToUpdate = _fixture.Create<UpdateAuctionDto>();

        _auctionRepo.Setup(repo => repo.GetAunctionEntityById(It.IsAny<Guid>())).ReturnsAsync(auction);
        _auctionRepo.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(true);

        // When
        var result = await _auctionsController.UpdateAuction(Guid.NewGuid(), auctionToUpdate);
        var okResult = result as OkResult;

        // Then
        Assert.IsType<OkResult>(okResult);
    }
    
    
    
}
